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

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

            float clampedX = Mathf.Clamp(localPoint.x, -handleRange, handleRange);
            handle.anchoredPosition = new Vector2(clampedX, 0f);
            Horizontal = clampedX / handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            handle.anchoredPosition = Vector2.zero;
            Horizontal = 0f;
        }
    }
}
