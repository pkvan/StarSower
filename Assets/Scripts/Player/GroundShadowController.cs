using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Bong do duoi chan Hero (S2-002). La child RIENG cua root, khong nam trong "Visual" — nho vay
    // no khong bao gio bi lat theo flipX va khong bi Animator dong toi. Chi doc trang thai roi ghi
    // color.a + localScale cua CHINH no, khong dong toi vat ly.
    //
    // Bong khong ban tia xuong tim mat platform: no bam theo chan nhan vat. Dung dat thi day chinh
    // la mat platform; roi len thi mo dan va nho lai, du de mat cam nhan do cao ma khong can them
    // mot lan raycast moi frame tren mobile.
    public class GroundShadowController : MonoBehaviour
    {
        [Tooltip("Component implement IGroundDetector (vd: GroundChecker) tren root.")]
        [SerializeField] private MonoBehaviour groundDetectorSource;

        [SerializeField] private SpriteRenderer shadowRenderer;

        [Header("Do mo")]
        [Range(0f, 1f)]
        [SerializeField] private float groundedAlpha = 0.45f;

        [Range(0f, 1f)]
        [SerializeField] private float airborneAlpha = 0.18f;

        [Tooltip("Toc do chuyen do mo/kich thuoc (don vi/giay). De 0 thi doi tuc thi.")]
        [SerializeField] private float fadeSpeed = 6f;

        [Header("Kich thuoc")]
        [Range(0.1f, 1f)]
        [SerializeField] private float airborneScale = 0.72f;

        [Header("Ngoai vung choi")]
        [Tooltip("Player roi xuong duoi moc nay thi an han bong. Trung voi killFloorY cua " +
                 "GameOverManager de bong khong con lo lung mot minh sau khi nhan vat da roi khoi man.")]
        [SerializeField] private float hideBelowY = -12f;

        private IGroundDetector groundDetector;
        private Vector3 baseScale;
        private Color baseColor;
        private float currentAlpha;
        private float currentScale = 1f;

        private void Awake()
        {
            groundDetector = groundDetectorSource as IGroundDetector;
            baseScale = transform.localScale;
            baseColor = shadowRenderer.color;
            currentAlpha = groundedAlpha;
        }

        private void LateUpdate()
        {
            if (transform.position.y < hideBelowY)
            {
                ApplyAlpha(0f);
                return;
            }

            bool isGrounded = groundDetector.IsGrounded;
            float targetAlpha = isGrounded ? groundedAlpha : airborneAlpha;
            float targetScale = isGrounded ? 1f : airborneScale;

            if (fadeSpeed <= 0f)
            {
                currentAlpha = targetAlpha;
                currentScale = targetScale;
            }
            else
            {
                float step = fadeSpeed * Time.deltaTime;
                currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, step);
                currentScale = Mathf.MoveTowards(currentScale, targetScale, step);
            }

            ApplyAlpha(currentAlpha);
            transform.localScale = baseScale * currentScale;
        }

        private void ApplyAlpha(float alpha)
        {
            Color c = baseColor;
            c.a = alpha;
            shadowRenderer.color = c;
        }
    }
}
