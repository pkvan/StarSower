using UnityEngine;

namespace StarSower.Transition
{
    // Placeholder cho "ánh sáng chớp qua": tint trắng ấm + easing nhanh-vào-chậm-ra (ease-out
    // cubic) tạo cảm giác loé sáng rồi tan dần, khác hẳn nhịp đều đều của ColorFadeEffect.
    public class LightFadeEffect : TransitionEffectBase
    {
        [SerializeField] private Color tintColor = new Color(1f, 0.97f, 0.85f, 1f);

        protected override Color TintColor => tintColor;

        protected override float Ease(float t)
        {
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }
    }
}
