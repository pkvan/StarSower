using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarSower.Core;
using StarSower.CameraSystem;

namespace StarSower.Managers
{
    // Theo dõi khoảng cách rơi của Player so với điểm cao nhất từng đạt (CameraFollowY.HighestY —
    // KHÔNG dùng vị trí camera thời gian thực, vì camera giờ được phép lia xuống theo Player lúc
    // rơi). Khi vượt ngưỡng: phát GameEvents.OnGameOver (PlayerController tự lo việc khoá input
    // của chính nó) rồi reload lại scene sau 1 khoảng delay ngắn.
    public class GameOverManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;

        [Tooltip("CŨ (S1-00x). Để trống là đúng — CameraFollowY đã bị CameraFollow2D thay thế và " +
                 "tắt đi từ lâu, kéo theo cả hệ thống chết-do-rơi ngừng hoạt động. Giữ field để " +
                 "scene cũ không hỏng, nhưng luật chết giờ dùng Kill Floor ở dưới.")]
        [SerializeField] private CameraFollowY cameraFollow;

        [Tooltip("CHỈ dùng khi Camera Follow (cũ) còn được gán. Cảnh báo: Player nhảy cao tới 7.34 " +
                 "unit nên mọi giá trị dưới ~8 sẽ giết oan người chơi ngay giữa một cú nhảy bình thường.")]
        [SerializeField] private float maxFallDistance = 12f;

        [Tooltip("Ngưỡng Y mà dưới đó coi như đã rơi ra khỏi màn. Đặt THẤP HƠN HẲN platform thấp " +
                 "nhất (platform đầu ở y = -1.5) nên không thể báo nhầm: chỉ khi rơi qua hết mọi " +
                 "chỗ có thể đáp mới xuống tới đây.\n\n" +
                 "Cách này thay cho 'rơi quá N unit so với đỉnh cao nhất' — luật cũ báo nhầm liên " +
                 "tục ở các màn mới, nơi hụt một cú nhảy là tụt cả chục unit xuống platform dưới " +
                 "mà vẫn còn sống.")]
        [SerializeField] private float killFloorY = -12f;

        [Tooltip("Thời gian chờ trước khi reload lại level, để người chơi kịp nhận ra vừa rơi chết.")]
        [SerializeField] private float reloadDelay = 1.5f;

        [Tooltip("S3-R3. Có gán thì chạm Kill Floor sẽ HỒI SINH tại mốc gần nhất thay vì nạp lại " +
                 "cả màn — bắt buộc với màn S3 cao ~80 unit, nơi nạp lại đồng nghĩa mất hết tiến " +
                 "trình vì hụt đúng một cú nhảy. Để trống thì luật cũ (nạp lại scene) giữ nguyên, " +
                 "nên mọi scene chưa gán vẫn chạy y như trước.")]
        [SerializeField] private Level.RespawnManager respawnManager;

        [Tooltip("Bật để hiện IsGameOver/CurrentFallDistance trên màn hình — dùng tạm lúc debug.")]
        [SerializeField] private bool debugLogging = false;

        private bool isGameOver;

        // Chỉ đọc — dùng cho debug HUD.
        public bool IsGameOver => isGameOver;
        public float CurrentFallDistance { get; private set; }

        private void Update()
        {
            if (isGameOver)
                return;

            float playerY = playerTransform.position.y;

            // Kill Floor là luật chính: không thể báo nhầm vì mọi platform đều nằm trên nó.
            if (playerY <= killFloorY)
            {
                TriggerGameOver();
                return;
            }

            // Luật cũ chỉ còn chạy khi scene vẫn gán CameraFollowY. Không gán thì bỏ qua hoàn toàn
            // thay vì ném NullReference — đó chính là thứ khiến component này bị tắt trước đây.
            if (cameraFollow == null)
                return;

            CurrentFallDistance = cameraFollow.HighestY - playerY;
            if (CurrentFallDistance >= maxFallDistance)
                TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            // Có mốc hồi sinh thì rơi KHÔNG còn là kết thúc màn: kéo Player về mốc rồi chơi tiếp,
            // không phát OnGameOver (phát sẽ khiến PlayerController tự khoá input vĩnh viễn) và
            // không đặt isGameOver (đặt sẽ khoá luôn Update, mốc sau đó hết tác dụng).
            if (respawnManager != null)
            {
                respawnManager.Respawn();
                return;
            }

            isGameOver = true;
            GameEvents.RaiseGameOver();
            StartCoroutine(ReloadLevelAfterDelay());
        }

        private IEnumerator ReloadLevelAfterDelay()
        {
            yield return new WaitForSeconds(reloadDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // HUD debug tạm thời — đặt ở góc khác với PlayerController để không đè lên nhau.
        // GameOverManager (tầng Managers) được phép biết cả Player lẫn Camera nên không vi phạm
        // Dependency Direction — ngược lại PlayerController không được phép biết GameOverManager.
        private void OnGUI()
        {
            if (!debugLogging)
                return;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,
                normal = { textColor = Color.cyan }
            };

            string text =
                $"IsGameOver: {isGameOver}\n" +
                $"CurrentFallDistance: {CurrentFallDistance:F2}\n" +
                $"MaxFallDistance: {maxFallDistance:F2}\n" +
                $"HighestY: {cameraFollow.HighestY:F2}";

            GUI.Label(new Rect(20, 620, 900, 300), text, style);
        }
    }
}
