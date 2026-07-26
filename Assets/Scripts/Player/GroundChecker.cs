using UnityEngine;
using StarSower.Core;

namespace StarSower.Player
{
    // Phát hiện đứng đất bằng contact normal của va chạm THỰC TẾ, thay vì overlap 1 vùng cố định.
    // Lý do: khi Player đứng chênh vênh ở mép/góc platform, chỉ 1 phần nhỏ collider tựa lên platform
    // — 1 vùng dò cố định (canh giữa Player) có thể không chạm platform dù Player vẫn đứng vững về mặt
    // vật lý, gây Grounded=False sai và không nhảy được. Kiểm tra contact normal bắt đúng câu hỏi
    // "có bề mặt nào đang đỡ Player từ dưới lên không", đúng cho mọi vị trí kể cả đứng trên mép.
    [RequireComponent(typeof(Collider2D))]
    public class GroundChecker : MonoBehaviour, IGroundDetector, ISurfaceProvider
    {
        [Tooltip("Layer được tính là mặt đất/platform.")]
        [SerializeField] private LayerMask groundLayer;

        [Tooltip("Ngưỡng normal.y tối thiểu để coi 1 điểm chạm là 'mặt đất đỡ từ dưới lên' " +
                 "(1 = mặt phẳng tuyệt đối; thấp hơn = chấp nhận mặt dốc hơn). Mặt bên/tường có normal.y ~0 nên bị loại.")]
        [Range(0f, 1f)]
        [SerializeField] private float minGroundNormalY = 0.5f;

        public bool IsGrounded { get; private set; }

        // Ma sát của bề mặt đang đỡ Player (S1-017). 1 = mặt thường; nhỏ hơn = trơn.
        // Thông tin này VỐN ĐÃ có sẵn trong OnCollisionStay2D — trước đây bị vứt đi sau khi trả
        // lời xong câu hỏi grounded. Giữ lại nó rẻ hơn nhiều so với dò lại bề mặt bằng raycast riêng.
        public float SurfaceFriction { get; private set; } = 1f;

        // Tốc độ trôi của bề mặt đang đỡ Player. 0 = đứng yên được (mọi mặt đất thường).
        public float SurfaceDriftSpeed { get; private set; }

        private bool groundedThisStep;
        private float driftThisStep;

        // -1 = chưa gặp bề mặt nào trong bước này. KHÔNG khởi tạo bằng 1f: dưới kia lấy Mathf.Max
        // nên mốc 1f sẽ nuốt mất mọi giá trị trơn (max(1, 0.25) luôn là 1) — băng sẽ không bao giờ trượt.
        private const float NoSurface = -1f;
        private float frictionThisStep = NoSurface;

        private void FixedUpdate()
        {
            // Chốt kết quả của bước physics trước rồi reset cho bước mới. Có độ trễ tối đa 1 bước
            // physics (~0.02s) so với va chạm thật, nhưng Coyote Time đã dư sức bù khoảng này.
            IsGrounded = groundedThisStep;
            SurfaceFriction = frictionThisStep > 0f ? frictionThisStep : 1f;
            SurfaceDriftSpeed = groundedThisStep ? driftThisStep : 0f;

            groundedThisStep = false;
            frictionThisStep = NoSurface;
            driftThisStep = 0f;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y < minGroundNormalY)
                    continue;

                groundedThisStep = true;

                // Đứng vắt qua hai platform khác loại thì lấy bên BÁM HƠN. Chọn max thay vì min là
                // có chủ ý: người chơi chạm được một mép đá thường giữa vùng băng phải cảm thấy
                // "bám lại được", không phải "vẫn trượt". Thua thiệt cho người chơi là không công bằng.
                var surface = collision.gameObject.GetComponent<IGroundSurface>();
                float friction = surface != null ? surface.FrictionMultiplier : 1f;

                // Bề mặt BÁM HƠN thắng, và mang theo luôn độ trôi CỦA CHÍNH NÓ — không trộn ma sát
                // của mặt này với độ trôi của mặt kia, vì như thế sẽ ra một bề mặt không hề tồn tại.
                if (friction > frictionThisStep)
                {
                    frictionThisStep = friction;
                    driftThisStep = surface != null ? surface.DriftSpeed : 0f;
                }
                return;
            }
        }
    }
}
