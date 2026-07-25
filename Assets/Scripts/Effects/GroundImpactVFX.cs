using UnityEngine;
using StarSower.Core;
using StarSower.Player;

namespace StarSower.Effects
{
    // Phát particle khi Player tiếp đất sau khi rơi. Chỉ đọc IGroundDetector/PlayerMotor
    // (không ghi gì) nên không ảnh hưởng luồng vật lý hiện có — gắn thêm cạnh Player là đủ.
    public class GroundImpactVFX : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour groundDetectorSource;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private ParticleSystem impactParticle;

        [Tooltip("Vận tốc rơi tối thiểu (giá trị tuyệt đối) mới coi là 'tiếp đất' đáng để hiện hiệu ứng.")]
        [SerializeField] private float minImpactSpeed = 4f;

        private IGroundDetector groundDetector;
        private bool wasGrounded;
        private float lastFallSpeed;

        private void Awake()
        {
            groundDetector = groundDetectorSource as IGroundDetector;
        }

        private void Update()
        {
            bool isGrounded = groundDetector.IsGrounded;

            if (!isGrounded)
                lastFallSpeed = motor.Velocity.y;

            if (isGrounded && !wasGrounded && -lastFallSpeed >= minImpactSpeed && impactParticle != null)
                impactParticle.Play();

            wasGrounded = isGrounded;
        }
    }
}
