using UnityEngine;
using StarSower.Core;

namespace StarSower.Platform
{
    // Sinh platform ngẫu nhiên phía trên Camera theo trục Y. Chỉ quyết định "khi nào" và
    // "ở đâu" — việc cấp phát uỷ quyền cho IPlatformPool, việc thu hồi khi rơi khỏi màn hình
    // do PlatformRecycler tự lo trên từng platform đã spawn.
    //
    // Đảm bảo nhảy tới được: mỗi platform mới chỉ lệch ngang tối đa maxHorizontalGap và
    // lệch dọc trong [minVerticalGap, maxVerticalGap] so với platform trước đó — hai giá trị
    // này cần được chỉnh cho khớp với tầm nhảy thực tế của PlayerMotor (moveSpeed/jumpForce).
    public class PlatformSpawner : MonoBehaviour
    {
        [Tooltip("Các prefab platform có thể sinh ra (chọn ngẫu nhiên mỗi lần spawn).")]
        [SerializeField] private GameObject[] platformPrefabs;

        [SerializeField] private Transform cameraTransform;

        [Tooltip("Component implement IPlatformPool. Để trống sẽ Instantiate/Destroy thông thường.")]
        [SerializeField] private MonoBehaviour poolSource;

        [Header("Khoảng cách sinh platform")]
        [Tooltip("Sinh platform mới khi còn cách rìa trên Camera một khoảng này.")]
        [SerializeField] private float spawnAheadDistance = 6f;

        [Header("Khoảng cách giữa 2 platform liên tiếp (đảm bảo nhảy tới được)")]
        [SerializeField] private float minVerticalGap = 1.2f;
        [SerializeField] private float maxVerticalGap = 2f;
        [SerializeField] private float maxHorizontalGap = 2.5f;

        [Tooltip("Giới hạn toạ độ X hai bên, tránh platform sinh ra ngoài tầm nhìn.")]
        [SerializeField] private float horizontalBound = 2.2f;

        private IPlatformPool pool;
        private float nextSpawnY;
        private float previousX;

        private void Awake()
        {
            pool = poolSource as IPlatformPool;
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            nextSpawnY = transform.position.y;
            previousX = transform.position.x;
        }

        private void Update()
        {
            if (cameraTransform == null)
                return;

            float spawnThresholdY = cameraTransform.position.y + spawnAheadDistance;
            while (nextSpawnY < spawnThresholdY)
                SpawnNextPlatform();
        }

        private void SpawnNextPlatform()
        {
            if (platformPrefabs == null || platformPrefabs.Length == 0)
                return;

            GameObject prefab = platformPrefabs[Random.Range(0, platformPrefabs.Length)];

            float verticalGap = Random.Range(minVerticalGap, maxVerticalGap);
            float horizontalOffset = Random.Range(-maxHorizontalGap, maxHorizontalGap);
            float nextX = Mathf.Clamp(previousX + horizontalOffset, -horizontalBound, horizontalBound);
            float nextY = nextSpawnY + verticalGap;

            Vector3 spawnPosition = new Vector3(nextX, nextY, 0f);
            GameObject instance = pool != null
                ? pool.Get(prefab, spawnPosition, Quaternion.identity)
                : Instantiate(prefab, spawnPosition, Quaternion.identity);

            PlatformRecycler recycler = instance.GetComponent<PlatformRecycler>();
            if (recycler == null)
                recycler = instance.AddComponent<PlatformRecycler>();
            recycler.Initialize(cameraTransform, pool);

            nextSpawnY = nextY;
            previousX = nextX;
        }
    }
}
