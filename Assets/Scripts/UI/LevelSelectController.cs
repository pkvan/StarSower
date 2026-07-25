using UnityEngine;
using UnityEngine.UI;
using StarSower.Level;

namespace StarSower.UI
{
    // Danh sách level: đọc LevelDatabase + ProgressManager (chỉ đọc, không ghi), dựng 1
    // LevelSelectEntryView cho mỗi level trong Database — không hardcode số lượng level ở đâu cả.
    // Chọn 1 level -> báo LevelManager xử lý (load scene hoặc đóng panel nếu là level hiện tại).
    public class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private ProgressManager progressManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private LevelSelectEntryView entryPrefab;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            Populate();
            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            panelRoot.SetActive(false);
        }

        private void Populate()
        {
            for (int i = entryContainer.childCount - 1; i >= 0; i--)
                Destroy(entryContainer.GetChild(i).gameObject);

            foreach (LevelDefinition level in levelDatabase.Levels)
            {
                LevelSelectEntryView entry = Instantiate(entryPrefab, entryContainer);
                bool unlocked = progressManager.IsUnlocked(level.levelId);
                int stars = progressManager.GetStars(level.levelId);
                entry.Setup(level, unlocked, stars, OnLevelSelected);
            }
        }

        private void OnLevelSelected(LevelDefinition level)
        {
            levelManager.LoadLevel(level);
        }
    }
}
