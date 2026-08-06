using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Cac kha nang di chuyen mo rong cua S3-000: nhay doi, bam tuong, nhay tuong, lao, bay luon.
    //
    // Component nay chi RA LENH cho PlayerMotor — khong bao gio tu ghi Rigidbody2D. PlayerMotor van
    // la nguoi ghi duy nhat, dung quy tac Single-Writer di suot du an.
    //
    // Dat rieng thay vi nhet vao PlayerController: PlayerController dang giu dung mot viec (doc
    // input -> day xuong motor + jumpController). Gop them nam kha nang vao do se pha vo chuyen do,
    // va xoa component nay di thi game quay ve dung luong di chuyen cu, khong hong gi.
    public class PlayerAbilities : MonoBehaviour
    {
        [Header("Nguon trang thai")]
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private MonoBehaviour groundDetectorSource;
        [SerializeField] private WallDetector wallDetector;
        [SerializeField] private MonoBehaviour inputSource;

        [Header("Nhay doi")]
        [SerializeField] private bool doubleJumpEnabled = true;
        [Tooltip("Luc nhay lan hai. De thap hon jumpForce cua cu nhay dau (12) cho cam giac hut dan.")]
        [SerializeField] private float doubleJumpForce = 10f;

        [Header("Bam tuong")]
        [SerializeField] private bool wallSlideEnabled = true;
        [Tooltip("Toc do roi toi da khi dang bam tuong. Cang nho cang bam lau.")]
        [SerializeField] private float wallSlideSpeed = 2.5f;

        [Header("Nhay tuong")]
        [SerializeField] private bool wallJumpEnabled = true;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(9f, 12f);

        [Tooltip("Bao lau sau khi nhay tuong thi tra lai quyen dieu khien ngang. Khong co quang " +
                 "nay, giu cần huong vao tuong se dan nguoi choi ap nguoc lai va cu nhay thanh vo nghia.")]
        [SerializeField] private float wallJumpLockTime = 0.16f;

        [Header("Lao")]
        [SerializeField] private bool dashEnabled = true;
        [SerializeField] private float dashSpeed = 18f;
        [SerializeField] private float dashDuration = 0.16f;
        [Tooltip("Phai cho bao lau moi lao tiep.")]
        [SerializeField] private float dashCooldown = 0.5f;
        [Tooltip("Cho phep lao khi dang tren khong. Tat di thi chi lao duoc khi dung dat.")]
        [SerializeField] private bool airDashEnabled = true;

        [Header("Bay luon")]
        [SerializeField] private bool glideEnabled = true;
        [Tooltip("Toc do roi toi da khi dang bay luon.")]
        [SerializeField] private float glideFallSpeed = 2.2f;
        [Tooltip("Phai roi it nhat bay nhieu giay moi bat duoc bay luon. Chan viec giu nut nhay tu " +
                 "luc cat canh khien cu nhay thuong nao cung bien thanh bay luon.")]
        [SerializeField] private float glideDelay = 0.18f;

        private IGroundDetector ground;
        private IInputProvider input;

        private bool doubleJumpAvailable;
        private bool airDashAvailable;
        private float dashCooldownTimer;
        private float wallJumpLockTimer;
        private float airborneTime;
        private bool wasGrounded = true;

        // Quang khoa dieu khien ngang sau cu nhay tuong — PlayerController doc de biet co nen day
        // input ngang xuong motor hay khong.
        public bool HorizontalControlLocked => wallJumpLockTimer > 0f;

        // Dang bam tuong hay khong — PlayerAnimationController doc de chon hoat anh.
        public bool IsWallSliding { get; private set; }
        public bool IsGliding { get; private set; }
        public bool IsDashing => motor != null && motor.IsDashing;

        private void Awake()
        {
            ground = groundDetectorSource as IGroundDetector;
            input = inputSource as IInputProvider;
        }

        // Chay trong FixedUpdate cua PlayerController, TRUOC motor.Tick(): moi quyet dinh phai xong
        // truoc khi motor chot van toc cua buoc vat ly nay.
        public void Tick(float deltaTime)
        {
            if (motor == null || ground == null || input == null)
                return;

            bool grounded = ground.IsGrounded;
            int wall = wallDetector != null ? wallDetector.WallDirection : 0;

            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - deltaTime);
            wallJumpLockTimer = Mathf.Max(0f, wallJumpLockTimer - deltaTime);
            airborneTime = grounded ? 0f : airborneTime + deltaTime;

            // Cham dat la nap lai moi thu. Bam tuong cung nap lai — day la thu bien tuong tu cho
            // ngat quang thanh cho nghi, dung tinh than Ori chu khong phai Jump King.
            if (grounded || (wall != 0 && !grounded))
            {
                doubleJumpAvailable = doubleJumpEnabled;
                airDashAvailable = airDashEnabled;
            }
            wasGrounded = grounded;

            HandleWallSlide(grounded, wall);
            HandleDash(grounded);
            HandleAirJump(grounded, wall);
            HandleGlide(grounded);
        }

        private void HandleWallSlide(bool grounded, int wall)
        {
            bool sliding = wallSlideEnabled && !grounded && wall != 0
                           && motor.Velocity.y < 0f
                           && Mathf.Abs(input.Horizontal) > 0.2f
                           && Mathf.Sign(input.Horizontal) == wall;

            IsWallSliding = sliding;
            motor.SetWallSlide(sliding ? wallSlideSpeed : -1f);
        }

        private void HandleDash(bool grounded)
        {
            if (!dashEnabled || !input.DashPressed || dashCooldownTimer > 0f || motor.IsDashing)
                return;

            if (!grounded)
            {
                if (!airDashAvailable)
                    return;
                airDashAvailable = false;
            }

            // Lao theo huong dang bam; dung yen thi lao theo huong dang nhin.
            float dir = Mathf.Abs(input.Horizontal) > 0.2f ? Mathf.Sign(input.Horizontal) : Facing();
            motor.BeginDash(new Vector2(dir * dashSpeed, 0f), dashDuration);
            dashCooldownTimer = dashCooldown;
        }

        private void HandleAirJump(bool grounded, int wall)
        {
            if (grounded || !input.JumpPressed)
                return;

            // Nhay TUONG duoc uu tien hon nhay doi: dang ap tuong ma bam nhay thi y dinh gan nhu
            // chac chan la bat ra khoi tuong, khong phai nhay them mot nhip tai cho.
            if (wallJumpEnabled && wall != 0)
            {
                motor.CancelDash();
                motor.SetVelocity(new Vector2(-wall * wallJumpForce.x, wallJumpForce.y));
                wallJumpLockTimer = wallJumpLockTime;
                doubleJumpAvailable = doubleJumpEnabled;
                return;
            }

            if (!doubleJumpAvailable)
                return;

            doubleJumpAvailable = false;
            motor.CancelDash();
            motor.SetVelocity(new Vector2(motor.Velocity.x, doubleJumpForce));
        }

        private void HandleGlide(bool grounded)
        {
            bool gliding = glideEnabled && !grounded && !motor.IsDashing
                           && !IsWallSliding
                           && input.JumpHeld
                           && motor.Velocity.y < 0f
                           && airborneTime > glideDelay;

            IsGliding = gliding;
            motor.SetGlide(gliding ? glideFallSpeed : -1f);
        }

        private float Facing()
        {
            float vx = motor.Velocity.x;
            return Mathf.Abs(vx) > 0.05f ? Mathf.Sign(vx) : 1f;
        }
    }
}
