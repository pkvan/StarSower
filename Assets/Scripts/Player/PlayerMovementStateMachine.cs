using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Suy ra PlayerMovementState hiện tại từ vận tốc (PlayerMotor) + trạng thái grounded
    // (IGroundDetector). Thuần đọc — không System nào phụ thuộc ngược vào class này, nên
    // xoá được bất cứ lúc nào mà không ảnh hưởng gameplay. Chỗ để Animation/Audio/VFX
    // sau này lắng nghe CurrentState mà không cần biết PlayerMotor/GroundChecker tồn tại.
    public class PlayerMovementStateMachine : MonoBehaviour
    {
        [Tooltip("Tốc độ ngang tối thiểu để tính là đang chạy (Running) thay vì đứng yên (Idle).")]
        [SerializeField] private float idleSpeedThreshold = 0.1f;

        [Tooltip("Component implement IGroundDetector (vd: GroundChecker).")]
        [SerializeField] private MonoBehaviour groundDetectorSource;

        [SerializeField] private PlayerMotor motor;

        private IGroundDetector groundDetector;

        public PlayerMovementState CurrentState { get; private set; }

        private void Awake()
        {
            groundDetector = groundDetectorSource as IGroundDetector;
        }

        private void Update()
        {
            CurrentState = EvaluateState();
        }

        private PlayerMovementState EvaluateState()
        {
            Vector2 velocity = motor.Velocity;

            if (groundDetector.IsGrounded)
                return Mathf.Abs(velocity.x) > idleSpeedThreshold ? PlayerMovementState.Running : PlayerMovementState.Idle;

            return velocity.y > 0f ? PlayerMovementState.Jumping : PlayerMovementState.Falling;
        }
    }
}
