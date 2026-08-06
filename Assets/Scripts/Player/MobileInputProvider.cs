using UnityEngine;
using StarSower.Core;
using StarSower.UI;

namespace StarSower.Player
{
    // Input Mobile: đọc trục ngang từ OnScreenJoystick và nút nhảy từ TouchButton.
    // Là adapter UI -> IInputProvider duy nhất biết đến 2 widget UI này — PlayerController
    // chỉ thấy IInputProvider nên không phụ thuộc trực tiếp vào UI.
    public class MobileInputProvider : MonoBehaviour, IInputProvider
    {
        [SerializeField] private OnScreenJoystick moveJoystick;
        [SerializeField] private TouchButton jumpButton;

        [Tooltip("S3-000 — nut lao. De trong thi khong lao duoc tren mobile, nhung game van chay.")]
        [SerializeField] private TouchButton dashButton;

        public float Horizontal => moveJoystick != null ? moveJoystick.Horizontal : 0f;
        public bool JumpHeld => jumpButton != null && jumpButton.IsPressed;

        public bool JumpPressed
        {
            get
            {
                if (jumpButton == null || !jumpButton.WasPressedThisFrame)
                    return false;

                jumpButton.ConsumePress();
                return true;
            }
        }

        public bool DashPressed
        {
            get
            {
                if (dashButton == null || !dashButton.WasPressedThisFrame)
                    return false;

                dashButton.ConsumePress();
                return true;
            }
        }
    }
}
