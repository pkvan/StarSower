using UnityEngine;
using StarSower.Core;

namespace StarSower.CameraSystem
{
    // Camera ghi nhớ điểm Y cao nhất từng đạt được (HighestY) — mốc này không bao giờ giảm,
    // dùng làm chuẩn cho fall-detection (GameOverManager). Trong dead zone, camera bám theo
    // HighestY như cũ (chỉ đi lên, không rung khi nhảy nhỏ). Khi Player rơi RA NGOÀI dead zone,
    // camera được phép "lia" xuống theo Player (để còn thấy đường nhảy lại) nhưng bị chặn
    // (leash) không xuống quá maxDropDistance dưới HighestY — không bao giờ teleport, luôn
    // qua SmoothDamp. Là chủ sở hữu duy nhất của transform.position — nếu có gắn ICameraShake,
    // offset rung được cộng vào ở bước cuối, tách khỏi trạng thái SmoothDamp.
    public class CameraFollowY : MonoBehaviour
    {
        [Tooltip("Component implement ICameraTarget (vd: TransformCameraTarget gắn trên Player).")]
        [SerializeField] private MonoBehaviour targetSource;

        [SerializeField] private float verticalOffset = 2f;
        [SerializeField] private float smoothTime = 0.2f;

        [Tooltip("Tốc độ tối đa camera được phép di chuyển (unit/giây). Chặn overshoot khi target dịch chuyển đột ngột (vd: Region Transition sau này).")]
        [SerializeField] private float maxFollowSpeed = 30f;

        [Header("Dead Zone")]
        [Tooltip("Vùng đệm quanh camera (cả trên lẫn dưới) mà Player di chuyển tự do không kéo camera theo.")]
        [SerializeField] private float deadZoneHeight = 1f;

        [Header("Rơi (Camera Leash)")]
        [Tooltip("Khi Player rơi ra ngoài Dead Zone, camera được lia xuống tối đa bao nhiêu dưới điểm cao nhất đã đạt (HighestY). " +
                 "Nên đặt NHỎ HƠN maxFallDistance của GameOverManager, để camera dừng lia trước khi Player thật sự chết.")]
        [SerializeField] private float maxDropDistance = 5f;

        [Header("Camera Shake (tuỳ chọn)")]
        [Tooltip("Component implement ICameraShake. Để trống nếu chưa cần rung camera.")]
        [SerializeField] private MonoBehaviour cameraShakeSource;

        private ICameraTarget target;
        private ICameraShake cameraShake;
        private float baseX;
        private float trackedY;
        private float highestY;
        private float verticalVelocity;

        // Mốc cao nhất từng đạt — KHÔNG dao động theo việc camera lia xuống lúc rơi.
        // GameOverManager dùng giá trị này (thay vì transform.position.y) để đo độ sâu rơi.
        public float HighestY => highestY;

        private void Awake()
        {
            target = targetSource as ICameraTarget;
            cameraShake = cameraShakeSource as ICameraShake;
        }

        private void Start()
        {
            baseX = transform.position.x;
            trackedY = transform.position.y;
            highestY = trackedY;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            float targetFollowY = target.Position.y + verticalOffset;

            float deadZoneTop = trackedY + deadZoneHeight;
            if (targetFollowY > deadZoneTop)
                highestY += targetFollowY - deadZoneTop;

            // Công thức LIÊN TỤC (không if/else rẽ nhánh) từ "đứng yên tại HighestY" sang
            // "lia xuống theo Player" — bản cũ dùng if/else khiến desiredY NHẢY ĐỘT NGỘT đúng
            // tại biên Dead Zone (chênh lệch = deadZoneHeight). Khi Player đứng yên rung nhẹ
            // ngay biên đó, desiredY nhảy qua lại mỗi frame -> camera rung liên tục ("động đất").
            // Công thức dưới đây cho giá trị khớp nhau tuyệt đối tại điểm biên nên không còn bước nhảy.
            float desiredY = Mathf.Clamp(targetFollowY + deadZoneHeight, highestY - maxDropDistance, highestY);

            trackedY = Mathf.SmoothDamp(trackedY, desiredY, ref verticalVelocity, smoothTime, maxFollowSpeed);

            Vector3 shakeOffset = cameraShake != null ? cameraShake.CurrentOffset : Vector3.zero;
            transform.position = new Vector3(baseX + shakeOffset.x, trackedY + shakeOffset.y, transform.position.z);
        }
    }
}
