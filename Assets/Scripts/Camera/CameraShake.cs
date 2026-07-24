using UnityEngine;
using StarSower.Core;

namespace StarSower.CameraSystem
{
    // Rung camera bằng offset ngẫu nhiên giảm dần theo thời gian. Không tự ghi transform.position —
    // chỉ tính CurrentOffset để CameraFollowY (chủ sở hữu transform.position) cộng vào mỗi frame,
    // tránh việc 2 script cùng ghi đè vị trí camera và làm hỏng độ mượt của SmoothDamp.
    public class CameraShake : MonoBehaviour, ICameraShake
    {
        [Tooltip("Dùng khi gọi Shake() không tham số.")]
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float defaultMagnitude = 0.2f;

        public Vector3 CurrentOffset { get; private set; }

        private float activeDuration;
        private float remainingTime;
        private float currentMagnitude;

        public void Shake(float duration, float magnitude)
        {
            activeDuration = Mathf.Max(duration, 0.0001f);
            remainingTime = duration;
            currentMagnitude = magnitude;
        }

        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude);
        }

        private void Update()
        {
            if (remainingTime <= 0f)
            {
                CurrentOffset = Vector3.zero;
                return;
            }

            remainingTime -= Time.deltaTime;
            float damper = Mathf.Clamp01(remainingTime / activeDuration);
            CurrentOffset = (Vector3)(Random.insideUnitCircle * currentMagnitude * damper);
        }
    }
}
