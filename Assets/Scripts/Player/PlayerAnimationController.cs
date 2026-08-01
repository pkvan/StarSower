using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Cau noi MOT CHIEU giua trang thai di chuyen va Animator (S2-002). Chi ĐỌC PlayerMotor +
    // IGroundDetector rồi ghi tham so Animator — khong dong toi Rigidbody2D, khong quyet dinh gi ve
    // vat ly, nen xoa component nay di thi gameplay van chay y nguyen, chi mat phan hinh anh.
    //
    // Dat rieng thay vi nhet them vao PlayerController: PlayerController dang giu dung mot viec
    // (dieu phoi input -> vat ly). Them phan doi hinh vao do se pha vo chuyen do.
    //
    // Lat huong dung SpriteRenderer.flipX tren CHILD "Visual". Khong bao gio lat scale cua root:
    // root dang mang Rigidbody2D, Collider2D va GroundCheck — lat scale am se lat luon vung va cham.
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Nguon trang thai (chi doc)")]
        [Tooltip("Component implement IGroundDetector (vd: GroundChecker).")]
        [SerializeField] private MonoBehaviour groundDetectorSource;

        [SerializeField] private PlayerMotor motor;

        [Header("Hinh anh")]
        [Tooltip("Animator nam tren child 'Visual'.")]
        [SerializeField] private Animator animator;

        [Tooltip("SpriteRenderer nam tren child 'Visual' — DUY NHAT thu duoc phep lat.")]
        [SerializeField] private SpriteRenderer visualRenderer;

        [Header("Nguong")]
        [Tooltip("Vung chet cua huong nhin: toc do ngang nho hon nguong nay thi GIU nguyen huong cu, " +
                 "khong lat. Tranh nhan vat rung qua rung lai luc gan dung yen.")]
        [SerializeField] private float facingDeadZone = 0.15f;

        [Tooltip("Van toc doc di len vuot qua nguong nay thi coi la vua bat dau nhay. Bat theo " +
                 "van toc chu khong theo input: nho vay cu bat cua SpringPlatform (ILaunchable) " +
                 "cung dien Jump ma khong phai noi them day nao.")]
        [SerializeField] private float jumpDetectSpeed = 4f;

        [Tooltip("Phai o tren khong it nhat bay nhieu giay thi cham dat moi tinh la tiep dat. " +
                 "Chan hai truong hop: vua spawn, va grounded nhap nhay 1 buoc vat ly khi di qua khe.")]
        [SerializeField] private float minAirborneTimeForLanding = 0.08f;

        [Tooltip("Van toc doc phai am hon nguong nay luc cham dat moi tinh la tiep dat — " +
                 "di xuong doc thoai thoai se khong kich hoat.")]
        [SerializeField] private float landDetectSpeed = -0.5f;

        // Bam san ID tham so: Animator.SetFloat(string) phai bam chuoi moi lan goi, chay moi frame
        // tren mobile thi khong dang.
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int JumpTriggeredHash = Animator.StringToHash("JumpTriggered");
        private static readonly int LandTriggeredHash = Animator.StringToHash("LandTriggered");
        private static readonly int IdleStateHash = Animator.StringToHash("Idle");

        private IGroundDetector groundDetector;
        private float airborneTime;
        private float previousVerticalVelocity;
        private bool wasGrounded = true;   // coi nhu dang dung dat luc spawn -> khong dien Landing

        // Cho canh dien (vd Hero chay vao khung o man chom sao) muon tay lai hoat anh trong khi
        // vat ly da bi dong bang. Luc do van toc that bang 0 nen Animator se hien Idle — phai co
        // duong bao thang cho no biet "dang chay".
        private bool scriptedActive;
        private float scriptedSpeed;

        private void Awake()
        {
            groundDetector = groundDetectorSource as IGroundDetector;
            BaseVisualSortingOrder = visualRenderer.sortingOrder;
        }

        // Bat/tat che do canh dien. horizontalSpeed > 0 la chay sang phai, < 0 sang trai, 0 la dung.
        public void SetScriptedMotion(bool active, float horizontalSpeed)
        {
            scriptedActive = active;
            scriptedSpeed = horizontalSpeed;
        }

        // Do mo cua phan hinh. Dat o day chu khong de ben ngoai tu ghi thang vao SpriteRenderer:
        // component nay von da la nguoi duy nhat dong toi visualRenderer (flipX), gom luon mau vao
        // day thi khong bao gio co hai noi cung ghi mot thu.
        // Thu tu ve goc, ghi lai luc Awake. Canh chom sao muon nang Hero len tren nen troi roi
        // tra lai — phai nho so cu chu khong hardcode, vi so do nam trong prefab.
        public int BaseVisualSortingOrder { get; private set; }

        public void SetVisualSortingOrder(int order)
        {
            visualRenderer.sortingOrder = order;
        }

        public void RestoreVisualSortingOrder()
        {
            visualRenderer.sortingOrder = BaseVisualSortingOrder;
        }

        public void SetVisualAlpha(float alpha)
        {
            Color c = visualRenderer.color;
            c.a = Mathf.Clamp01(alpha);
            visualRenderer.color = c;
        }

        private void Update()
        {
            if (scriptedActive)
            {
                animator.SetFloat(SpeedHash, Mathf.Abs(scriptedSpeed));
                animator.SetFloat(VerticalVelocityHash, 0f);
                animator.SetBool(IsGroundedHash, true);
                UpdateFacing(scriptedSpeed);
                return;
            }

            Vector2 velocity = motor.Velocity;
            bool isGrounded = groundDetector.IsGrounded;

            animator.SetFloat(SpeedHash, Mathf.Abs(velocity.x));
            animator.SetFloat(VerticalVelocityHash, velocity.y);
            animator.SetBool(IsGroundedHash, isGrounded);

            // Bat dau nhay: van toc doc VUOT QUA nguong trong dung frame nay. So voi frame truoc
            // thay vi chi kiem tra "> nguong", neu khong se ban trigger lien tuc suot ca cu nhay.
            if (velocity.y > jumpDetectSpeed && previousVerticalVelocity <= jumpDetectSpeed)
                animator.SetTrigger(JumpTriggeredHash);

            if (isGrounded)
            {
                bool justLanded = !wasGrounded
                                  && airborneTime >= minAirborneTimeForLanding
                                  && previousVerticalVelocity <= landDetectSpeed;

                if (justLanded)
                    animator.SetTrigger(LandTriggeredHash);

                airborneTime = 0f;
            }
            else
            {
                airborneTime += Time.deltaTime;
            }

            UpdateFacing(velocity.x);

            wasGrounded = isGrounded;
            previousVerticalVelocity = velocity.y;
        }

        // Chi lat rieng phan hinh. Duoi vung chet thi giu nguyen huong dang co, nen dung yen van
        // nhin ve phia vua di — khong tu quay ve mac dinh.
        //
        // Tranh GOC ve nhan vat nhin sang TRAI (do kiem: mat nam ben trai tam dau, ao choang trai
        // ve ben phai — o ca Run, Jump, Fall, Landing). Vi vay di sang PHAI moi la luc phai lat.
        private void UpdateFacing(float horizontalVelocity)
        {
            if (Mathf.Abs(horizontalVelocity) < facingDeadZone)
                return;

            visualRenderer.flipX = horizontalVelocity > 0f;
        }

        // Dua Animator ve dung trang thai dang co ngay lap tuc — dung sau khi hoi sinh, de nhan vat
        // khong con ket o khung Fall cua lan chet truoc.
        public void ResetVisualState()
        {
            airborneTime = 0f;
            previousVerticalVelocity = 0f;
            wasGrounded = true;

            animator.ResetTrigger(JumpTriggeredHash);
            animator.ResetTrigger(LandTriggeredHash);
            animator.SetFloat(SpeedHash, 0f);
            animator.SetFloat(VerticalVelocityHash, 0f);
            animator.SetBool(IsGroundedHash, true);
            animator.Play(IdleStateHash, 0, 0f);
        }
    }
}
