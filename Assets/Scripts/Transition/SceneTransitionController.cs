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

        // Che kín với thời lượng RIÊNG cho một lần gọi (S2-006). Thêm nạp chồng thay vì sửa
        // fadeDuration: fadeDuration là nhịp chung của mọi lần chuyển khu vực, đổi nó đi thì lúc
        // VÀO khu vực mới cũng nhanh theo — trong khi chỗ cần nhanh chỉ là lúc rời khu vực cũ.
        public IEnumerator PlayIn(float duration)
        {
            yield return ActiveEffect.PlayIn(Mathf.Max(0f, duration));
        }

        // Mở màn NGAY, không hoạt ảnh. Dùng khi bên dưới lớp chuyển cảnh đã có một màn che khác
        // cũng đen kín: fade lúc đó là đen chồng đen, người chơi không thấy gì suốt cả quãng đó.
        public void SnapOpen()
        {
            StartCoroutine(ActiveEffect.PlayOut(0f));
        }

        // Mở dần lộ khu vực mới — gọi ngay sau khi scene mới load xong.
        public IEnumerator PlayOut()
        {
            yield return ActiveEffect.PlayOut(fadeDuration);
        }

        // Đặt màn hình về trạng thái che kín ngay lập tức, không hoạt ảnh — dùng lúc Start() của
        // scene mới để tránh lộ 1 khung hình chưa che trước khi PlayOut() bắt đầu.
        // Doi mau cho nhung lan che/mo ke tiep. Goi ClearTint() de tra ve mau cua scene.
        public void SetTint(Color color)
        {
            (ActiveEffect as TransitionEffectBase)?.SetTintOverride(color);
        }

        public void ClearTint()
        {
            (ActiveEffect as TransitionEffectBase)?.ClearTintOverride();
        }

        public void SnapCovered()
        {
            StartCoroutine(ActiveEffect.PlayIn(0f));
        }
    }
}
