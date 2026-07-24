using UnityEngine;
using StarSower.Core;

namespace StarSower.CameraSystem
{
    // Cài đặt mặc định của ICameraTarget: bọc 1 Transform (thường là Player).
    // Chỉ tồn tại để CameraFollowY không cần giữ tham chiếu Transform trực tiếp —
    // khi cần camera theo dõi thứ khác (điểm cutscene, boss), thay component này bằng
    // một ICameraTarget khác mà không phải sửa CameraFollowY.
    public class TransformCameraTarget : MonoBehaviour, ICameraTarget
    {
        public Vector3 Position => transform.position;
    }
}
