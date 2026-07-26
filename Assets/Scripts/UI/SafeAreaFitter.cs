using UnityEngine;

namespace StarSower.UI
{
    // Co RectTransform về đúng Safe Area của máy (S1-020B) — vùng không bị tai thỏ, thanh Home,
    // hay góc bo che mất. Gắn vào một GameObject TRUNG GIAN nằm giữa Canvas và các phần tử UI:
    // Canvas -> [SafeArea] -> HUD/joystick/nút... Nhờ vậy mọi thứ neo vào mép đều tự lùi vào.
    //
    // Dùng anchor chuẩn hoá chứ không dùng offset pixel: một lần tính là đúng cho mọi kích thước
    // màn hình, và không phải tính lại khi Canvas Scaler đổi tỉ lệ.
    //
    // Theo dõi cả thay đổi hướng máy/độ phân giải: iPhone đổi Safe Area khi xoay hoặc khi thanh
    // trạng thái ẩn/hiện, nếu chỉ tính một lần lúc Start thì UI sẽ lệch sau đó.
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Tooltip("Bỏ qua phần lùi ở cạnh trên. Bật khi muốn nền/ảnh tràn lên sát tai thỏ, chỉ có " +
                 "chữ và nút mới cần lùi vào.")]
        [SerializeField] private bool ignoreTop;

        [Tooltip("Bỏ qua phần lùi ở cạnh dưới (thanh Home).")]
        [SerializeField] private bool ignoreBottom;

        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Rẻ: hai phép so sánh mỗi khung hình, chỉ tính lại khi thực sự đổi.
            if (Screen.safeArea == lastSafeArea &&
                Screen.width == lastScreenSize.x && Screen.height == lastScreenSize.y)
                return;

            Apply();
        }

        private void Apply()
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Vector2 min = lastSafeArea.position;
            Vector2 max = lastSafeArea.position + lastSafeArea.size;

            min.x /= Screen.width;
            max.x /= Screen.width;
            min.y /= Screen.height;
            max.y /= Screen.height;

            if (ignoreBottom) min.y = 0f;
            if (ignoreTop) max.y = 1f;

            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
