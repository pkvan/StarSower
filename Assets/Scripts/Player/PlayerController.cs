using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Điều phối: đọc IInputProvider, hỏi IGroundDetector, ra lệnh cho PlayerMotor/PlayerJumpController.
    // Không tự đọc Input trực tiếp, không biết UI, không tự tính vật lý — chỉ gọi method đúng thứ tự.
    // Update() chỉ ghi nhận ý định (không đụng Rigidbody2D); FixedUpdate() là nơi DUY NHẤT
    // các lệnh vật lý thực sự được áp dụng, để không phụ thuộc framerate không ổn định trên mobile.
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerJumpController))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Component implement IInputProvider (vd: KeyboardInputProvider, MobileInputProvider).")]
        [SerializeField] private MonoBehaviour inputProviderSource;

        [Tooltip("Component implement IGroundDetector (vd: GroundChecker).")]
        [SerializeField] private MonoBehaviour groundDetectorSource;

        [Tooltip("Bật để in log input/velocity/position mỗi frame — dùng tạm lúc debug, tắt khi xong.")]
        [SerializeField] private bool debugLogging = false;

        private IInputProvider input;
        private IGroundDetector groundDetector;
        private PlayerMotor motor;
        private PlayerJumpController jumpController;
        private bool movementLocked;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            jumpController = GetComponent<PlayerJumpController>();
            input = inputProviderSource as IInputProvider;
            groundDetector = groundDetectorSource as IGroundDetector;
        }

        // Logic phản ứng khi Player chết nằm ở đây, không phải trong GameOverManager —
        // GameOverManager chỉ phát hiện + phát event, không cần biết PlayerController tồn tại.
        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void HandleGameOver()
        {
            SetInputEnabled(false);
        }

        private void Update()
        {
            float horizontal = movementLocked ? 0f : input.Horizontal;
            motor.SetMoveInput(horizontal, groundDetector.IsGrounded);

            // Đọc đúng 1 lần — trên Mobile, JumpPressed có side-effect "tiêu thụ" input
            // (ConsumePress trong TouchButton), đọc 2 lần trong cùng frame có thể mất input.
            // Vẫn đọc kể cả khi movementLocked để ConsumePress() chạy, tránh giữ input "kẹt".
            bool jumpPressed = input.JumpPressed;
            if (jumpPressed && !movementLocked)
                jumpController.NotifyJumpPressed();

            if (debugLogging)
            {
                Debug.Log($"[PlayerController] Horizontal={horizontal:F2} JumpPressed={jumpPressed} " +
                          $"JumpHeld={input.JumpHeld} Velocity={motor.Velocity} Position={transform.position}");
            }
        }

        private void FixedUpdate()
        {
            jumpController.Tick(groundDetector.IsGrounded, Time.fixedDeltaTime);

            if (jumpController.TryConsumeJump())
                motor.Jump();

            motor.Tick(Time.fixedDeltaTime, input.JumpHeld);
        }

        // Tắt/bật hoàn toàn điều khiển Player (input + vật lý) — dùng cho cinematic, cutscene...
        // Tắt component này dừng luôn Update()/FixedUpdate() ở trên; PlayerMotor lo phần Rigidbody2D.
        public void SetInputEnabled(bool isEnabled)
        {
            enabled = isEnabled;
            motor.SetPhysicsActive(isEnabled);
        }

        // Khoá riêng phần di chuyển (đứng yên, không nhảy được) nhưng GIỮ NGUYÊN vật lý/Update
        // đang chạy — dùng khi chạm Goal, để animation (Idle/Celebrate, khi có Animator sau này)
        // và trạng thái grounded vẫn cập nhật bình thường. Khác SetInputEnabled(false): cái đó tắt
        // hẳn component + đóng băng Rigidbody2D, chỉ hợp cho Game Over.
        public void SetMovementLocked(bool locked)
        {
            movementLocked = locked;
        }

        // HUD debug tạm thời — chỉ hiện khi debugLogging bật, để đọc số liệu trực tiếp qua
        // ảnh chụp màn hình thay vì phải nối máy lấy log Console. Xoá khi hết cần debug.
        private void OnGUI()
        {
            if (!debugLogging)
                return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                normal = { textColor = Color.yellow }
            };

            string text =
                $"Grounded: {groundDetector.IsGrounded}\n" +
                $"PhysicsActive: {motor.IsPhysicsActive}\n" +
                $"IsSleeping: {motor.IsSleeping}\n" +
                $"JumpBufferTimer: {jumpController.JumpBufferTimer:F3}\n" +
                $"CoyoteTimer: {jumpController.CoyoteTimer:F3}\n" +
                $"JumpHeld: {input.JumpHeld}\n" +
                $"VelocityY: {motor.Velocity.y:F2}\n" +
                $"PlayerPos: {transform.position.x:F2}, {transform.position.y:F2}";

            GUI.Label(new Rect(20, 80, 900, 500), text, style);
        }
    }
}
