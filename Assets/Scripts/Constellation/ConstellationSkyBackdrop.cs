using UnityEngine;
using StarSower.FX;

namespace StarSower.Constellations
{
    // Nen troi cua man khoi phuc: mot tam nen phu kin khung hinh + quang sang mem + vai hat tinh
    // van troi rat cham.
    //
    // Hat tinh van lay tu StarFXPool nen KHONG Instantiate giua buoi dien. So hat giu trong khoang
    // 30-60 nhu yeu cau: emitRate * lifetime la so hat song dong thoi, chinh hai so do la ra.
    public class ConstellationSkyBackdrop : MonoBehaviour
    {
        [Header("Nen")]
        [Tooltip("Tam nen phu kin khung hinh. De trong thi chi co nen den cua Fade.")]
        [SerializeField] private SpriteRenderer background;

        [Tooltip("Quang sang mem o giua (anh trang / anh sang moi truong). Tuy chon.")]
        [SerializeField] private SpriteRenderer ambientGlow;

        [Header("Hat tinh van")]
        [Tooltip("So hat nha ra moi giay. Nhan voi Lifetime ra so hat song dong thoi.")]
        [Min(0f)]
        [SerializeField] private float emitRate = 8f;

        [Min(0.1f)]
        [SerializeField] private float particleLifetime = 5f;

        [Tooltip("Ban kinh vung rai hat quanh tam man (world units).")]
        [SerializeField] private float spawnRadius = 4.5f;

        [SerializeField] private float driftSpeed = 0.12f;
        [Range(0f, 1f)] [SerializeField] private float particleAlpha = 0.35f;
        [SerializeField] private float particleScaleMin = 0.5f;
        [SerializeField] private float particleScaleMax = 1.1f;

        private StarFXPool pool;
        private Transform center;
        private float accumulator;
        private bool running;

        public void Begin(StarFXPool fxPool, Transform focus)
        {
            pool = fxPool;
            center = focus != null ? focus : transform;
            accumulator = 0f;
            running = true;
            gameObject.SetActive(true);
        }

        public void Stop()
        {
            running = false;
        }

        // Doi do mo cua ca nen theo tien trinh fade, de nen troi hien ra cung nhip voi moi thu khac.
        public void SetAlpha(float a)
        {
            if (background != null)
            {
                Color c = background.color; c.a = a; background.color = c;
            }
            if (ambientGlow != null)
            {
                Color c = ambientGlow.color; c.a = a * 0.8f; ambientGlow.color = c;
            }
        }

        private void Update()
        {
            if (!running || pool == null || emitRate <= 0f)
                return;

            accumulator += Time.deltaTime * emitRate;
            while (accumulator >= 1f)
            {
                accumulator -= 1f;

                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Vector3 p = center.position + new Vector3(offset.x, offset.y, 0f);
                Vector2 drift = Random.insideUnitCircle.normalized * driftSpeed;

                // Dung Dust cho tinh van: mo, cham, khong loe — dung chat "bui sao" chu khong
                // phai hat lap lanh giat cuc.
                pool.Spawn((StarFXType)((int)StarFXType.Dust01 + Random.Range(0, 3)),
                           p, Random.Range(0f, 360f),
                           Random.Range(particleScaleMin, particleScaleMax),
                           particleAlpha, particleLifetime, drift);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Nhac ngay trong Inspector khi cau hinh se vuot tran hat cho phep.
            float live = emitRate * particleLifetime;
            if (live > 60f)
                Debug.LogWarning($"[Sky] emitRate x lifetime = {live:F0} hat song dong thoi, " +
                                 "vuot tran 60. Ha emitRate hoac lifetime.", this);
        }
#endif
    }
}
