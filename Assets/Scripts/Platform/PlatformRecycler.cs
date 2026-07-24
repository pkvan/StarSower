using UnityEngine;
using StarSower.Core;

namespace StarSower.Platform
{
    // Gắn trên mỗi platform được spawn: tự thu hồi (trả Pool hoặc Destroy) khi rơi xuống
    // dưới Camera quá xa. Tách khỏi PlatformSpawner để spawner chỉ lo việc sinh platform.
    public class PlatformRecycler : MonoBehaviour
    {
        [Tooltip("Khoảng cách (world units) dưới Camera mà platform sẽ bị thu hồi.")]
        [SerializeField] private float despawnDistanceBelowCamera = 8f;

        private Transform cameraTransform;
        private IPlatformPool pool;

        // PlatformSpawner gọi hàm này ngay sau khi spawn để cấp camera/pool cần theo dõi.
        public void Initialize(Transform camera, IPlatformPool platformPool)
        {
            cameraTransform = camera;
            pool = platformPool;
        }

        private void Update()
        {
            if (cameraTransform == null)
                return;

            float distanceBelowCamera = cameraTransform.position.y - transform.position.y;
            if (distanceBelowCamera >= despawnDistanceBelowCamera)
                Recycle();
        }

        private void Recycle()
        {
            if (pool != null)
                pool.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
