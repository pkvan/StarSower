using UnityEngine;

namespace StarSower.Transition
{
    // Placeholder cho "mây che màn hình": tint trắng xám mềm + easing mượt (SmoothStep) thay vì
    // tuyến tính, tạo cảm giác cuộn nhẹ nhàng hơn ColorFadeEffect. Khi có sprite mây thật, chỉ cần
    // gán vào field image ở Inspector — không cần sửa code.
    public class CloudFadeEffect : TransitionEffectBase
    {
        [SerializeField] private Color tintColor = new Color(0.85f, 0.85f, 0.9f, 1f);

        protected override Color TintColor => tintColor;

        protected override float Ease(float t) => Mathf.SmoothStep(0f, 1f, t);
    }
}
