using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StarSower.Core;
using StarSower.Collectibles;

namespace StarSower.Level
{
    // Hiển thị màn hình Level Complete khi GameEvents.OnLevelComplete được phát. Không biết Goal
    // tồn tại (chỉ nghe GameEvents) — nhưng được phép biết CollectibleManager/ProgressManager/
    // LevelManager để hiện số Star + rating + điều hướng, cùng tầng UI/Managers như GameOverManager
    // đã biết Player/Camera. Số sao KHÔNG bắt buộc để hoàn thành — chỉ ảnh hưởng rating hiển thị ở
    // đây và số sao lưu lại, không ảnh hưởng gì tới việc Goal có cho qua màn hay không.
    public class LevelCompleteUI : MonoBehaviour
    {
        [SerializeField] private string levelName = "Star Valley";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text starsText;
        [SerializeField] private Text starRatingText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private ProgressManager progressManager;
        [SerializeField] private LevelManager levelManager;

        [Tooltip("Thời gian fade in panel khi level hoàn thành.")]
        [SerializeField] private float fadeDuration = 0.4f;

        private void Awake()
        {
            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();

            panelRoot.SetActive(false);
            panelCanvasGroup.alpha = 0f;
            retryButton.onClick.AddListener(HandleRetry);
            nextLevelButton.onClick.AddListener(HandleNextLevel);
            levelSelectButton.onClick.AddListener(HandleLevelSelect);
        }

        private void OnEnable()
        {
            GameEvents.OnLevelComplete += HandleLevelComplete;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelComplete -= HandleLevelComplete;
        }

        private void HandleLevelComplete(float elapsedTime)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            int collected = collectibleManager.CollectedStars;
            int total = collectibleManager.TotalStars;
            int rating = ComputeStarRating(collected, total);

            levelNameText.text = levelName;
            timeText.text = $"Time: {minutes:00}:{seconds:00}";
            starsText.text = $"⭐ {collected} / {total}";
            starRatingText.text = FormatStarRating(rating);

            progressManager.CompleteLevel(levelManager.CurrentLevelId, rating, collected, elapsedTime);
            nextLevelButton.interactable = levelManager.HasNextLevel;

            panelRoot.SetActive(true);
            StartCoroutine(FadeInRoutine());
        }

        // Không bắt buộc thu hết sao để hoàn thành (Goal luôn cho qua) — số sao chỉ đổi rating
        // hiển thị/lưu lại ở đây: đủ 100% -> 3 sao, từ 50% -> 2 sao, còn lại -> 1 sao (vẫn hoàn
        // thành level thì luôn được tối thiểu 1 sao). Không có Star nào trong level (total=0) coi
        // như trọn vẹn.
        private int ComputeStarRating(int collected, int total)
        {
            if (total <= 0)
                return 3;

            float ratio = (float)collected / total;
            if (ratio >= 1f)
                return 3;
            if (ratio >= 0.5f)
                return 2;
            return 1;
        }

        private string FormatStarRating(int rating)
        {
            return new string('★', rating) + new string('☆', 3 - rating);
        }

        private IEnumerator FadeInRoutine()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
        }

        private void HandleRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleNextLevel()
        {
            levelManager.LoadNextLevel();
        }

        private void HandleLevelSelect()
        {
            panelRoot.SetActive(false);
            levelManager.LoadLevelSelect();
        }
    }
}
