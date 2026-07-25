using UnityEngine;

namespace StarSower.Transition
{
    // Kiểu transition đơn giản nhất: fade tuyến tính sang 1 màu đặc (mặc định — luôn hoạt động
    // dù chưa có asset mây/ánh sáng nào).
    public class ColorFadeEffect : TransitionEffectBase
    {
        [SerializeField] private Color tintColor = new Color(0.05f, 0.05f, 0.12f, 1f);

        protected override Color TintColor => tintColor;
    }
}
