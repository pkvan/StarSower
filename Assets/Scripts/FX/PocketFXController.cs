using UnityEngine;

namespace StarSower.FX
{
    // Gan tren Hero. Giu diem den cua sao (tui deo) va dien doan ket khi sao bay toi noi:
    // quang sang tui -> bung nhe -> chop nho -> mot hat lap lanh.
    //
    // Khong biet gi ve dem sao hay tien trinh — chi lo phan nhin. StarCollectEffect moi la noi
    // trao thuong.
    public class PocketFXController : MonoBehaviour
    {
        [Tooltip("Diem sao bay toi. De trong thi lay chinh Transform nay — van chay, chi kem chinh xac.")]
        [SerializeField] private Transform pocketAnchor;

        [Header("Pocket Glow")]
        [Min(0.01f)]
        [SerializeField] private float glowLifetime = 0.25f;
        [SerializeField] private float glowScale = 1f;

        [Header("Pocket Burst")]
        [Min(0.01f)]
        [SerializeField] private float burstLifetime = 0.28f;
        [SerializeField] private float burstScale = 1f;

        [Header("Chop cuoi")]
        [Min(0.01f)]
        [SerializeField] private float flashLifetime = 0.04f;
        [Tooltip("Nho hon han chop dau — day chi la diem nhan luc thu vao, khong phai mot vu no nua.")]
        [SerializeField] private float flashScale = 0.45f;

        [Header("Hat lap lanh cuoi")]
        [SerializeField] private float finalSparkleScale = 1.15f;
        [SerializeField] private float finalSparkleOffset = 0.1f;

        public Transform PocketAnchor => pocketAnchor != null ? pocketAnchor : transform;

        private void Awake()
        {
            if (pocketAnchor == null)
                pocketAnchor = transform;
        }

        // Goi boi StarCollectEffect ngay khi Fly Core cham tui. Nhan pool tu ben goi de khong phai
        // tu di tim — tim kiem toan scene giua luc dien hieu ung la thu phai tranh.
        public void PlayArrival(StarFXPool pool, Vector3 position)
        {
            if (pool == null)
                return;

            // Quang sang bam theo tui trong luc con song, nen Hero co di tiep thi anh sang van
            // dinh tren nguoi chu khong dung lai giua khong trung.
            PooledStarFX glow = pool.Spawn(StarFXType.PocketGlow, position, 0f, glowScale, 1f, glowLifetime);
            if (glow != null)
                glow.transform.SetParent(PocketAnchor, worldPositionStays: true);

            pool.Spawn(StarFXType.PocketBurst, position, 0f, burstScale, 1f, burstLifetime);
            pool.Spawn(StarFXType.Flash, position, 0f, flashScale, 1f, flashLifetime);

            Vector3 offset = new Vector3(Random.Range(-finalSparkleOffset, finalSparkleOffset),
                                         Random.Range(-finalSparkleOffset, finalSparkleOffset), 0f);
            pool.Spawn((StarFXType)((int)StarFXType.Sparkle01 + Random.Range(0, 3)),
                       position + offset, Random.Range(0f, 360f), finalSparkleScale);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.4f, 0.9f);
            Gizmos.DrawWireSphere(PocketAnchor.position, 0.12f);
        }
#endif
    }
}
