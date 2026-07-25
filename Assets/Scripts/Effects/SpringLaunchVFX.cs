using UnityEngine;
using StarSower.Player;

namespace StarSower.Effects
{
    // Phát particle khi Player bị Spring Platform bắn đi. Không móc trực tiếp vào SpringPlatform
    // (tránh đụng code Platform Mechanics hiện có) — nhận biết qua việc VelocityY tăng vọt vượt
    // ngưỡng chỉ Spring mới tạo ra (launchVelocity mặc định 18, cao hơn hẳn jumpForce 12).
    public class SpringLaunchVFX : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private ParticleSystem launchParticle;

        [Tooltip("VelocityY tối thiểu để tính là 'vừa bị Spring bắn' (đặt cao hơn jumpForce thường).")]
        [SerializeField] private float springVelocityThreshold = 14f;

        private float previousVelocityY;

        private void Update()
        {
            float currentVelocityY = motor.Velocity.y;

            if (currentVelocityY >= springVelocityThreshold && previousVelocityY < springVelocityThreshold && launchParticle != null)
                launchParticle.Play();

            previousVelocityY = currentVelocityY;
        }
    }
}
