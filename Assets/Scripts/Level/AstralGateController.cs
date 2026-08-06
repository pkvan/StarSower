using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using StarSower.CameraSystem;
using StarSower.Collectibles;
using StarSower.Core;
using StarSower.FX;
using StarSower.Player;

namespace StarSower.Level
{
    // Astral Gate — vat the ket thuc chang, thay cho o vuong tam thoi (S2-008).
    //
    // PHAM VI: component nay CHI lo phan TRINH DIEN cua cong (hinh, quang sang, hat, tieng).
    // No khong khoa di chuyen, khong che man hinh, khong goi man chom sao, khong luu gi ca —
    // giu dung luat so 38 cua du an "Goal chi phat event". Toan bo trinh tu roi khu vuc van do
    // LevelFlowManager cam trich, y nhu truoc.
    //
    // Hai su kien duoc phat, KHONG phai mot:
    //   OnGoalReached     — ngay khi cham cong. LevelFlowManager khoa di chuyen tu day, nen nguoi
    //                       choi khong the di ra giua luc cong dang mo.
    //   OnLevelCompleted  — sau khi dien xong, va nay moi la cai khoi dong chuyen man.
    // Tach lam hai vi hai viec xay ra cach nhau vai giay; gop lam mot thi hoac nguoi choi con di
    // duoc suot doan dien, hoac man hinh toi ngay khi cong chua kip mo.
    public class AstralGateController : MonoBehaviour
    {
        // S2-012 — cong di qua 5 trang thai. Truoc day tat ca don vao mot coroutine chay tu luc
        // cham cong; tach ra vi hai NUA cua trinh tu duoc kich hoat boi hai viec khac nhau:
        //   Locked -> Awakening -> Opening  : tu chay khi nhat DU SAO, nguoi choi dung xem.
        //   Transition -> Complete          : chi chay khi nguoi choi TU BUOC vao cong.
        public enum GateState
        {
            Locked,
            Awakening,
            Opening,
            Transition,
            Complete,
        }

        public GateState State { get; private set; } = GateState.Locked;

        [Header("Nhan dien")]
        [SerializeField] private LayerMask playerLayer;

        [Header("Khoa (S2-009)")]
        [Tooltip("Cong khoa cho toi khi nhat du sao. De trong thi cong mo tu do nhu truoc.")]
        [SerializeField] private CollectibleManager collectibleManager;

        [Header("Hinh")]
        [SerializeField] private SpriteRenderer gateBody;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite openSprite;

        [Tooltip("Vong hao quang. Tat luc dau, bat o State 2 roi quay cham.")]
        [SerializeField] private SpriteRenderer aura;

        [Tooltip("Toc do quay cua hao quang, do moi giay.")]
        [SerializeField] private float auraRotationSpeed = 20f;

        [Header("Anh sang")]
        [SerializeField] private Light2D gateLight;

        [Tooltip("Cuong do khi hao quang da bat het (State 2).")]
        [SerializeField] private float auraLightIntensity = 1.2f;

        [Tooltip("Cuong do khi cong da mo (State 3). Phai sang hon State 2.")]
        [SerializeField] private float openLightIntensity = 2.2f;

        [Header("Hat")]
        [Tooltip("De trong thi tu tim trong scene. Khong co pool thi cong van chay, chi thieu hat.")]
        [SerializeField] private StarFXPool pool;

        [Tooltip("Goc phat hat. De trong thi phat quanh chinh cong.")]
        [SerializeField] private Transform fxOrigin;

        [Tooltip("Ban kinh rai hat quanh goc phat (world units).")]
        [SerializeField] private float fxRadius = 1.1f;

        [Tooltip("So hat moi giay o State 2.")]
        [SerializeField] private float auraFxRate = 14f;

        [Tooltip("So hat moi giay o State 3 va State 4 — day hon cho ra cam giac cong dang mo.")]
        [SerializeField] private float openFxRate = 26f;

        [Tooltip("Cong phinh ra mot nhip luc mo. 0 = khong phinh (mac dinh — phinh lam cong " +
                 "trong nhu bi day ra, nguoc voi cam giac hut vao).")]
        [SerializeField] private float openScalePunch;

        [Tooltip("So chum sang trang nha ra moi giay ngay giua cong. Cac chum nay cong sang chong " +
                 "len nhau nen loi giua don ve trang, du tung manh von mau vang am.")]
        [Min(0f)]
        [SerializeField] private float whiteCoreRate = 18f;

        [Tooltip("Toc do hat bi keo VE PHIA cong. 0 = hat bay len nhu cu, khong co cam giac hut.")]
        [SerializeField] private float suctionSpeed = 2.6f;

        [Tooltip("Hat sinh ra o vanh ngoai xa bao nhieu lan Fx Radius, roi bi hut vao giua.")]
        [Min(1f)]
        [SerializeField] private float suctionRingScale = 2.2f;

        [Tooltip("Cong tu mo di o cuoi State 4. Phai tan truoc khi sang man chom sao, neu khong " +
                 "no con nam do trong ca doan phim.")]
        [Min(0f)]
        [SerializeField] private float gateFadeDuration = 0.7f;

        [Tooltip("Hao quang quay nhanh gap may lan trong luc cong dang mo.")]
        [Min(1f)]
        [SerializeField] private float openSpinBoost = 3f;

        [Header("Nguoi choi tan vao cong (S2-012)")]
        [Tooltip("Cho bao lau sau khi nhat ngoi sao cuoi cung roi cong moi thuc day.")]
        [Min(0f)]
        [SerializeField] private float awakenDelay = 0.25f;

        [Tooltip("Hero thu nho lai trong luc tan di. 1 -> gia tri nay -> 0.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float heroShrinkMid = 0.85f;

        [Tooltip("Rung camera luc cong bung mo. De trong thi bo qua.")]
        [SerializeField] private MonoBehaviour cameraShakeSource;

        [Header("Hero tan di")]
        [Tooltip("De trong thi tu tim trong scene. Khong tim thay thi bo qua, cong van chay.")]
        [SerializeField] private PlayerAnimationController heroAnimation;
        [SerializeField] private GroundShadowController heroShadow;

        [Tooltip("De trong thi tu tim. Dung de tat va cham luc Hero tan vao cong.")]
        [SerializeField] private PlayerMotor heroMotor;

        [Tooltip("Hero mo dan roi bien mat trong bao lau, tinh tu dau State 4.")]
        [Min(0f)]
        [SerializeField] private float heroFadeDuration = 1.2f;

        [Header("Nhip (giay)")]
        [Tooltip("State 2: tu luc cham cong toi luc hao quang sang het.")]
        [SerializeField] private float auraDuration = 0.5f;

        [Tooltip("State 3: tu luc hao quang day toi luc cong mo han.")]
        [SerializeField] private float openDuration = 1f;

        [Tooltip("State 4: giu canh cong da mo truoc khi phat OnLevelCompleted.")]
        [SerializeField] private float cinematicDelay = 2.2f;

        [Header("Tieng (tuy chon)")]
        [Tooltip("Thieu clip thi cong van chay im lang — khong bao gio duoc chan luong choi.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip auraClip;
        [SerializeField] private AudioClip openClip;

        [Tooltip("Tieng luc chum sang bung ra. Thieu clip thi im lang, khong bao gio chan luong choi.")]
        [SerializeField] private AudioClip burstClip;
        [SerializeField] private AudioClip transitionClip;

        private ICameraShake cameraShake;
        private Transform heroTransform;
        private Vector3 heroBaseScale = Vector3.one;
        private bool triggered;
        private bool unlocked = true;
        private bool playerInside;
        private Collider2D waitingPlayer;
        private float fxAccumulator;
        private float coreAccumulator;

        private void Awake()
        {
            if (pool == null)
                pool = FindFirstObjectByType<StarFXPool>();

            if (fxOrigin == null)
                fxOrigin = transform;

            if (heroAnimation == null)
                heroAnimation = FindFirstObjectByType<PlayerAnimationController>();

            if (heroShadow == null)
                heroShadow = FindFirstObjectByType<GroundShadowController>();

            if (heroMotor == null)
                heroMotor = FindFirstObjectByType<PlayerMotor>();

            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();

            cameraShake = cameraShakeSource as ICameraShake;
            if (cameraShake == null)
                cameraShake = FindFirstObjectByType<CameraShake>();

            if (heroAnimation != null)
            {
                heroTransform = heroAnimation.transform;
                heroBaseScale = heroTransform.localScale;
            }

            ApplyIdleState();
        }

        // Dat ve trang thai 1 o Awake chu khong tin vao gia tri luu trong scene: mo scene ra ma
        // hao quang dang bat (do luc chinh tay quen tat) thi cong lo mat truoc khi nguoi choi toi.
        private void ApplyIdleState()
        {
            if (gateBody != null)
            {
                if (idleSprite != null)
                    gateBody.sprite = idleSprite;
                SetAlpha(gateBody, 1f);
                gateBody.transform.localScale = Vector3.one;
            }

            if (aura != null)
            {
                aura.gameObject.SetActive(false);
                aura.transform.localRotation = Quaternion.identity;
            }

            if (gateLight != null)
            {
                gateLight.intensity = 0f;
                gateLight.enabled = false;
            }

            // Mac dinh COI NHU DA KHOA khi co CollectibleManager: su kien dau tien cua no se quyet
            // dinh mo hay khong. Khong co manager thi giu nguyen luat cu, cong mo tu do.
            unlocked = collectibleManager == null;
            State = unlocked ? GateState.Opening : GateState.Locked;
        }

        // Goi tu AstralGateTrigger tren child TriggerCollider. Tach ra vi collider nam o child,
        // ma OnTriggerEnter2D chi den duoc GameObject dang mang collider.
        private void OnEnable()
        {
            if (collectibleManager != null)
                collectibleManager.OnCollectedChanged += HandleCollectedChanged;
        }

        private void OnDisable()
        {
            if (collectibleManager != null)
                collectibleManager.OnCollectedChanged -= HandleCollectedChanged;
        }

        // CollectibleManager ban su kien nay ca luc khoi tao (0/N) lan moi lan nhat, nen cong tu
        // khoa minh o lan ban dau tien — khong can goi Lock() rieng o dau ca.
        private void HandleCollectedChanged(int collected, int total)
        {
            if (total > 0 && collected >= total)
                Unlock();
            else
                Lock();
        }

        public void Lock()
        {
            if (!unlocked)
                return;

            unlocked = false;
            State = GateState.Locked;

            // Vung cham VAN BAT — nguoi choi phai cham duoc vao cong de biet no dang khoa. Chi
            // rieng phan trinh dien la tat.
            if (aura != null)
                aura.gameObject.SetActive(false);

            if (gateLight != null)
                gateLight.enabled = false;
        }

        public void Unlock()
        {
            if (unlocked)
                return;

            unlocked = true;
            StartCoroutine(AwakenRoutine());
        }

        // Nua DAU cua trinh tu: chay ngay khi nhat du sao, khong doi nguoi choi cham vao. Cong
        // sang len va mo ra ngay tai cho de nguoi choi THAY duoc no vua mo khoa.
        private IEnumerator AwakenRoutine()
        {
            State = GateState.Awakening;
            if (awakenDelay > 0f)
                yield return new WaitForSeconds(awakenDelay);

            yield return AuraPhase();

            State = GateState.Opening;
            yield return OpenPhase();

            // Cong da mo va dung yen cho. Nguoi choi co the dang dung san trong vung cham — luc do
            // OnTriggerEnter2D khong ban lai nua nen phai tu goi.
            if (playerInside && waitingPlayer != null)
                NotifyTriggerEnter(waitingPlayer);
        }

        public void NotifyTriggerExit(Collider2D other)
        {
            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            playerInside = false;
            waitingPlayer = null;
        }

        public void NotifyTriggerEnter(Collider2D other)
        {
            if (triggered)
                return;

            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            playerInside = true;
            waitingPlayer = other;

            // Chua du sao thi cham vao khong lam gi ca. KHONG dat triggered: con phai mo duoc khi
            // nhat not so sao con lai.
            if (!unlocked)
                return;

            // Cong chua mo xong thi chua di qua duoc. Khong dat triggered: con phai vao duoc khi
            // AwakenRoutine chay xong.
            if (State != GateState.Opening && State != GateState.Transition)
                return;

            triggered = true;
            GameEvents.RaiseGoalReached();
            StartCoroutine(GateRoutine());
        }

        // Nua SAU: nguoi choi da tu buoc vao cong.
        private IEnumerator GateRoutine()
        {
            State = GateState.Transition;
            Play(transitionClip);

            // Tat va cham qua chinh API cua PlayerMotor — no van la noi duy nhat dung toi
            // Rigidbody2D, dung quy tac Single-Writer cua du an. LevelFlowManager da khoa di
            // chuyen tu su kien OnGoalReached, nen o day chi con phan va cham.
            heroMotor?.SetPhysicsActive(false);

            yield return HoldPhase();

            State = GateState.Complete;
            GameEvents.RaiseLevelCompleted();

            // Tra co Hero ve nhu cu NGAY o day. Luc nay do mo cua Hero dang bang 0 nen khong ai
            // thay no bat lai, va canh chom sao chi phai lo phan do mo — khong dong gi vao he
            // cinematic. Tra muon hon thi Hero chay vao voi co bang 0, tuc vo hinh.
            if (heroTransform != null)
                heroTransform.localScale = heroBaseScale;
        }

        // State 2 — hao quang hien va sang dan, den sang dan theo.
        private IEnumerator AuraPhase()
        {
            if (aura != null)
                aura.gameObject.SetActive(true);

            if (gateLight != null)
                gateLight.enabled = true;

            Play(auraClip);

            float t = 0f;
            while (t < auraDuration)
            {
                t += Time.deltaTime;
                float k = auraDuration > 0f ? Mathf.Clamp01(t / auraDuration) : 1f;

                SetAuraAlpha(k);
                SetLight(Mathf.Lerp(0f, auraLightIntensity, k));
                SpinAura();
                EmitFx(auraFxRate, k);
                yield return null;
            }

            SetAuraAlpha(1f);
            SetLight(auraLightIntensity);
        }

        // State 3 — doi sang hinh cong da mo, day den sang hon.
        private IEnumerator OpenPhase()
        {
            if (gateBody != null && openSprite != null)
                gateBody.sprite = openSprite;

            Play(openClip);

            // Chum sang luc cong bung mo. Toan bo do sang den tu cac manh FX cong sang cua pool
            // chu khong tu Light2D — den 2D lam lech mau ca khung hinh nen da bo han.
            if (pool != null)
            {
                Vector3 p = fxOrigin.position;
                pool.Spawn(StarFXType.Flash, p, 0f, 2.6f, 1f, 0.35f);
                pool.Spawn(StarFXType.Ring, p, 0f, 1.6f);
                pool.Spawn(StarFXType.Ring, p, 0f, 2.4f, 0.7f);
                pool.Spawn(StarFXType.PocketBurst, p, 0f, 1.4f);
                pool.Spawn(StarFXType.PocketGlow, p, 0f, 2.2f, 0.8f);
            }

            // Rung MOT lan luc cong bung mo, dung cap MEDIUM cua CameraShake thay vi tu bia mot
            // cap so rieng — do manh cua rung la chuyen cam giac chung cua game.
            (cameraShake as CameraShake)?.ShakeMedium();
            Play(burstClip);

            Vector3 bodyBase = gateBody != null ? gateBody.transform.localScale : Vector3.one;
            Vector3 auraBase = aura != null ? aura.transform.localScale : Vector3.one;

            float t = 0f;
            while (t < openDuration)
            {
                t += Time.deltaTime;
                float k = openDuration > 0f ? Mathf.Clamp01(t / openDuration) : 1f;

                // Nhun mot nhip roi lang ve: sin(pi*k) len 1 o giua roi ve 0, nen cong khong bi
                // ket o co lon sau khi mo.
                float punch = 1f + Mathf.Sin(k * Mathf.PI) * openScalePunch;
                if (gateBody != null) gateBody.transform.localScale = bodyBase * punch;

                // Hao quang CO LAI chu khong no ra: co lai doc ra la dang hut vao, no ra doc ra la
                // dang day ra — dung cam giac nguoc han nhau.
                if (aura != null) aura.transform.localScale = auraBase * Mathf.Lerp(1f, 0.82f, k);

                SetLight(Mathf.Lerp(auraLightIntensity, openLightIntensity, k));
                SpinAura(openSpinBoost);
                EmitFx(openFxRate, 1f, suction: true);
                EmitWhiteCore(k);
                yield return null;
            }

            if (gateBody != null) gateBody.transform.localScale = bodyBase;
            SetLight(openLightIntensity);
        }

        // State 4 — giu canh cong da mo. Man hinh se do LevelFlowManager che sau khi
        // OnLevelCompleted phat ra o cuoi quang nay.
        private IEnumerator HoldPhase()
        {
            float t = 0f;
            while (t < cinematicDelay)
            {
                t += Time.deltaTime;

                // Hero tan vao cong. Ghi qua PlayerAnimationController / GroundShadowController chu
                // khong dong thang vao SpriteRenderer — hai component do moi la chu cua mau.
                if (heroFadeDuration > 0f)
                {
                    float k = Mathf.Clamp01(t / heroFadeDuration);
                    heroAnimation?.SetVisualAlpha(1f - k);
                    heroShadow?.SetFadeMultiplier(1f - k);

                    // 1 -> 0.85 -> 0: thu nhe o nua dau roi hut han ve 0. Thu thang tu 1 xuong 0
                    // doc ra la bi bop bep; chung lai mot chut truoc roi moi bien mat thi giong
                    // dang bi cong hut vao hon.
                    float scale = k < 0.5f
                        ? Mathf.Lerp(1f, heroShrinkMid, k * 2f)
                        : Mathf.Lerp(heroShrinkMid, 0f, (k - 0.5f) * 2f);
                    if (heroTransform != null)
                        heroTransform.localScale = heroBaseScale * scale;
                }

                // Cong tan di o CUOI quang giu, sau khi da hut xong nhan vat. Tinh nguoc tu diem
                // ket thuc chu khong tu diem bat dau: doi cinematicDelay thi cong van tan dung luc
                // man hinh sap che, khong phai chinh lai tay.
                if (gateFadeDuration > 0f)
                {
                    float left = cinematicDelay - t;
                    float g = Mathf.Clamp01(left / gateFadeDuration);
                    SetAlpha(gateBody, g);
                    SetAuraAlpha(g);
                }

                SpinAura(openSpinBoost);
                EmitFx(openFxRate, 1f, suction: true);
                EmitWhiteCore(1f);
                yield return null;
            }

            heroAnimation?.SetVisualAlpha(0f);
            heroShadow?.SetFadeMultiplier(0f);
            if (heroTransform != null)
                heroTransform.localScale = Vector3.zero;
            SetAlpha(gateBody, 0f);
            SetAuraAlpha(0f);
        }

        private void SpinAura(float boost = 1f)
        {
            if (aura == null)
                return;

            aura.transform.Rotate(0f, 0f, -auraRotationSpeed * boost * Time.deltaTime);
        }

        private void SetAuraAlpha(float a)
        {
            if (aura == null)
                return;

            Color c = aura.color;
            c.a = Mathf.Clamp01(a);
            aura.color = c;
        }

        private void SetLight(float intensity)
        {
            if (gateLight != null)
                gateLight.intensity = intensity;
        }

        // Bo tich luy nhu StarFlyAnimator: mot khung hinh co the nha 0, 1 hay nhieu hat tuy do dai
        // khung, nen mat do khong doi theo framerate.
        private void EmitFx(float rate, float intensity, bool suction = false)
        {
            if (pool == null || rate <= 0f)
                return;

            fxAccumulator += Time.deltaTime * rate * Mathf.Clamp01(intensity);
            while (fxAccumulator >= 1f)
            {
                fxAccumulator -= 1f;

                Vector2 offset;
                Vector2 drift;
                if (suction)
                {
                    // Sinh o VANH NGOAI roi keo thang ve tam: mat doc ra la dong hat dang bi hut
                    // vao trong. Sinh o giua roi keo vao thi khong thay duong di, mat het y nghia.
                    Vector2 dir = Random.insideUnitCircle.normalized;
                    if (dir == Vector2.zero) dir = Vector2.up;
                    offset = dir * (fxRadius * Random.Range(1f, suctionRingScale));
                    drift = -dir * suctionSpeed;
                }
                else
                {
                    offset = Random.insideUnitCircle * fxRadius;
                    drift = Vector2.up * Random.Range(0.2f, 0.7f);
                }
                Vector3 p = fxOrigin.position + new Vector3(offset.x, offset.y, 0f);

                // Xen ke bui va lap lanh: bui lam day khong gian, lap lanh tao diem nhan.
                bool sparkle = Random.value < 0.45f;
                StarFXType type = sparkle
                    ? (StarFXType)((int)StarFXType.Sparkle01 + Random.Range(0, 3))
                    : (StarFXType)((int)StarFXType.Dust01 + Random.Range(0, 3));

                pool.Spawn(type, p, Random.Range(0f, 360f), Random.Range(0.7f, 1.2f),
                           sparkle ? 1f : Random.Range(0.2f, 0.45f), 0f, drift);
            }
        }

        // Chum sang giua cong. Tung manh Flash von mau vang am (1.00, 0.95, 0.67), nhung material
        // la cong sang nen vai manh chong len nhau se day R va G cham tran truoc, B duoi len sau —
        // ket qua loi giua don ve TRANG. Do la cach duy nhat ra duoc mau trang o day, vi
        // SpriteRenderer.color khong the sang hon trang.
        private void EmitWhiteCore(float intensity)
        {
            if (pool == null || whiteCoreRate <= 0f)
                return;

            coreAccumulator += Time.deltaTime * whiteCoreRate * Mathf.Clamp01(intensity);
            while (coreAccumulator >= 1f)
            {
                coreAccumulator -= 1f;

                Vector3 jitter = new Vector3(Random.Range(-0.12f, 0.12f), Random.Range(-0.12f, 0.12f), 0f);
                pool.Spawn(StarFXType.Flash, fxOrigin.position + jitter, Random.Range(0f, 360f),
                           Random.Range(0.9f, 1.5f), 0.85f, 0.22f);
            }
        }

        private static void SetAlpha(SpriteRenderer sr, float a)
        {
            if (sr == null)
                return;

            Color c = sr.color;
            c.a = Mathf.Clamp01(a);
            sr.color = c;
        }

        private void Play(AudioClip clip)
        {
            if (audioSource == null || clip == null)
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}
