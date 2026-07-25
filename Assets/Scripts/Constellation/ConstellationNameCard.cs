using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarSower.Constellations
{
    // Tấm thẻ tên hiện GIỮA MÀN HÌNH ngay sau khi chòm sao vẽ xong: fade in -> giữ ~2 giây ->
    // fade out. Không nút bấm, không chờ người chơi, không chặn gameplay lâu hơn thời lượng của
    // chính nó — đúng tinh thần "không popup" của S1-012.
    //
    // Thuần hiển thị: nhận ConstellationData rồi đọc DisplayName/Description ra. Không biết mốc
    // fragment, không biết save, không tự quyết định lúc nào hiện — ConstellationManager gọi.
    // Nhờ vậy tên chòm sao chỉ tồn tại ở đúng một nơi là asset ConstellationData.
    public class ConstellationNameCard : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text nameLabel;

        [Tooltip("Dòng mô tả dưới tên. Để trống thì bỏ qua, thẻ chỉ hiện mỗi tên.")]
        [SerializeField] private Text descriptionLabel;

        [Header("Timing")]
        [Tooltip("Nên ngắn hơn thời gian vẽ chòm sao, để tên đọc được từ sớm trong lúc nét vẫn đang hiện ra.")]
        [SerializeField] private float fadeInDuration = 0.6f;

        // Không có fadeOutDuration ở đây: thời lượng tan do ConstellationManager truyền vào để tên
        // và chòm sao tan hết đúng cùng một khoảnh khắc.

        [Header("Format")]
        [Tooltip("{0} = tên chòm sao. Để trơn vì font builtin (Arial) KHÔNG có glyph ✨ (U+2728) — " +
                 "điền vào sẽ ra ô vuông rỗng. Thêm trang trí được sau khi nhập font riêng.")]
        [SerializeField] private string nameFormat = "{0}";

        [Tooltip("{0} = mô tả.")]
        [SerializeField] private string descriptionFormat = "\"{0}\"";

        private void Awake()
        {
            canvasGroup.alpha = 0f;
        }

        // Tách FadeIn/FadeOut thay vì một hàm Show() chạy trọn gói: ConstellationManager cần khởi
        // động phần fade in ĐỒNG THỜI với nét vẽ đầu tiên, rồi mới quyết định lúc nào tan — mà lúc
        // đó phải tan cùng nhịp với chòm sao.
        public IEnumerator FadeIn(ConstellationData constellation)
        {
            if (constellation == null)
                yield break;

            nameLabel.text = string.Format(nameFormat, constellation.DisplayName);

            if (descriptionLabel != null)
            {
                // Description còn là placeholder nên nhiều chòm sao có thể bỏ trống. Tắt hẳn dòng
                // mô tả thay vì để một cặp ngoặc kép rỗng lơ lửng dưới tên.
                bool hasDescription = !string.IsNullOrEmpty(constellation.Description);
                descriptionLabel.gameObject.SetActive(hasDescription);
                if (hasDescription)
                    descriptionLabel.text = string.Format(descriptionFormat, constellation.Description);
            }

            yield return Fade(0f, 1f, fadeInDuration);
        }

        public IEnumerator FadeOut(float duration)
        {
            yield return Fade(canvasGroup.alpha, 0f, duration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                canvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
