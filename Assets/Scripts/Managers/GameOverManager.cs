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
        [SerializeField] private CameraFollowY cameraFollow;
        [SerializeField] private float maxFallDistance = 6f;

        [Tooltip("Thời gian chờ trước khi reload lại level, để người chơi kịp nhận ra vừa rơi chết.")]
        [SerializeField] private float reloadDelay = 1.5f;

        [Tooltip("Bật để hiện IsGameOver/CurrentFallDistance trên màn hình — dùng tạm lúc debug.")]
        [SerializeField] private bool debugLogging = false;

        private bool isGameOver;

        // Chỉ đọc — dùng cho debug HUD.
        public bool IsGameOver => isGameOver;
        public float CurrentFallDistance { get; private set; }

        private void Update()
        {
            CurrentFallDistance = cameraFollow.HighestY - playerTransform.position.y;

            if (isGameOver)
                return;

            if (CurrentFallDistance >= maxFallDistance)
                TriggerGameOver();
        }

        private void TriggerGameOver()
        {
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
