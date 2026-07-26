using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Áp dụng di chuyển/nhảy lên Rigidbody2D. Không đọc input, không quyết định "được phép nhảy" —
    // chỉ biết cách thực thi vật lý cho intent đã được PlayerController quyết định.
    // Toàn bộ ghi Rigidbody2D dồn vào Tick(), gọi từ FixedUpdate của PlayerController —
    // SetMoveInput() chỉ lưu intent nên gọi từ Update() an toàn, không phải physics write.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMotor : MonoBehaviour, ILaunchable
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 60f;
        [SerializeField] private float deceleration = 80f;

        [Tooltip("Tỉ lệ moveSpeed còn giữ được khi đang ở trên không (1 = full control).")]
        [Range(0f, 1f)]
        [SerializeField] private float airControlMultiplier = 0.8f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Nhân thêm vào gravity lúc đang lên VÀ còn giữ nút nhảy (Variable Jump Height).")]
        [SerializeField] private float gravityMultiplier = 1f;

        [Tooltip("Nhân thêm vào gravity lúc đang lên nhưng đã thả nút nhảy sớm — tạo cú nhảy thấp (short hop).")]
        [SerializeField] private float lowJumpMultiplier = 3.5f;

        [Tooltip("Nhân thêm vào gravity lúc đang rơi — rơi nhanh hơn lúc lên để cảm giác nhảy không bị ì.")]
        [SerializeField] private float fallMultiplier = 2.5f;

        [Tooltip("Thời gian tối thiểu ngay sau khi bắt đầu nhảy mà gravity vẫn tính như đang giữ nút, " +
                 "bất kể jumpHeld đọc được gì. Bảo vệ cú tap nhanh (bấm-thả gần như cùng lúc) khỏi bị " +
                 "lowJumpMultiplier dập tắt ngay từ khung hình đầu tiên do đọc jumpHeld không kịp.")]
        [SerializeField] private float minAscentGraceTime = 0.08f;

        private Rigidbody2D rb;
        private float targetHorizontalSpeed;
        private float ascentGraceTimer;
        private float surfaceFriction = 1f;
        private float surfaceDriftSpeed;
        private bool isGrounded;
        private float lastFacing = 1f;

        // Chỉ đọc — PlayerMotor vẫn là nơi duy nhất ghi Rigidbody2D. Dùng cho
        // PlayerMovementStateMachine (và Animation/Audio sau này) suy ra trạng thái.
        public Vector2 Velocity => rb.linearVelocity;

        // Chỉ đọc — dùng cho debug HUD, để xác nhận Rigidbody2D có đang bị "đóng băng" không.
        public bool IsPhysicsActive => rb.simulated;

        // Chỉ đọc — dùng cho debug HUD, để xác nhận Rigidbody2D có đang bị Unity tự cho "ngủ" không.
        public bool IsSleeping => rb.IsSleeping();

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Chỉ lưu ý định di chuyển — an toàn khi gọi từ Update(), không ghi Rigidbody2D.
        public void SetMoveInput(float horizontal, bool isGrounded)
        {
            this.isGrounded = isGrounded;

            float controlScale = isGrounded ? 1f : airControlMultiplier;
            targetHorizontalSpeed = horizontal * moveSpeed * controlScale;

            // Nhớ hướng vừa đi để lúc buông tay trên băng còn biết trôi về phía nào.
            if (!Mathf.Approximately(horizontal, 0f))
                lastFacing = Mathf.Sign(horizontal);
        }

        // Ma sát của bề mặt đang đứng (S1-017). API CỘNG THÊM, giống Launch() — không đụng luồng
        // Move/Jump/Tick sẵn có. Chỉ lưu số, việc áp dụng nằm trong Tick() để PlayerMotor vẫn là
        // nơi DUY NHẤT ghi Rigidbody2D.
        public void SetSurface(float frictionMultiplier, float driftSpeed)
        {
            surfaceFriction = Mathf.Max(0.01f, frictionMultiplier);
            surfaceDriftSpeed = Mathf.Max(0f, driftSpeed);
        }

        public void Jump()
        {
            rb.WakeUp();
            Vector2 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
            ascentGraceTimer = minAscentGraceTime;
        }

        // ILaunchable — bị hệ thống ngoài (vd: SpringPlatform) bắn đi với vận tốc cho trước.
        // Đặt ascentGraceTimer để đoạn đầu cú bật không bị lowJumpMultiplier cắt ngắn ngay.
        // Đây là API CỘNG THÊM, không đụng gì tới luồng Move/Jump/Tick hiện có.
        public void Launch(Vector2 velocity)
        {
            rb.WakeUp();
            rb.linearVelocity = velocity;
            ascentGraceTimer = minAscentGraceTime;
        }

        // Đóng băng/khôi phục vật lý hoàn toàn (dùng cho cinematic, cutscene...).
        // PlayerMotor vẫn là nơi duy nhất đụng Rigidbody2D — bên gọi không cần biết chi tiết.
        public void SetPhysicsActive(bool isActive)
        {
            rb.simulated = isActive;
            if (isActive)
                return;

            rb.linearVelocity = Vector2.zero;
        }

        // Nơi DUY NHẤT ghi Rigidbody2D cho di chuyển/gravity — gọi từ FixedUpdate.
        public void Tick(float deltaTime, bool jumpHeld)
        {
            Vector2 velocity = rb.linearVelocity;

            // Đứng trên mặt trơn mà buông tay thì KHÔNG đứng im được: thay vì hãm về 0, nhân vật bị
            // đẩy tới một vận tốc trôi. Đây mới là thứ làm nên cảm giác băng — chỉ giảm ma sát là
            // chưa đủ, vì đứng im vốn đã có vận tốc 0 nên chẳng có gì để hãm.
            //
            // Hướng trôi = hướng đang đi; đứng yên hẳn thì lấy hướng vừa nhìn. Không random, không
            // đẩy về phía mép — người chơi luôn đoán được mình sắp trôi đi đâu.
            bool releasedOnSlippery = isGrounded
                                      && surfaceDriftSpeed > 0f
                                      && Mathf.Approximately(targetHorizontalSpeed, 0f);

            if (releasedOnSlippery)
            {
                float direction = Mathf.Abs(velocity.x) > 0.05f ? Mathf.Sign(velocity.x) : lastFacing;
                float driftTarget = surfaceDriftSpeed * direction;
                velocity.x = Mathf.MoveTowards(velocity.x, driftTarget,
                                               deceleration * surfaceFriction * deltaTime);
            }
            else
            {
                // CHỈ deceleration bị bề mặt làm chậm lại. acceleration giữ nguyên là có chủ ý: trên
                // băng, người chơi vẫn tăng tốc nhanh như thường (điều khiển còn nhạy), chỉ là DỪNG
                // lâu hơn. Nếu bóp cả acceleration thì nhân vật hoá ra ì ạch — đúng kiểu ức chế mà
                // yêu cầu thiết kế cấm. Nhờ vậy bấm ngược lại là ghìm được ngay, trôi không thành bẫy.
                bool speedingUp = Mathf.Abs(targetHorizontalSpeed) > Mathf.Abs(velocity.x);
                float rate = speedingUp ? acceleration : deceleration * surfaceFriction;
                velocity.x = Mathf.MoveTowards(velocity.x, targetHorizontalSpeed, rate * deltaTime);
            }

            bool effectiveJumpHeld = jumpHeld || ascentGraceTimer > 0f;
            ascentGraceTimer = Mathf.Max(0f, ascentGraceTimer - deltaTime);

            float gravityScaleMultiplier;
            if (velocity.y > 0f)
                gravityScaleMultiplier = effectiveJumpHeld ? gravityMultiplier : lowJumpMultiplier;
            else
                gravityScaleMultiplier = fallMultiplier;

            if (!Mathf.Approximately(gravityScaleMultiplier, 1f))
            {
                float extraGravity = Physics2D.gravity.y * rb.gravityScale * (gravityScaleMultiplier - 1f);
                velocity.y += extraGravity * deltaTime;
            }

            rb.linearVelocity = velocity;
        }
    }
}
