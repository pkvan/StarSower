using UnityEngine;
using StarSower.Core;

namespace StarSower.CameraSystem
{
    // Camera bám theo Player: trục X có Dead Zone + smoothing chậm (ổn định khi chạy ngang,
    // không đung đưa khi đổi hướng liên tục); trục Y không Dead Zone + smoothing nhanh hơn
    // (luôn bám sát khi nhảy/rơi). Không ratchet, không khóa trục — khác CameraFollowY (chỉ đi lên).
    // Là chủ sở hữu duy nhất của transform.position; nếu có ICameraShake, offset rung cộng ở bước
    // cuối, tách khỏi trạng thái SmoothDamp để không hỏng độ mượt. Dead Zone X dùng Mathf.Clamp
    // liên tục nên không có bước nhảy tại biên -> không rung.
    public class CameraFollow2D : MonoBehaviour
    {
        [Tooltip("Component implement ICameraTarget (vd: TransformCameraTarget gắn trên Player).")]
        [SerializeField] private MonoBehaviour targetSource;

        [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

        [Tooltip("Tốc độ tối đa camera được phép di chuyển (unit/giây). Chặn overshoot khi target dịch chuyển đột ngột.")]
        [SerializeField] private float maxFollowSpeed = 30f;

        [Header("Trục X (ổn định khi chạy ngang)")]
        [Tooltip("Bám theo trục X. Tắt nếu muốn giữ camera cố định ngang.")]
        [SerializeField] private bool followX = true;
        [Tooltip("Bề rộng Dead Zone ngang (world units): Player di chuyển trong vùng này thì camera đứng yên.")]
        [SerializeField] private float deadZoneWidth = 2f;
        [Tooltip("Thời gian smoothing trục X — để lớn hơn trục Y cho cảm giác ổn định khi chạy ngang.")]
        [SerializeField] private float smoothTimeX = 0.25f;

        [Header("Trục Y (bám sát khi nhảy/rơi)")]
        [Tooltip("Bám theo trục Y (bao gồm lia xuống khi rơi).")]
        [SerializeField] private bool followY = true;
        [Tooltip("Thời gian smoothing trục Y — để nhỏ hơn trục X cho phản hồi nhanh khi nhảy/rơi.")]
        [SerializeField] private float smoothTimeY = 0.12f;

        [Header("Camera Shake (tuỳ chọn)")]
        [Tooltip("Component implement ICameraShake. Để trống nếu chưa cần rung camera.")]
        [SerializeField] private MonoBehaviour cameraShakeSource;

        private ICameraTarget target;
        private ICameraShake cameraShake;
        private Vector2 tracked;
        private float velocityX;
        private float velocityY;

        private void Awake()
        {
            target = targetSource as ICameraTarget;
            cameraShake = cameraShakeSource as ICameraShake;
        }

        private void Start()
        {
            tracked = transform.position;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Vector2 targetPosition = target.Position;

            if (followX)
            {
                // Dead Zone liên tục: camera chỉ dịch khi Player ra khỏi vùng đệm quanh tâm.
                // Tại biên, desiredX == tracked.x nên không có bước nhảy -> không rung khi đổi hướng.
                float centerX = targetPosition.x + offset.x;
                float halfDeadZone = deadZoneWidth * 0.5f;
                float desiredX = Mathf.Clamp(tracked.x, centerX - halfDeadZone, centerX + halfDeadZone);
                tracked.x = Mathf.SmoothDamp(tracked.x, desiredX, ref velocityX, smoothTimeX, maxFollowSpeed);
            }

            if (followY)
            {
                float desiredY = targetPosition.y + offset.y;
                tracked.y = Mathf.SmoothDamp(tracked.y, desiredY, ref velocityY, smoothTimeY, maxFollowSpeed);
            }

            Vector3 shakeOffset = cameraShake != null ? cameraShake.CurrentOffset : Vector3.zero;
            transform.position = new Vector3(tracked.x + shakeOffset.x, tracked.y + shakeOffset.y, transform.position.z);
        }
    }
}
