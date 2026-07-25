using System.Collections;
using UnityEngine;
using StarSower.Core;

namespace StarSower.Transition
{
    public enum FadeType
    {
        Color,
        Cloud,
        Light,
    }

    // Điều phối MỘT hiệu ứng che/mở màn hình cụ thể (chọn qua fadeType) — không tự quyết định KHI
    // NÀO che/mở, chỉ biết CÁCH che/mở. LevelFlowManager là nơi gọi PlayIn()/PlayOut() đúng lúc
    // trong trình tự transition. Đổi fadeType trong Inspector là đổi được cả "phong cách" chuyển
    // cảnh mà không cần sửa dòng code nào — mỗi kiểu là 1 component ITransitionEffect riêng.
    public class SceneTransitionController : MonoBehaviour
    {
        [SerializeField] private FadeType fadeType = FadeType.Color;

        [Tooltip("Thời gian mỗi lượt che/mở màn hình.")]
        [SerializeField] private float fadeDuration = 0.8f;

        [Header("Effect Sources (component implement ITransitionEffect)")]
        [SerializeField] private MonoBehaviour colorEffectSource;
        [SerializeField] private MonoBehaviour cloudEffectSource;
        [SerializeField] private MonoBehaviour lightEffectSource;

        public float FadeDuration => fadeDuration;

        private ITransitionEffect ActiveEffect
        {
            get
            {
                switch (fadeType)
                {
                    case FadeType.Cloud: return cloudEffectSource as ITransitionEffect;
                    case FadeType.Light: return lightEffectSource as ITransitionEffect;
                    default: return colorEffectSource as ITransitionEffect;
                }
            }
        }

        // Che kín màn hình — gọi trước khi load scene mới.
        public IEnumerator PlayIn()
        {
            yield return ActiveEffect.PlayIn(fadeDuration);
        }

        // Mở dần lộ khu vực mới — gọi ngay sau khi scene mới load xong.
        public IEnumerator PlayOut()
        {
            yield return ActiveEffect.PlayOut(fadeDuration);
        }

        // Đặt màn hình về trạng thái che kín ngay lập tức, không hoạt ảnh — dùng lúc Start() của
        // scene mới để tránh lộ 1 khung hình chưa che trước khi PlayOut() bắt đầu.
        public void SnapCovered()
        {
            StartCoroutine(ActiveEffect.PlayIn(0f));
        }
    }
}
