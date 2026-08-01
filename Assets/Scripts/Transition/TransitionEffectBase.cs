using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Core;

namespace StarSower.Transition
{
    // Logic fade CanvasGroup dùng chung cho mọi kiểu hiệu ứng transition (màu/mây/ánh sáng...).
    // Mỗi kiểu con chỉ cần khai báo màu tint + đường cong easing riêng — tránh lặp lại coroutine
    // fade 3 lần cho 3 style gần giống hệt nhau.
    public abstract class TransitionEffectBase : MonoBehaviour, ITransitionEffect
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Image image;

        protected abstract Color TintColor { get; }

        // Mau ghi de tam thoi. Doan phim chom sao ket bang mot chum sang trang, neu lop chuyen canh
        // van giu mau xanh den mac dinh thi giua hai buoc se loe mot nhip toi — nen no can bao
        // duoc "lan nay dung mau nay".
        private Color? tintOverride;

        public void SetTintOverride(Color color) => tintOverride = color;
        public void ClearTintOverride() => tintOverride = null;

        private Color ActiveTint => tintOverride ?? TintColor;

        // Mặc định tuyến tính — kiểu con override để có cảm giác riêng (mượt như mây, chớp như ánh sáng...).
        protected virtual float Ease(float t) => t;

        public IEnumerator PlayIn(float duration) => Fade(0f, 1f, duration);
        public IEnumerator PlayOut(float duration) => Fade(1f, 0f, duration);

        private IEnumerator Fade(float from, float to, float duration)
        {
            image.color = ActiveTint;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Ease(Mathf.Clamp01(elapsed / duration));
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
