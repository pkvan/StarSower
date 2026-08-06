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

        // S3-002 — ba cấp rung có tên, thay cho việc mỗi nơi gọi tự bịa một cặp số.
        //
        // Gom lại đây vì độ mạnh của rung là chuyện CẢM GIÁC CHUNG của game, không phải chuyện
        // riêng của cái cổng hay ngôi sao. Muốn cả game rung nhẹ đi thì sửa một chỗ, không phải đi
        // lùng từng lời gọi. StarSower cần cảm giác bình yên nên cả ba cấp đều nhỏ hơn hẳn mức
        // một game hành động sẽ dùng.
        [Header("Cấp rung (S3-002)")]
        [Tooltip("Tiếp đất, nhặt sao.")]
        [SerializeField] private Vector2 smallShake = new Vector2(0.14f, 0.05f);

        [Tooltip("Astral Gate mở ra.")]
        [SerializeField] private Vector2 mediumShake = new Vector2(0.22f, 0.10f);

        [Tooltip("Sự kiện lớn — dành sẵn, chưa nơi nào gọi.")]
        [SerializeField] private Vector2 largeShake = new Vector2(0.38f, 0.20f);

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

        public void ShakeSmall() => Shake(smallShake.x, smallShake.y);
        public void ShakeMedium() => Shake(mediumShake.x, mediumShake.y);
        public void ShakeLarge() => Shake(largeShake.x, largeShake.y);

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
