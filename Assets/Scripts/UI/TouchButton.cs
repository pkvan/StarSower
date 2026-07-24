using UnityEngine;
using UnityEngine.EventSystems;

namespace StarSower.UI
{
    // Nút chạm thô: ghi nhận "vừa được nhấn" (giống Input.GetButtonDown) và "đang được giữ"
    // (giống Input.GetButton). Không biết gì về Player/Input System — nơi đọc giá trị
    // (vd: MobileInputProvider) phải tự gọi ConsumePress() sau khi đọc WasPressedThisFrame
    // để tránh nhận nhầm 2 lần trong 2 frame.
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool WasPressedThisFrame { get; private set; }
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            WasPressedThisFrame = true;
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
        }

        public void ConsumePress()
        {
            WasPressedThisFrame = false;
        }
    }
}
