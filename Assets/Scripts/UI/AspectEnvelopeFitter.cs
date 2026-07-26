using UnityEngine;

namespace StarSower.UI
{
    // Phóng ảnh cho PHỦ KÍN khung cha mà vẫn giữ đúng tỉ lệ, phần thừa tràn ra ngoài bị cắt
    // (S1-020B). Đây là kiểu "aspect fill", khác hẳn Image.preserveAspect vốn là "aspect FIT" —
    // nó co ảnh cho VỪA khung, nên ảnh ngang 1672x941 đặt vào màn dọc chỉ lấp được khoảng nửa
    // chiều cao, phần còn lại trống trơn. Đó chính là lỗi nền chòm sao chỉ phủ nửa màn iPhone.
    //
    // Tự chạy lại khi kích thước khung cha đổi, nên đúng trên mọi cỡ iPhone mà không cần cấu hình.
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class AspectEnvelopeFitter : MonoBehaviour
    {
        [Tooltip("Tỉ lệ gốc của ảnh = rộng / cao. Ảnh nền 1672x941 -> 1.777.")]
        [SerializeField] private float sourceAspect = 1672f / 941f;

        private RectTransform rectTransform;
        private RectTransform parentRect;
        private Vector2 lastParentSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentRect = rectTransform.parent as RectTransform;
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            if (parentRect == null)
                return;

            if (parentRect.rect.size == lastParentSize)
                return;

            Apply();
        }

        private void Apply()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (parentRect == null) parentRect = rectTransform.parent as RectTransform;
            if (parentRect == null || sourceAspect <= 0f)
                return;

            Vector2 parentSize = parentRect.rect.size;
            lastParentSize = parentSize;
            if (parentSize.x <= 0f || parentSize.y <= 0f)
                return;

            // Neo vào GIỮA và tự đặt kích thước — không dùng anchor stretch, vì stretch sẽ ép ảnh
            // méo theo khung thay vì giữ tỉ lệ.
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            float parentAspect = parentSize.x / parentSize.y;

            // Khung DỌC hơn ảnh -> lấy chiều cao làm chuẩn, bề rộng tràn ra hai bên (và ngược lại).
            rectTransform.sizeDelta = parentAspect < sourceAspect
                ? new Vector2(parentSize.y * sourceAspect, parentSize.y)
                : new Vector2(parentSize.x, parentSize.x / sourceAspect);
        }
    }
}
