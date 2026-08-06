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

        [Header("Gioi han ngang (S2-004)")]
        [Tooltip("Chan Player ra khoi be rong choi duoc. Tat thi moi thu chay y nhu truoc.")]
        [SerializeField] private bool useHorizontalBounds = true;

        [Tooltip("Tam ngang cua man — trung voi Level Center X cua CameraAspectFitter.")]
        [SerializeField] private float boundsCenterX;

        [Tooltip("Nua be rong choi duoc = playableWidth / 2. Phai khop CameraAspectFitter, " +
                 "neu khong Player se dung o cho khac voi mep man hinh.")]
        [SerializeField] private float boundsHalfWidth = 2.6f;

        [Tooltip("Nua be ngang cua Player, tru vao gioi han de nguoi choi khong bi lo nua nguoi ra " +
                 "ngoai mep. 0.375 = collider 1x1 nhan scale root 0.75, chia doi.")]
        [SerializeField] private float playerHalfWidth = 0.375f;

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

        // S3-R3 — dời Player tới một vị trí và xoá sạch quán tính (dùng cho hồi sinh tại mốc).
        // API CỘNG THÊM, không đụng luồng Move/Jump/Tick.
        //
        // Dùng rb.position chứ không phải transform.position: ghi thẳng transform sẽ khiến
        // Rigidbody2D chỉ thấy vị trí mới ở bước physics kế tiếp, đủ để va chạm bị tính ở chỗ CŨ
        // và Player kẹt trong sàn ngay lúc vừa hồi sinh.
        //
        // Xoá luôn dashTimer/wallSlide/glide: hồi sinh giữa một cú lao hay đang bám tường mà
        // không dọn thì trạng thái đó theo người chơi về tận mốc.
        public void Teleport(Vector2 position)
        {
            rb.position = position;
            rb.linearVelocity = Vector2.zero;
            rb.WakeUp();

            dashTimer = 0f;
            glideFallSpeed = -1f;
            wallSlideSpeed = -1f;
            ascentGraceTimer = 0f;
            targetHorizontalSpeed = 0f;
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

        // ---- S3-000: cac kha nang moi deu di qua day ----
        //
        // Moi thu ghi Rigidbody2D van nam trong PlayerMotor. PlayerAbilities chi RA LENH, khong tu
        // dong vao rigidbody — giu dung Single-Writer da theo suot du an.

        private float dashTimer;
        private Vector2 dashVelocity;
        private float glideFallSpeed = -1f;
        private float wallSlideSpeed = -1f;

        public bool IsDashing => dashTimer > 0f;

        // Lao mot doan: trong suot quang nay bo qua trong luc va dieu khien ngang, nen cu lao la
        // mot duong thang du dang roi hay dang len.
        public void BeginDash(Vector2 velocity, float duration)
        {
            dashVelocity = velocity;
            dashTimer = Mathf.Max(0f, duration);
            rb.linearVelocity = velocity;
        }

        public void CancelDash()
        {
            dashTimer = 0f;
        }

        // Bay luon: gioi han toc do ROI, khong dong toi trong luc. Dat -1 de tat.
        public void SetGlide(float maxFallSpeed)
        {
            glideFallSpeed = maxFallSpeed;
        }

        // Bam tuong: cung la gioi han toc do roi nhung cham hon nua. Dat -1 de tat.
        public void SetWallSlide(float maxFallSpeed)
        {
            wallSlideSpeed = maxFallSpeed;
        }

        // Nhay tuong / nhay doi: ghi thang van toc, khong cong don — cong don thi nhay doi luc dang
        // bay len se bay vot gap doi.
        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
            ascentGraceTimer = minAscentGraceTime;
        }

        // Nơi DUY NHẤT ghi Rigidbody2D cho di chuyển/gravity — gọi từ FixedUpdate.
        public void Tick(float deltaTime, bool jumpHeld)
        {
            // Dang lao: giu nguyen van toc lao, khong trong luc, khong dieu khien.
            if (dashTimer > 0f)
            {
                dashTimer -= deltaTime;
                rb.linearVelocity = dashVelocity;
                return;
            }

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

            // Bam tuong khat hon bay luon: lay gioi han NGHIEM NGAT hon trong hai cai dang bat.
            float fallLimit = -1f;
            if (wallSlideSpeed >= 0f) fallLimit = wallSlideSpeed;
            if (glideFallSpeed >= 0f) fallLimit = fallLimit < 0f ? glideFallSpeed : Mathf.Min(fallLimit, glideFallSpeed);
            if (fallLimit >= 0f && velocity.y < -fallLimit)
                velocity.y = -fallLimit;

            ApplyHorizontalBounds(ref velocity, deltaTime);

            rb.linearVelocity = velocity;
        }

        // Giu Player trong be rong choi duoc. Dat o day vi PlayerMotor la nơi DUY NHAT duoc ghi
        // Rigidbody2D — kep vi tri o mot component khac se thanh hai nguoi cung ghi.
        //
        // Tru playerHalfWidth de moc tinh theo MEP nguoi choi chu khong phai tam: kep theo tam thi
        // nua nguoi van tho ra ngoai khung hinh.
        //
        // Triet tieu velocity.x khi cham moc, neu khong van toc doi vao "tuong" cu tich lai, buong
        // tay ra la bat nguoc — va tren bang trôi (surfaceDriftSpeed) se ri ra ngoai tung chut mot.
        private void ApplyHorizontalBounds(ref Vector2 velocity, float deltaTime)
        {
            if (!useHorizontalBounds)
                return;

            float limit = boundsHalfWidth - playerHalfWidth;
            if (limit <= 0f)
                return;

            float minX = boundsCenterX - limit;
            float maxX = boundsCenterX + limit;
            float x = rb.position.x;

            // Chan TRUOC khi vuot, khong phai keo ve sau khi da vuot: bop van toc vua du de buoc
            // physics ke tiep dung lai chinh xac tai moc. Kep vi tri sau khi lo se lo toi
            // moveSpeed * fixedDeltaTime = 0.1 unit roi moi giat nguoc lai — nhin thay ro o mep.
            if (velocity.x < 0f && x + velocity.x * deltaTime < minX)
                velocity.x = deltaTime > 0f ? (minX - x) / deltaTime : 0f;
            else if (velocity.x > 0f && x + velocity.x * deltaTime > maxX)
                velocity.x = deltaTime > 0f ? (maxX - x) / deltaTime : 0f;

            // Luoi an toan: da nam ngoai san (spawn sai cho, bi Launch ban ra) thi keo thang ve.
            if (x < minX || x > maxX)
                rb.position = new Vector2(Mathf.Clamp(x, minX, maxX), rb.position.y);
        }
    }
}
