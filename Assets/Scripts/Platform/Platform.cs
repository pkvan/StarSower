using UnityEngine;

namespace StarSower.Platform
{
    // Đánh dấu một platform tĩnh, đảm bảo luôn có Collider2D để Player đứng lên.
    // Hiện chỉ là marker component — chỗ để mở rộng sau này (platform di chuyển, dễ vỡ...).
    [RequireComponent(typeof(Collider2D))]
    public class Platform : MonoBehaviour
    {
    }
}
