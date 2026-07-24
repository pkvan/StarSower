using UnityEngine;

namespace StarSower.Platform
{
    // Platform di chuyển ping-pong giữa vị trí gốc và (gốc + direction*distance) bằng Rigidbody2D
    // Kinematic + MovePosition (không đổi trực tiếp transform.position). Player đứng trên được ma sát
    // vật lý mang theo. direction chuẩn hoá nên có thể là ngang, dọc, hay chéo tuỳ cấu hình.
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveDistance = 3f;

        [Tooltip("Hướng di chuyển (sẽ được chuẩn hoá). Vd (1,0) = ngang, (0,1) = dọc.")]
        [SerializeField] private Vector2 direction = Vector2.right;

        private Rigidbody2D rb;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private Vector2 targetPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            startPosition = rb.position;
            endPosition = startPosition + direction.normalized * moveDistance;
            targetPosition = endPosition;
        }

        private void FixedUpdate()
        {
            Vector2 next = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(next);

            if ((next - targetPosition).sqrMagnitude <= Mathf.Epsilon)
                targetPosition = targetPosition == endPosition ? startPosition : endPosition;
        }
    }
}
