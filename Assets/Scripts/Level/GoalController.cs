using UnityEngine;
using StarSower.Core;

namespace StarSower.Level
{
    // Điểm kết thúc 1 chặng: khi Player chạm vào (trigger), CHỈ phát đúng 1 sự kiện
    // GameEvents.OnLevelCompleted rồi thôi — không tự khoá input, không tự lưu, không tự điều
    // khiển UI/camera/scene. Toàn bộ trình tự "đứng yên -> camera lướt lên -> transition -> load
    // scene -> hiện tên khu vực" do LevelFlowManager (lắng nghe sự kiện này) đảm nhiệm.
    [RequireComponent(typeof(Collider2D))]
    public class GoalController : MonoBehaviour
    {
        [SerializeField] private LayerMask playerLayer;

        private bool hasTriggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered)
                return;

            if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
                return;

            hasTriggered = true;
            GameEvents.RaiseLevelCompleted();
        }
    }
}
