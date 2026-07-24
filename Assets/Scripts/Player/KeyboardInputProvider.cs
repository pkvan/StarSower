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
        public bool JumpHeld => Input.GetButton(jumpButtonName);
    }
}
