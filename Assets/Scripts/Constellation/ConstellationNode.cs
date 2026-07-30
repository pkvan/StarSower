using UnityEngine;
using StarSower.FX;

namespace StarSower.Constellations
{
    // Mot ngoi sao trong chom, dung trong KHONG GIAN THE GIOI (SpriteRenderer, khong phai UI).
    // Nho vay StarFlyAnimator bay thang toi day duoc ma khong phai quy doi toa do man hinh.
    //
    // Ba trang thai:
    //   Toi     — chua khoi phuc, chi la mot cham mo trong nen troi.
    //   Sang    — da khoi phuc tu truoc, sang san ngay tu dau, khong dien lai.
    //   Vua sang— khoi phuc trong luot nay: chop + vong sang + nhip tho.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ConstellationNode : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Mau")]
        [SerializeField] private Color darkColor = new Color(0.45f, 0.55f, 0.75f, 0.22f);
        [SerializeField] private Color litColor = new Color(1f, 0.97f, 0.85f, 1f);

        [Header("Co")]
        [SerializeField] private float darkScale = 0.55f;
        [SerializeField] private float litScale = 1f;

        [Header("Nhip tho khi da sang")]
        [Tooltip("Bien do phong to/thu nho. 0 = khong tho.")]
        [SerializeField] private float pulseAmplitude = 0.06f;
        [Min(0.1f)]
        [SerializeField] private float pulsePeriod = 2.8f;

        [Header("FX luc kich hoat")]
        [SerializeField] private float flashScale = 0.9f;
        [SerializeField] private float ringScale = 1.1f;
        [Min(0.01f)]
        [SerializeField] private float activateDuration = 0.15f;

        private Transform cachedTransform;
        private bool isLit;
        private float phase;
        private float activateTimer = -1f;
        private float glowBoost;
        private float zoomScale = 1f;

        public bool IsLit => isLit;

        // Phong to rieng ngoi sao khi ca chom duoc zoom — khong dung camera nen Hero khong bi cat.
        public void SetZoomScale(float k)
        {
            zoomScale = Mathf.Max(0.01f, k);
            if (activateTimer < 0f)
                Apply(isLit ? 1f : 0f);
        }

        // Man ket: ca chom sang bung len cung luc. 0 = binh thuong, 1 = sang nhat.
        public void SetGlowBoost(float amount)
        {
            glowBoost = Mathf.Clamp01(amount);
            if (isLit && activateTimer < 0f)
                Apply(1f);
        }

        private void Awake()
        {
            cachedTransform = transform;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            // Lech pha ngau nhien: ca chom sao khong tho dong loat nhu mot khoi den.
            phase = Random.Range(0f, 100f);
        }

        // Dat trang thai tuc thi, khong dien gi — dung luc dung canh cho cac node da khoi phuc
        // tu nhung lan choi truoc, va cho cac node con toi.
        public void SetInstant(bool lit)
        {
            isLit = lit;
            activateTimer = -1f;
            Apply(lit ? 1f : 0f);
        }

        // Khoi phuc trong luot nay: chop, vong sang, roi chuyen sang trang thai sang.
        public void Activate(StarFXPool pool)
        {
            isLit = true;
            activateTimer = 0f;

            if (pool == null)
                return;

            Vector3 p = cachedTransform.position;
            pool.Spawn(StarFXType.Flash, p, 0f, flashScale, 1f, 0.12f);
            pool.Spawn(StarFXType.Ring, p, 0f, ringScale);
        }

        private void Update()
        {
            if (activateTimer >= 0f)
            {
                activateTimer += Time.deltaTime;
                float t = Mathf.Clamp01(activateTimer / activateDuration);
                // Vot qua co dich mot chut roi lang ve — cam giac "cam vao dung cho" thay vi
                // chi don gian to dan len.
                float overshoot = Mathf.Sin(t * Mathf.PI) * 0.25f;
                Apply(t, overshoot);
                if (t >= 1f)
                    activateTimer = -1f;
                return;
            }

            if (!isLit || pulseAmplitude <= 0f)
                return;

            float breath = Mathf.Sin((Time.time + phase) * (Mathf.PI * 2f / pulsePeriod)) * pulseAmplitude;
            SetScale(litScale * (1f + glowBoost * 0.25f) * (1f + breath) * zoomScale);
        }

        private void Apply(float litAmount, float extraScale = 0f)
        {
            Color c = Color.Lerp(darkColor, litColor, litAmount);
            // Sang bung: day mau ve phia trang va no to them mot chut.
            if (glowBoost > 0f)
                c = Color.Lerp(c, Color.white, glowBoost * 0.55f);
            spriteRenderer.color = c;
            SetScale((Mathf.Lerp(darkScale, litScale, litAmount) * (1f + glowBoost * 0.25f) + extraScale) * zoomScale);
        }

        private void SetScale(float s)
        {
            cachedTransform.localScale = new Vector3(s, s, 1f);
        }
    }
}
