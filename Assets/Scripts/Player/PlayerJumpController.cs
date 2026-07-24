using UnityEngine;

namespace StarSower.Player
{
    // Quyết định "có được phép nhảy ngay bây giờ không" qua Jump Buffer + Coyote Time.
    // Không tự đụng Rigidbody2D — PlayerController mới là nơi gọi PlayerMotor.Jump()
    // sau khi TryConsumeJump() trả về true. Không tự chạy Update/FixedUpdate riêng — được
    // PlayerController gọi Tick() theo đúng 1 thứ tự xác định, tránh phụ thuộc script execution order.
    public class PlayerJumpController : MonoBehaviour
    {
        [Tooltip("Khoảng thời gian sau khi bấm nhảy mà input vẫn còn hiệu lực, kể cả khi chưa chạm đất.")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Tooltip("Khoảng thời gian sau khi rời platform mà vẫn còn được phép nhảy.")]
        [SerializeField] private float coyoteTime = 0.1f;

        private float jumpBufferTimer;
        private float coyoteTimer;

        // Chỉ đọc — dùng cho debug HUD (PlayerController.OnGUI), không có ai khác cần đụng tới.
        public float JumpBufferTimer => jumpBufferTimer;
        public float CoyoteTimer => coyoteTimer;

        public void NotifyJumpPressed()
        {
            jumpBufferTimer = jumpBufferTime;
        }

        public void Tick(bool isGrounded, float deltaTime)
        {
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - deltaTime);
            coyoteTimer = isGrounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - deltaTime);
        }

        public bool TryConsumeJump()
        {
            if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
                return false;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            return true;
        }
    }
}
