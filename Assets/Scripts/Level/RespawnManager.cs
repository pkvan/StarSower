using UnityEngine;
using StarSower.Core;
using StarSower.Player;

namespace StarSower.Level
{
    // S3-R3 — hồi sinh tại mốc gần nhất thay vì nạp lại cả màn.
    //
    // Vì sao cần: từ S3 màn chơi cao ~80 unit (8 màn hình). Hụt một cú nhảy ở tầng trên có thể rơi
    // 40 unit — mất 5 phút leo — mà chẳng chết, nên luật "chạm Kill Floor thì nạp lại scene" không
    // hề chạm tới. Đó đúng là kiểu phạt mà thiết kế S3 cấm: "failure should always feel fair".
    //
    // Luật ở đây: rơi quá maxFallBelowCheckpoint so với MỐC đang giữ thì hồi sinh ngay tại mốc đó.
    // Hụt một cú nhảy vì thế luôn tốn đúng một tầng, không bao giờ hơn — bất kể màn cao bao nhiêu.
    //
    // KHÔNG tự ghi Rigidbody2D: gọi PlayerMotor.Teleport(). PlayerMotor vẫn là nơi ghi duy nhất,
    // đúng quy tắc Single-Writer đi suốt dự án.
    //
    // Để trống trong scene cũ là an toàn tuyệt đối: GameOverManager chỉ đổi hành vi khi field
    // respawnManager được gán, không gán thì luật Kill Floor + nạp lại scene chạy y như trước.
    public class RespawnManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerMotor playerMotor;

        [Tooltip("Mốc khởi đầu — thường là chỗ Player đứng lúc vào màn. Để trống thì lấy luôn vị " +
                 "trí ban đầu của Player.")]
        [SerializeField] private Transform initialCheckpoint;

        [Tooltip("Rơi quá bao nhiêu unit so với mốc đang giữ thì hồi sinh. Để LỚN HƠN chiều cao " +
                 "một tầng (S3 dùng ~13 unit/tầng) để rơi xuống thềm ngay dưới vẫn được chơi tiếp " +
                 "— chỉ khi rơi hụt qua cả thềm đó mới kéo về.")]
        [Min(1f)]
        [SerializeField] private float maxFallBelowCheckpoint = 16f;

        [Tooltip("Đặt Player cao hơn mốc bao nhiêu lúc hồi sinh, để không sinh ra kẹt trong sàn.")]
        [SerializeField] private float respawnHeightOffset = 1f;

        private Vector2 checkpoint;
        private bool hasCheckpoint;

        // Chỉ đọc — Checkpoint gọi vào để biết mình có phải mốc mới không.
        public Vector2 CurrentCheckpoint => checkpoint;

        private void Awake()
        {
            checkpoint = initialCheckpoint != null
                ? (Vector2)initialCheckpoint.position
                : (Vector2)playerTransform.position;
            hasCheckpoint = true;
        }

        // Chỉ nhận mốc CAO HƠN mốc đang giữ. Đi ngược xuống một thềm cũ không được phép kéo mốc
        // tụt lại — nếu không, rơi xuống rồi chạm lại thềm dưới sẽ khoá luôn tiến trình đã leo.
        public void SetCheckpoint(Vector2 position)
        {
            if (hasCheckpoint && position.y <= checkpoint.y)
                return;

            checkpoint = position;
            hasCheckpoint = true;
        }

        private void Update()
        {
            if (!hasCheckpoint)
                return;

            if (playerTransform.position.y < checkpoint.y - maxFallBelowCheckpoint)
                Respawn();
        }

        public void Respawn()
        {
            playerMotor.Teleport(checkpoint + new Vector2(0f, respawnHeightOffset));
            GameEvents.RaisePlayerRespawned();
        }
    }
}
