using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarSower.UI
{
    // Title card thuần UI: fade in -> giữ -> fade out cho 2 dòng chữ. Không biết gì về
    // "cinematic" hay "level" — tái sử dụng được cho bất kỳ title card nào khác sau này
    // (banner Region, achievement toast...). Bên gọi chỉ cần await PlayRoutine().
    public class LevelTitleView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform titleRoot;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text chapterLabel;

        [Header("Timing")]
        [SerializeField] private float fadeInDuration = 0.6f;
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private float fadeOutDuration = 0.6f;

        [Header("Scale")]
        [SerializeField] private float startScale = 0.7f;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public IEnumerator PlayRoutine(string title, string chapter)
        {
            titleLabel.text = title;
            chapterLabel.text = chapter;
            gameObject.SetActive(true);

            yield return Animate(0f, 1f, startScale, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Animate(1f, 0f, 1f, 1f, fadeOutDuration);

            gameObject.SetActive(false);
        }

        private IEnumerator Animate(float fromAlpha, float toAlpha, float fromScale, float toScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
                titleRoot.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
                yield return null;
            }

            canvasGroup.alpha = toAlpha;
            titleRoot.localScale = Vector3.one * toScale;
        }
    }
}
