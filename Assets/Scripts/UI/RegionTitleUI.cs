using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Biome;

namespace StarSower.UI
{
    // Hiện tên chòm sao của khu vực ở ĐẦU màn hình, đúng MỘT LẦN cho mỗi Region trong một phiên
    // chơi: fade in -> giữ ~2-3 giây -> fade out.
    //
    // KHÔNG chặn gameplay: LevelFlowManager gọi ShowOnce() mà KHÔNG yield (fire-and-forget), nên
    // người chơi di chuyển bình thường suốt lúc chữ đang hiện. Cũng không đụng Time.timeScale —
    // game không hề dừng lại.
    //
    // Đọc Region qua BiomeManager.Region thay vì tự có field RegionData riêng — đúng tiền lệ
    // RegionAtmosphereManager (quyết định thiết kế #40): chỉ MỘT nơi quyết định "scene này thuộc
    // Region nào", tránh hai chỗ khai báo lệch nhau.
    //
    // Không hardcode Forgotten Forest: tên chòm sao nằm trong RegionData.ConstellationTitle. Thêm
    // khu vực mới chỉ cần điền ô đó trong asset, không sửa dòng code nào.
    public class RegionTitleUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleLabel;

        [Tooltip("Nguồn dữ liệu Region của scene này. Để trống thì title không hiện.")]
        [SerializeField] private BiomeManager biomeManager;

        [Header("Format")]
        [Tooltip("{0} = tên chòm sao lấy từ RegionData. CẢNH BÁO: font builtin (Arial) KHÔNG có " +
                 "glyph ✦ (U+2726, khối Dingbats) — sẽ hiện ra ô vuông rỗng. Đổi thành '{0}' trơn " +
                 "hoặc '★ {0} ★' (U+2605, Arial có) nếu chưa nhập font riêng.")]
        [SerializeField] private string titleFormat = "✦ {0} ✦";

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 1f;
        [Tooltip("Thời gian giữ chữ hiện rõ. Yêu cầu thiết kế: khoảng 2-3 giây.")]
        [SerializeField] private float holdDuration = 2.5f;
        [SerializeField] private float fadeOutDuration = 1.2f;

        private Coroutine running;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                // Không bao giờ nhận raycast — chữ trang trí không được nuốt thao tác chạm của
                // người chơi trong lúc đang hiện.
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        // Gọi bởi LevelFlowManager ngay sau khi trả quyền điều khiển. KHÔNG trả về IEnumerator —
        // cố tình để bên gọi không thể vô tình yield và chặn gameplay.
        public void ShowOnce()
        {
            RegionData region = biomeManager != null ? biomeManager.Region : null;
            if (region == null || canvasGroup == null || titleLabel == null)
                return;

            string title = region.ConstellationTitle;
            if (string.IsNullOrEmpty(title))
                return;

            if (RegionTitleSession.HasShown(region.RegionId))
                return;

            RegionTitleSession.MarkShown(region.RegionId);

            if (running != null)
                StopCoroutine(running);

            running = StartCoroutine(PlayRoutine(title));
        }

        private IEnumerator PlayRoutine(string title)
        {
            titleLabel.text = string.Format(titleFormat, title);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            running = null;
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
