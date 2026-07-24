using UnityEngine;
using StarSower.Core;

namespace StarSower.Platform
{
    // Cài đặt IPlatformPool bằng Instantiate/Destroy thông thường.
    // Chỗ duy nhất cần thay khi làm Object Pool thật (Queue tái sử dụng, SetActive thay vì
    // Destroy...) — PlatformSpawner và PlatformRecycler không cần sửa gì khi đổi cài đặt này.
    public class SimplePlatformPool : MonoBehaviour, IPlatformPool
    {
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position, rotation);
        }

        public void Release(GameObject instance)
        {
            Destroy(instance);
        }
    }
}
