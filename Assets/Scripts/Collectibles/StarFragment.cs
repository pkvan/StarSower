using UnityEngine;

namespace StarSower.Collectibles
{
    // Vật phẩm thu thập: xoay nhẹ quanh Z + bobbing lên xuống liên tục; khi Player chạm (trigger,
    // không cản chuyển động) thì báo CollectibleManager, phát particle/sound rồi biến mất vĩnh viễn.
    // Không tự đếm tổng số Star (đó là việc của CollectibleManager) — chỉ báo "tôi vừa được nhặt".
    [RequireComponent(typeof(Collider2D))]
    public class StarFragment : MonoBehaviour
    {
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private LayerMask playerLayer;

        [Header("Animation")]
        [Tooltip("Tốc độ quay quanh trục Z (độ/giây).")]
        [SerializeField] private float rotationSpeed = 90f;

        [Tooltip("Biên độ lơ lửng lên xuống (world units).")]
        [SerializeField] private float bobHeight = 0.15f;

        [Tooltip("Tốc độ lơ lửng lên xuống (chu kỳ/giây).")]
        [SerializeField] private float bobSpeed = 2f;

        [Header("Collect Effect")]
        [Tooltip("Particle phát tại vị trí Star khi thu thập (tuỳ chọn, để trống nếu chưa có).")]
        [SerializeField] private ParticleSystem collectParticlePrefab;

        [Tooltip("Âm thanh phát khi thu thập (tuỳ chọn, để trống nếu chưa có).")]
        [SerializeField] private AudioClip collectSound;

        private Vector3 basePosition;
        private bool isCollected;

        // S2-005. Co thi nhuong toan bo phan nhin + thoi diem cong diem cho no; khong co thi moi
        // thu chay y nhu truoc. Nho vay man nao chua gan hieu ung moi van hoat dong binh thuong.
        private StarSower.FX.StarCollectEffect collectEffect;
        private bool hasIdleAnimator;

        private void Awake()
        {
            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();

            collectEffect = GetComponent<StarSower.FX.StarCollectEffect>();

            // StarIdleAnimator cung ghi transform. Chi mot trong hai duoc phep — neu de ca hai
            // chay thi hai cong thuc lo lung dap len nhau moi frame.
            hasIdleAnimator = GetComponent<StarSower.FX.StarIdleAnimator>() != null;
        }

        private void Start()
        {
            basePosition = transform.position;
        }

        private void Update()
        {
            if (isCollected || hasIdleAnimator)
                return;

            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            float yOffset = Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f) * bobHeight;
            transform.position = basePosition + new Vector3(0f, yOffset, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected)
                return;

            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            Collect();
        }

        private void Collect()
        {
            isCollected = true;

            // Co hieu ung moi thi no lam chu toan bo tu day: dien hoat anh, va cong diem KHI SAO
            // BAY TOI TUI chu khong phai ngay bay gio. Van la CollectibleManager.RegisterCollected()
            // duoc goi, van dung mot bo dem — chi khac thoi diem.
            if (collectEffect != null && collectEffect.TryBeginCollect())
                return;

            collectibleManager.RegisterCollected();

            if (collectParticlePrefab != null)
            {
                ParticleSystem spawnedParticle = Instantiate(collectParticlePrefab, transform.position, Quaternion.identity);
                Destroy(spawnedParticle.gameObject, spawnedParticle.main.duration);
            }

            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            Destroy(gameObject);
        }
    }
}
