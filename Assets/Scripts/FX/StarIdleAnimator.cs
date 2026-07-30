using UnityEngine;

namespace StarSower.FX
{
    // Manh sao luc chua duoc nhat: lo lung, nghieng qua nghieng lai rat cham, quang sang tho nhe,
    // thi thoang bat ra mot hat lap lanh.
    //
    // Moc la localPosition/localRotation/localScale GOC luu luc Awake, va moi frame deu tinh LAI
    // tu moc do chu khong cong don. Cong don kieu transform.Rotate/position += se troi dan sau
    // vai chuc phut choi.
    public class StarIdleAnimator : MonoBehaviour
    {
        [Header("Lo lung")]
        [SerializeField] private float floatAmplitude = 0.06f;
        [Min(0.05f)]
        [Tooltip("Thoi gian tron mot chu ky len-xuong (giay).")]
        [SerializeField] private float floatPeriod = 2f;

        [Header("Nghieng")]
        [SerializeField] private float rotationAmplitude = 4f;
        [Min(0.05f)]
        [SerializeField] private float rotationPeriod = 3.1f;

        [Header("Quang sang")]
        [Tooltip("Con Star Glow. De trong thi bo qua phan tho sang.")]
        [SerializeField] private Transform glow;
        [SerializeField] private float glowScaleMin = 0.95f;
        [SerializeField] private float glowScaleMax = 1.05f;
        [Min(0.05f)]
        [SerializeField] private float glowPeriod = 2.6f;

        [Header("Lap lanh")]
        [Min(0.1f)]
        [SerializeField] private float sparkleIntervalMin = 1.5f;
        [Min(0.1f)]
        [SerializeField] private float sparkleIntervalMax = 3f;
        [SerializeField] private float sparkleRadius = 0.22f;
        [SerializeField] private float sparkleScale = 0.7f;

        private StarFXPool pool;
        private Vector3 baseLocalPosition;
        private Vector3 baseGlowScale = Vector3.one;
        private float baseRotationZ;
        private float phase;
        private float sparkleTimer;
        private float sparkleInterval;
        private bool running = true;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
            baseRotationZ = transform.localEulerAngles.z;
            if (glow != null)
                baseGlowScale = glow.localScale;

            // Lech pha ngau nhien: nhieu manh sao gan nhau se khong nhap nhô dong loat nhu mot khoi.
            phase = Random.Range(0f, 100f);
            sparkleInterval = Random.Range(sparkleIntervalMin, sparkleIntervalMax);
        }

        public void SetPool(StarFXPool fxPool)
        {
            pool = fxPool;
        }

        // Goi khi bat dau thu thap. Dung ngay moi thu, ke ca hat lap lanh dinh ky.
        public void StopIdle()
        {
            running = false;
        }

        private void Update()
        {
            if (!running)
                return;

            float t = Time.time + phase;

            float y = Mathf.Sin(t * (Mathf.PI * 2f / floatPeriod)) * floatAmplitude;
            transform.localPosition = baseLocalPosition + new Vector3(0f, y, 0f);

            float angle = Mathf.Sin(t * (Mathf.PI * 2f / rotationPeriod)) * rotationAmplitude;
            transform.localRotation = Quaternion.Euler(0f, 0f, baseRotationZ + angle);

            if (glow != null)
            {
                // Chu ky rieng, lech voi nhip lo lung -> hai chuyen dong khong bao gio trung phach,
                // nhin song hon la cung len cung xuong.
                float p = Mathf.InverseLerp(-1f, 1f, Mathf.Sin(t * (Mathf.PI * 2f / glowPeriod)));
                glow.localScale = baseGlowScale * Mathf.Lerp(glowScaleMin, glowScaleMax, p);
            }

            if (pool == null)
                return;

            sparkleTimer += Time.deltaTime;
            if (sparkleTimer < sparkleInterval)
                return;

            sparkleTimer = 0f;
            sparkleInterval = Random.Range(sparkleIntervalMin, sparkleIntervalMax);

            Vector2 offset = Random.insideUnitCircle * sparkleRadius;
            pool.Spawn((StarFXType)((int)StarFXType.Sparkle01 + Random.Range(0, 3)),
                       transform.position + (Vector3)offset,
                       Random.Range(0f, 360f), sparkleScale);
        }

        // Dua ve dung trang thai ban dau — dung khi man choi duoc nap lai.
        public void ResetIdle()
        {
            running = true;
            sparkleTimer = 0f;
            transform.localPosition = baseLocalPosition;
            transform.localRotation = Quaternion.Euler(0f, 0f, baseRotationZ);
            if (glow != null)
                glow.localScale = baseGlowScale;
        }
    }
}
