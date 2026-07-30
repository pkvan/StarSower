using UnityEngine;

namespace StarSower.FX
{
    // Mot manh FX dung lai duoc: song mot khoang, phong to/thu nho, mo dan, xoay, troi nhe, roi tu
    // tra minh ve pool. MOT script nay dung chung cho ca flash/burst/trail/dust/sparkle/pocket —
    // khong lam moi sprite mot MonoBehaviour rieng.
    //
    // Moi thu chay bang Time.deltaTime (co scale) nen FX dung theo game khi tam dung, dung nhu
    // phan con lai cua StarSower.
    [RequireComponent(typeof(SpriteRenderer))]
    public class PooledStarFX : MonoBehaviour
    {
        [Header("Thanh phan")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Vong doi")]
        [Min(0.01f)]
        [Tooltip("Thoi gian song mac dinh (giay). Ben goi Play() co the ghi de tung lan.")]
        [SerializeField] private float lifetime = 0.25f;

        [Tooltip("Tat khi co thanh phan khac tu dieu khien vong doi (vd Fly Core do StarFlyAnimator " +
                 "lai). Luc do phai goi ReturnNow() bang tay.")]
        [SerializeField] private bool autoReturn = true;

        [Header("Bien doi")]
        [Min(0f)]
        [SerializeField] private float startScale = 0.8f;
        [Min(0f)]
        [SerializeField] private float endScale = 1.2f;

        [Range(0f, 1f)]
        [SerializeField] private float startAlpha = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float endAlpha;

        [Tooltip("Do/giay. 0 = khong xoay.")]
        [SerializeField] private float rotationSpeed;

        [Tooltip("Van toc troi rieng cua manh FX (world units/giay). Ben goi co the cong them.")]
        [SerializeField] private Vector2 drift;

        // --- trang thai chay
        private StarFXPool owner;
        private StarFXType type;
        private bool isPlaying;
        private bool isReturned = true;      // luc chua Play thi coi nhu dang nam trong pool
        private float timer;
        private float activeLifetime;
        private float scaleMultiplier = 1f;
        private float alphaMultiplier = 1f;
        private Vector2 activeDrift;
        private Vector3 basePosition;
        private Color baseColor = Color.white;
        private Transform cachedTransform;

        public SpriteRenderer Renderer => spriteRenderer;

        private void Awake()
        {
            cachedTransform = transform;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
        }

        // Goi mot lan boi pool luc tao ra. Nho pool + loai de tu tra ve dung ngan.
        public void Bind(StarFXPool pool, StarFXType fxType)
        {
            owner = pool;
            type = fxType;
        }

        public void Play(Vector3 position, float rotationDegrees, float scaleMul, float alphaMul,
                         float lifetimeOverride, Vector2 extraDrift)
        {
            if (cachedTransform == null)
                cachedTransform = transform;

            basePosition = position;
            cachedTransform.position = position;
            cachedTransform.rotation = Quaternion.Euler(0f, 0f, rotationDegrees);

            scaleMultiplier = scaleMul;
            alphaMultiplier = Mathf.Clamp01(alphaMul);
            activeLifetime = lifetimeOverride > 0f ? lifetimeOverride : lifetime;
            activeDrift = drift + extraDrift;

            timer = 0f;
            isPlaying = true;
            isReturned = false;

            ApplyFrame(0f);
        }

        private void Update()
        {
            if (!isPlaying)
                return;

            timer += Time.deltaTime;
            float t = activeLifetime > 0f ? Mathf.Clamp01(timer / activeLifetime) : 1f;

            ApplyFrame(t);

            if (rotationSpeed != 0f)
                cachedTransform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (t >= 1f && autoReturn)
                ReturnNow();
        }

        private void ApplyFrame(float t)
        {
            // SmoothStep: vao/ra deu mem, khong can AnimationCurve (curve la object, danh gia
            // moi frame se sinh rac va kho chinh dong bo giua 17 prefab).
            float eased = t * t * (3f - 2f * t);

            float s = Mathf.Lerp(startScale, endScale, eased) * scaleMultiplier;
            cachedTransform.localScale = new Vector3(s, s, 1f);

            if (activeDrift.sqrMagnitude > 0f)
                cachedTransform.position = basePosition + (Vector3)(activeDrift * timer);

            Color c = baseColor;
            c.a = Mathf.Lerp(startAlpha, endAlpha, eased) * alphaMultiplier;
            spriteRenderer.color = c;
        }

        // Tra ve pool. An toan khi goi nhieu lan — co isReturned chan tra hai lan, neu khong cung
        // mot doi tuong se nam hai lan trong ngan va bi hai noi dung song song.
        public void ReturnNow()
        {
            if (isReturned)
                return;

            isPlaying = false;
            isReturned = true;

            if (owner != null)
                owner.Release(type, this);
            else
                gameObject.SetActive(false);
        }

        // Pool goi truoc khi cat vao ngan: dua moi thu ve mac dinh de lan dung sau khong dinh
        // trang thai cu (day la cho hay quen nhat khi tai su dung).
        public void ResetState()
        {
            isPlaying = false;
            timer = 0f;
            scaleMultiplier = 1f;
            alphaMultiplier = 1f;
            activeDrift = Vector2.zero;

            if (cachedTransform == null)
                cachedTransform = transform;

            cachedTransform.localScale = Vector3.one;
            cachedTransform.rotation = Quaternion.identity;
            spriteRenderer.color = baseColor;
        }

        // Danh dau lai la "dang o ngoai" — pool goi sau khi lay ra khoi ngan.
        public void MarkTaken()
        {
            isReturned = false;
        }
    }
}
