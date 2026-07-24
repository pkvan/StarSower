using UnityEngine;

namespace StarSower.Platform
{
    // Platform 1 chiều: Player nhảy xuyên từ dưới lên, đứng được từ trên. Dùng PlatformEffector2D
    // (giải pháp Unity chuẩn) — component này chỉ đảm bảo Collider2D bật usedByEffector để hiệu ứng
    // hoạt động, còn góc bề mặt (surfaceArc) tinh chỉnh trực tiếp trên PlatformEffector2D.
    [RequireComponent(typeof(PlatformEffector2D))]
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().usedByEffector = true;
        }
    }
}
