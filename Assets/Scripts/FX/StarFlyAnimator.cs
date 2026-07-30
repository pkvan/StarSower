using UnityEngine;

namespace StarSower.FX
{
    // Dua Fly Core tu cho manh sao bay ve tui cua Hero theo duong Bezier bac hai, vua bay vua
    // nha duoi/bui/lap lanh lay tu pool.
    //
    // Diem dau va diem dieu khien CHOT MOT LAN luc bat dau; diem den thi doc lai MOI FRAME tu
    // Transform cua tui. Nho vay Hero cu chay tiep trong luc sao dang bay ma sao van dap dung cho,
    // con duong cong thi khong bi vặn theo tung buoc chan.
    [RequireComponent(typeof(PooledStarFX))]
    public class StarFlyAnimator : MonoBehaviour
    {
        [Header("Bay")]
        [Min(0.05f)]
        [SerializeField] private float durationMin = 0.35f;
        [Min(0.05f)]
        [SerializeField] private float durationMax = 0.45f;

        [Tooltip("Do cao vong cung cong vao diem dieu khien (world units). Nho thoi — vong cung " +
                 "lon se thanh hoat hinh, khong con diem dam.")]
        [SerializeField] private float arcHeight = 0.9f;

        [Tooltip("Lech ngang ngau nhien cua diem dieu khien, +/- gia tri nay.")]
        [Min(0f)]
        [SerializeField] private float arcHorizontalJitter = 0.25f;

        [Header("Huong sprite")]
        [Tooltip("Xoay Fly Core theo huong bay. BAT BUOC bat voi anh sao choi co duoi ve san — " +
                 "khong xoay thi duoi chia mot dang con nguoi bay mot neo.")]
        [SerializeField] private bool orientToVelocity = true;

        [Tooltip("Goc ma 'dau' cua sprite dang chi khi chua xoay (do). Star_Fly_Core ve sao o goc " +
                 "tren-phai, duoi keo ve duoi-trai, nen dau chi huong 45 do.")]
        [SerializeField] private float artForwardAngle = 45f;

        [Header("Duoi")]
        [Min(0f)]
        [SerializeField] private float trailRateMin = 15f;
        [Min(0f)]
        [SerializeField] private float trailRateMax = 25f;
        [SerializeField] private float trailScaleMin = 0.85f;
        [SerializeField] private float trailScaleMax = 1.15f;
        [SerializeField] private float trailRotationRange = 15f;
        [Min(0.01f)]
        [SerializeField] private float trailLifetime = 0.22f;
        [Tooltip("Toc do troi nguoc huong bay cua manh duoi.")]
        [SerializeField] private float trailBackDrift = 0.8f;

        [Header("Bui")]
        [Min(0f)]
        [SerializeField] private float dustRate = 6f;
        [Range(0f, 1f)]
        [SerializeField] private float dustAlphaMin = 0.12f;
        [Range(0f, 1f)]
        [SerializeField] private float dustAlphaMax = 0.28f;
        [Min(0.01f)]
        [SerializeField] private float dustLifetime = 0.5f;
        [SerializeField] private float dustDriftSpeed = 0.35f;
        [SerializeField] private float dustOffsetRadius = 0.12f;

        [Header("Lap lanh")]
        [Min(0.01f)]
        [SerializeField] private float sparkleIntervalMin = 0.05f;
        [Min(0.01f)]
        [SerializeField] private float sparkleIntervalMax = 0.1f;
        [SerializeField] private float sparkleScaleMin = 0.6f;
        [SerializeField] private float sparkleScaleMax = 1f;
        [SerializeField] private float sparkleOffsetRadius = 0.18f;

        private PooledStarFX pooled;
        private Transform cachedTransform;
        private StarFXPool pool;
        private IStarFlightListener listener;

        // Ben goi dat truoc khi bay, ben nghe doc lai luc sao toi noi de biet day la
        // ngoi sao thu may. Khong cap phat gi, khong can Dictionary.
        public int Tag { get; set; }

        // Ghi de thoi luong bay cho MOT lan bay. <= 0 la dung khoang mac dinh cua prefab.
        // Them rieng thay vi sua durationMin/Max: prefab Fly Core dung chung voi luc nhat sao,
        // doi thang tren prefab se lam cham luon ca hieu ung nhat sao ngoai man choi.
        private float durationOverride;

        public void SetDurationOverride(float seconds)
        {
            durationOverride = seconds;
        }
        private Transform destination;

        private Vector3 start;
        private Vector3 control;
        private Vector3 lastDestination;
        private Vector3 previousPosition;

        private float duration;
        private float elapsed;
        private bool flying;

        private float trailRate;
        private float trailAccumulator;
        private float dustAccumulator;
        private float sparkleTimer;
        private float sparkleInterval;

        private void Awake()
        {
            pooled = GetComponent<PooledStarFX>();
            cachedTransform = transform;
        }

        public void Begin(StarFXPool fxPool, Vector3 startPosition, Transform pouch,
                          Vector3 fallbackDestination, IStarFlightListener owner)
        {
            pool = fxPool;
            listener = owner;
            destination = pouch;
            start = startPosition;
            previousPosition = startPosition;
            lastDestination = pouch != null ? pouch.position : fallbackDestination;

            Vector3 mid = (start + lastDestination) * 0.5f;
            control = mid + Vector3.up * arcHeight
                      + Vector3.right * Random.Range(-arcHorizontalJitter, arcHorizontalJitter);

            duration = durationOverride > 0f ? durationOverride : Random.Range(durationMin, durationMax);
            durationOverride = 0f;
            elapsed = 0f;
            flying = true;

            trailRate = Random.Range(trailRateMin, trailRateMax);
            trailAccumulator = 0f;
            dustAccumulator = 0f;
            sparkleTimer = 0f;
            sparkleInterval = Random.Range(sparkleIntervalMin, sparkleIntervalMax);

            cachedTransform.position = start;
        }

        private void Update()
        {
            if (!flying)
                return;

            float dt = Time.deltaTime;
            elapsed += dt;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

            // Doc lai diem den moi frame, nhung chi khi no con dung — Hero co the bi tat giua
            // chung (chuyen man, chet). Luc do giu diem cuoi cung biet duoc va van bay cho xong.
            if (destination != null && destination.gameObject.activeInHierarchy)
                lastDestination = destination.position;

            float eased = t * t * (3f - 2f * t);
            Vector3 position = Bezier(start, control, lastDestination, eased);
            cachedTransform.position = position;

            Vector3 delta = position - previousPosition;
            previousPosition = position;

            // Quay dau sao ve huong dang bay. Tru artForwardAngle vi anh von da ve nghieng san;
            // bo qua khi buoc di chuyen qua nho, neu khong goc se giat lung tung luc gan dung yen.
            if (orientToVelocity && delta.sqrMagnitude > 1e-8f)
            {
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - artForwardAngle;
                cachedTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            EmitTrail(dt, position, delta);
            EmitDust(dt, position);
            EmitSparkle(dt, position);

            if (t >= 1f)
                Finish();
        }

        private static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        // Bo tich luy: mot lan Update co the nha 0, 1 hay nhieu manh tuy khung hinh dai ngan, nen
        // mat do duoi khong doi theo framerate. Khong coroutine cho tung hat.
        private void EmitTrail(float dt, Vector3 position, Vector3 delta)
        {
            if (pool == null || trailRate <= 0f)
                return;

            trailAccumulator += dt * trailRate;
            while (trailAccumulator >= 1f)
            {
                trailAccumulator -= 1f;

                Vector2 back = delta.sqrMagnitude > 1e-6f
                    ? -(Vector2)delta.normalized * trailBackDrift
                    : Vector2.zero;

                pool.Spawn((StarFXType)((int)StarFXType.Trail01 + Random.Range(0, 3)),
                           position,
                           Random.Range(-trailRotationRange, trailRotationRange),
                           Random.Range(trailScaleMin, trailScaleMax),
                           1f, trailLifetime, back);
            }
        }

        private void EmitDust(float dt, Vector3 position)
        {
            if (pool == null || dustRate <= 0f)
                return;

            dustAccumulator += dt * dustRate;
            while (dustAccumulator >= 1f)
            {
                dustAccumulator -= 1f;

                Vector3 offset = new Vector3(Random.Range(-dustOffsetRadius, dustOffsetRadius),
                                             Random.Range(-dustOffsetRadius, dustOffsetRadius), 0f);
                Vector2 slow = Random.insideUnitCircle * dustDriftSpeed;

                pool.Spawn((StarFXType)((int)StarFXType.Dust01 + Random.Range(0, 3)),
                           position + offset,
                           Random.Range(0f, 360f),
                           Random.Range(0.7f, 1.1f),
                           Random.Range(dustAlphaMin, dustAlphaMax),
                           dustLifetime, slow);
            }
        }

        // Bo dem voi khoang cach BOC LAI NGAU NHIEN sau moi lan nha: neu de khoang co dinh thi mat
        // se doc ra nhip lap deu dan, mat het cam giac tu nhien.
        private void EmitSparkle(float dt, Vector3 position)
        {
            if (pool == null)
                return;

            sparkleTimer += dt;
            if (sparkleTimer < sparkleInterval)
                return;

            sparkleTimer = 0f;
            sparkleInterval = Random.Range(sparkleIntervalMin, sparkleIntervalMax);

            Vector3 offset = new Vector3(Random.Range(-sparkleOffsetRadius, sparkleOffsetRadius),
                                         Random.Range(-sparkleOffsetRadius, sparkleOffsetRadius), 0f);

            pool.Spawn((StarFXType)((int)StarFXType.Sparkle01 + Random.Range(0, 3)),
                       position + offset,
                       Random.Range(0f, 360f),
                       Random.Range(sparkleScaleMin, sparkleScaleMax));
        }

        private void Finish()
        {
            flying = false;

            // Bao TRUOC khi tra ve pool: tra truoc roi bao thi doi tuong co the da bi lay lai cho
            // luot nhat khac va trang thai bi giam len nhau.
            IStarFlightListener target = listener;
            listener = null;
            Vector3 arrival = lastDestination;

            pooled.ReturnNow();

            if (target != null)
                target.OnStarFlightArrived(this, arrival);
        }

        // Huy giua chung (scene dong, pool bi tat). Van bao den cho ben goi con trao thuong duoc.
        private void OnDisable()
        {
            if (!flying)
                return;

            flying = false;
            IStarFlightListener target = listener;
            listener = null;
            if (target != null)
                target.OnStarFlightArrived(this, lastDestination);
        }
    }
}
