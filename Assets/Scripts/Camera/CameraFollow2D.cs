using UnityEngine;
using StarSower.Core;
using StarSower.Player;

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

        [Tooltip("S3-000 — chiều cao Dead Zone dọc (world units). Nhảy nhỏ trong vùng này thì camera " +
                 "ĐỨNG YÊN. Không có nó, mỗi cú nhảy là cả khung hình nhấp nhô theo — thứ khiến " +
                 "màn hình ngang mệt mắt hơn hẳn màn hình dọc.")]
        [SerializeField] private float deadZoneHeight = 2.6f;

        [Tooltip("Rơi quá nhanh thì bỏ Dead Zone dọc và bám sát, để không rơi ra khỏi khung hình.")]
        [SerializeField] private float fallCatchUpSpeed = -12f;

        [Header("Nhìn trước (S3-000)")]
        [Tooltip("Camera lia trước mặt bao xa theo hướng đang chạy (world units). 0 = tắt.")]
        [SerializeField] private float lookAheadDistance = 2.4f;

        [Tooltip("Thời gian để lượng nhìn trước đổi chiều. Để lớn — đổi nhanh sẽ giật khi người " +
                 "chơi rê qua rê lại.")]
        [SerializeField] private float lookAheadSmoothTime = 0.45f;

        [Tooltip("Tốc độ ngang tối thiểu mới tính là 'đang chạy'. Dưới ngưỡng thì giữ hướng cũ, " +
                 "tránh camera đảo qua đảo lại lúc gần đứng yên.")]
        [SerializeField] private float lookAheadMinSpeed = 0.6f;

        [Tooltip("S3-002 — nhìn trước theo trục DỌC: nhảy thì khung hình nhích lên, rơi thì nhích " +
                 "xuống. Tính theo vận tốc dọc nên nhảy nhỏ chỉ nhích nhẹ. 0 = tắt.")]
        [SerializeField] private float verticalLookAhead = 1.2f;

        [Tooltip("Vận tốc dọc ứng với lượng nhìn trước tối đa.")]
        [SerializeField] private float verticalLookAheadRefSpeed = 12f;

        [Header("Biên khung hình (S3-002)")]
        [Tooltip("Chặn camera lia ra ngoài màn — không cho thấy khoảng trống hay vùng chưa dựng. " +
                 "Tắt thì camera đi tự do.")]
        [SerializeField] private bool useBounds;

        [Tooltip("Biên theo TÂM camera, đơn vị world. Đã trừ sẵn nửa khung nhìn khi Auto Inset bật.")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-50f, -50f);
        [SerializeField] private Vector2 boundsMax = new Vector2(50f, 50f);

        [Tooltip("Tự thu biên vào một nửa khung nhìn, để MÉP khung hình dừng đúng ở biên chứ không " +
                 "phải tâm camera. Tắt khi màn hẹp hơn khung nhìn, nếu không biên sẽ lộn ngược.")]
        [SerializeField] private bool boundsAutoInset = true;

        [Tooltip("Đọc vận tốc để biết hướng nhìn trước. Để trống thì tự tìm.")]
        [SerializeField] private PlayerMotor motor;

        [Header("Camera Shake (tuỳ chọn)")]
        [Tooltip("Component implement ICameraShake. Để trống nếu chưa cần rung camera.")]
        [SerializeField] private MonoBehaviour cameraShakeSource;

        private ICameraTarget target;
        private ICameraShake cameraShake;
        private Vector2 tracked;
        private float velocityX;
        private float velocityY;
        private float lookAhead;
        private float lookAheadVelocity;
        private float facing = 1f;
        private float verticalLook;
        private float verticalLookVelocity;
        private Camera cam;

        private void Awake()
        {
            target = targetSource as ICameraTarget;
            cameraShake = cameraShakeSource as ICameraShake;
            if (motor == null)
                motor = FindFirstObjectByType<PlayerMotor>();
            cam = GetComponent<Camera>();
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

            Vector2 velocity = motor != null ? motor.Velocity : Vector2.zero;

            if (followX)
            {
                // Nhìn trước: dồn khung hình về phía đang chạy, nên người chơi thấy đường phía
                // trước nhiều hơn phía sau. Chỉ đổi hướng khi thực sự đang chạy — dưới ngưỡng thì
                // giữ hướng cũ, nếu không camera sẽ đảo qua đảo lại mỗi lần rê nhẹ cần điều khiển.
                if (lookAheadDistance > 0f && Mathf.Abs(velocity.x) > lookAheadMinSpeed)
                    facing = Mathf.Sign(velocity.x);

                float lookTarget = lookAheadDistance > 0f ? facing * lookAheadDistance : 0f;
                lookAhead = Mathf.SmoothDamp(lookAhead, lookTarget, ref lookAheadVelocity, lookAheadSmoothTime);

                // Dead Zone liên tục: camera chỉ dịch khi Player ra khỏi vùng đệm quanh tâm.
                // Tại biên, desiredX == tracked.x nên không có bước nhảy -> không rung khi đổi hướng.
                float centerX = targetPosition.x + offset.x + lookAhead;
                float halfDeadZone = deadZoneWidth * 0.5f;
                float desiredX = Mathf.Clamp(tracked.x, centerX - halfDeadZone, centerX + halfDeadZone);
                tracked.x = Mathf.SmoothDamp(tracked.x, desiredX, ref velocityX, smoothTimeX, maxFollowSpeed);
            }

            if (followY)
            {
                float centerY = targetPosition.y + offset.y;

                // Dead Zone dọc: nhảy nhỏ thì khung hình đứng yên. Rơi nhanh thì bỏ vùng đệm và
                // bám sát, neu khong nguoi choi se roi ra khoi khung truoc khi camera kip duoi.
                // Nhìn trước theo trục dọc: nhảy thì hé lên, rơi thì hé xuống. Chuẩn hoá theo
                // verticalLookAheadRefSpeed rồi kẹp, nên nhảy nhỏ chỉ nhích nhẹ chứ không giật cả khung.
                if (verticalLookAhead > 0f && verticalLookAheadRefSpeed > 0f)
                {
                    float k = Mathf.Clamp(velocity.y / verticalLookAheadRefSpeed, -1f, 1f);
                    verticalLook = Mathf.SmoothDamp(verticalLook, k * verticalLookAhead,
                                                    ref verticalLookVelocity, lookAheadSmoothTime);
                    centerY += verticalLook;
                }

                float halfDeadZoneY = velocity.y < fallCatchUpSpeed ? 0f : deadZoneHeight * 0.5f;
                float desiredY = Mathf.Clamp(tracked.y, centerY - halfDeadZoneY, centerY + halfDeadZoneY);
                tracked.y = Mathf.SmoothDamp(tracked.y, desiredY, ref velocityY, smoothTimeY, maxFollowSpeed);
            }

            ApplyBounds();

            Vector3 shakeOffset = cameraShake != null ? cameraShake.CurrentOffset : Vector3.zero;
            transform.position = new Vector3(tracked.x + shakeOffset.x, tracked.y + shakeOffset.y, transform.position.z);
        }

        // Kẹp TÂM camera vào biên. Thu biên theo nửa khung nhìn để MÉP khung dừng đúng ở biên —
        // kẹp thẳng tâm thì vẫn lộ ra nửa khung khoảng trống ngoài màn.
        //
        // Khi màn hẹp hơn khung nhìn (min > max sau khi thu), lấy điểm giữa: không có cách nào che
        // kín được, nên chọn cách cân đối hai bên thay vì dán lệch về một phía.
        private void ApplyBounds()
        {
            if (!useBounds)
                return;

            float insetX = 0f, insetY = 0f;
            if (boundsAutoInset && cam != null && cam.orthographic)
            {
                insetY = cam.orthographicSize;
                insetX = insetY * cam.aspect;
            }

            float minX = boundsMin.x + insetX, maxX = boundsMax.x - insetX;
            float minY = boundsMin.y + insetY, maxY = boundsMax.y - insetY;

            tracked.x = minX <= maxX ? Mathf.Clamp(tracked.x, minX, maxX) : (boundsMin.x + boundsMax.x) * 0.5f;
            tracked.y = minY <= maxY ? Mathf.Clamp(tracked.y, minY, maxY) : (boundsMin.y + boundsMax.y) * 0.5f;
        }
    }
}
