using System.Collections.Generic;
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

        // ĐẾM số ngón đang giữ, không phải cờ bật/tắt. Với cờ, hai ngón cùng chạm nút nhảy rồi
        // một ngón nhả ra sẽ tắt IsPressed dù ngón kia còn giữ — cú nhảy bị cắt ngắn giữa chừng
        // (lowJumpMultiplier dập ngay). Đếm thì chỉ hết ngón cuối mới coi là nhả.
        private readonly HashSet<int> holdingPointers = new HashSet<int>();

        public bool IsPressed => holdingPointers.Count > 0;

        public void OnPointerDown(PointerEventData eventData)
        {
            WasPressedThisFrame = true;
            holdingPointers.Add(eventData.pointerId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            holdingPointers.Remove(eventData.pointerId);
        }

        private void OnDisable()
        {
            holdingPointers.Clear();
            WasPressedThisFrame = false;
        }

        public void ConsumePress()
        {
            WasPressedThisFrame = false;
        }
    }
}
