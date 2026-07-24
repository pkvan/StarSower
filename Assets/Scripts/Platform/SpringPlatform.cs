using UnityEngine;
using StarSower.Core;

namespace StarSower.Platform
{
    // Platform lò xo: khi Player chạm TỪ TRÊN, bắn Player lên với launchVelocity qua ILaunchable —
    // không phụ thuộc trực tiếp namespace Player, không sửa hệ thống jump. launchVelocity nên đặt
    // cao hơn jumpForce thường của Player để cảm giác "bật" rõ rệt (vd 18 so với 12).
    [RequireComponent(typeof(Collider2D))]
    public class SpringPlatform : MonoBehaviour
    {
        [Tooltip("Layer của Player.")]
        [SerializeField] private LayerMask playerLayer;

        [Tooltip("Vận tốc bắn Player lên (world units/giây). Đặt cao hơn jumpForce thường để bật cao hơn.")]
        [SerializeField] private float launchVelocity = 18f;

        [Tooltip("Player phải cao hơn tâm platform bao nhiêu mới tính là chạm từ trên (tránh bật khi va từ bên/dưới).")]
        [SerializeField] private float minHitHeight = 0.05f;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((playerLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;

            if (collision.transform.position.y < transform.position.y + minHitHeight)
                return;

            var launchable = collision.gameObject.GetComponent<ILaunchable>();
            if (launchable == null)
                return;

            Vector2 velocity = collision.rigidbody != null ? collision.rigidbody.linearVelocity : Vector2.zero;
            velocity.y = launchVelocity;
            launchable.Launch(velocity);
        }
    }
}
