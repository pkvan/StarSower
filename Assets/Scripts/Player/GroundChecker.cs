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
    public class GroundChecker : MonoBehaviour, IGroundDetector
    {
        [Tooltip("Layer được tính là mặt đất/platform.")]
        [SerializeField] private LayerMask groundLayer;

        [Tooltip("Ngưỡng normal.y tối thiểu để coi 1 điểm chạm là 'mặt đất đỡ từ dưới lên' " +
                 "(1 = mặt phẳng tuyệt đối; thấp hơn = chấp nhận mặt dốc hơn). Mặt bên/tường có normal.y ~0 nên bị loại.")]
        [Range(0f, 1f)]
        [SerializeField] private float minGroundNormalY = 0.5f;

        public bool IsGrounded { get; private set; }

        private bool groundedThisStep;

        private void FixedUpdate()
        {
            // Chốt kết quả của bước physics trước rồi reset cho bước mới. Có độ trễ tối đa 1 bước
            // physics (~0.02s) so với va chạm thật, nhưng Coyote Time đã dư sức bù khoảng này.
            IsGrounded = groundedThisStep;
            groundedThisStep = false;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if ((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= minGroundNormalY)
                {
                    groundedThisStep = true;
                    return;
                }
            }
        }
    }
}
