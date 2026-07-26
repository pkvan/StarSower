using UnityEngine;
using UnityEngine.EventSystems;

namespace StarSower.UI
{
    // Joystick ảo đơn trục ngang: kéo trong bán kính handleRange quanh vị trí neo,
    // thả tay tự về giữa. Chỉ phát ra Horizontal chuẩn hoá [-1, 1] — không biết gì về
    // Player/Input System, MobileInputProvider mới là nơi diễn giải giá trị này.
    public class OnScreenJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private RectTransform background;
        [SerializeField] private float handleRange = 100f;

        public float Horizontal { get; private set; }

        // Ngón nào đang "sở hữu" joystick. Không có nó thì ngón thứ hai chạm vào joystick sẽ
        // giành quyền, và quan trọng hơn: ngón thứ hai nhả ra sẽ ĐẶT LẠI joystick về 0 dù ngón
        // thứ nhất vẫn đang giữ — nhân vật khựng lại giữa lúc đang chạy.
        private const int NoPointer = -999;
        private int activePointerId = NoPointer;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (activePointerId != NoPointer)
                return;

            activePointerId = eventData.pointerId;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            float clampedX = Mathf.Clamp(localPoint.x, -handleRange, handleRange);
            handle.anchoredPosition = new Vector2(clampedX, 0f);
            Horizontal = clampedX / handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
                return;

            activePointerId = NoPointer;
            handle.anchoredPosition = Vector2.zero;
            Horizontal = 0f;
        }

        // Tay bị nhấc khỏi màn hình bất thường (cuộc gọi đến, chuyển app...) — nhả quyền sở hữu,
        // nếu không joystick sẽ kẹt ở giá trị cuối và nhân vật tự chạy mãi.
        private void OnDisable()
        {
            activePointerId = NoPointer;
            Horizontal = 0f;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
        }
    }
}
