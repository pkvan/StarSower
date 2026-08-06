using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Input bằng bàn phím/Input Manager, dùng để test trong Editor. Implement IInputProvider
    // để PlayerController có thể chuyển sang MobileInputProvider mà không cần sửa gì.
    public class KeyboardInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private string horizontalAxisName = "Horizontal";
        [SerializeField] private string jumpButtonName = "Jump";

        public float Horizontal => Input.GetAxisRaw(horizontalAxisName);
        public bool JumpPressed => Input.GetButtonDown(jumpButtonName);

        // S3-000 — Left Shift hoac K. Dung KeyCode chu khong Input Manager axis: them axis moi doi
        // sua ProjectSettings, ma phim lao thi khong can cau hinh lai.
        public bool DashPressed => Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K);
        public bool JumpHeld => Input.GetButton(jumpButtonName);
    }
}
