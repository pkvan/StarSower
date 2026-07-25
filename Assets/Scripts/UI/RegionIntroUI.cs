using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarSower.UI
{
    // Hiện tên khu vực mới giữa màn hình sau khi transition xong: fade in -> giữ -> fade out, tự
    // động, không cần người chơi bấm gì. Thuần hiển thị — không biết Goal/Scene/Save gì cả,
    // LevelFlowManager chỉ cần gọi ShowRegionName(tên) đúng lúc.
    public class RegionIntroUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text nameLabel;

        [SerializeField] private float fadeInDuration = 0.6f;
        [Tooltip("Thời gian giữ tên khu vực hiển thị đủ rõ trước khi fade out.")]
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private float fadeOutDuration = 0.6f;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
        }

        public IEnumerator ShowRegionName(string regionName)
        {
            nameLabel.text = regionName;

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
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
