using System.Collections;

namespace StarSower.Core
{
    // Kiểu hiệu ứng che/mở màn hình lúc chuyển khu vực (màu / mây / ánh sáng...). Tách interface
    // để SceneTransitionController không cần biết chi tiết từng kiểu — đổi style chỉ là đổi
    // component nào được gán vào ô transitionEffectSource, không phải sửa code điều phối.
    public interface ITransitionEffect
    {
        // Che kín màn hình (dùng lúc rời khu vực cũ, trước khi load scene mới).
        IEnumerator PlayIn(float duration);

        // Mở dần lộ ra khu vực mới (dùng ngay sau khi scene mới load xong).
        IEnumerator PlayOut(float duration);
    }
}
