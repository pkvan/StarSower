using UnityEngine;

namespace StarSower.Core
{
    // Trừu tượng hoá việc cấp phát/thu hồi GameObject platform, để PlatformSpawner và
    // PlatformRecycler không cần biết hiện đang Instantiate/Destroy hay dùng Object Pool thật.
    public interface IPlatformPool
    {
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
        void Release(GameObject instance);
    }
}
