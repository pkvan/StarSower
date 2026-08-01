using System.Collections.Generic;
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

        [Tooltip("S2-014 — de tra ra chom sao cua tung level ma hien so ngoi da khoi phuc. " +
                 "De trong thi the level van chay, chi khong hien phan chom sao.")]
        [SerializeField] private StarSower.Constellations.ChapterData chapter;

        [Tooltip("Tieu de man hinh. De trong thi bo qua.")]
        [SerializeField] private Text chapterTitle;

        [Tooltip("S2-014 — anh chu dao cua tung level, xep CUNG THU TU voi LevelDatabase. Lay lop " +
                 "nen dau tien cua moi RegionData. De trong thi the khong co anh nen.")]
        [SerializeField] private StarSower.Biome.RegionData[] regions = new StarSower.Biome.RegionData[0];

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

        // Man menu bat/tat panel qua MenuRouter chu khong goi Show(), nen phai dung lai danh sach
        // khi panel duoc bat len. Chan bang so khung hinh: Show() cung goi Populate(), hai loi goi
        // trong CUNG mot khung se tao ra hai bo the (Destroy bi hoan toi cuoi khung, con Instantiate
        // thi khong).
        private int populatedFrame = -1;

        private void OnEnable()
        {
            Populate();
        }

        private void Populate()
        {
            if (populatedFrame == Time.frameCount)
                return;
            populatedFrame = Time.frameCount;

            if (levelDatabase == null || progressManager == null || entryContainer == null || entryPrefab == null)
                return;

            for (int i = entryContainer.childCount - 1; i >= 0; i--)
                Destroy(entryContainer.GetChild(i).gameObject);

            if (chapterTitle != null && chapter != null)
                chapterTitle.text = chapter.ChapterName;

            IReadOnlyList<LevelDefinition> levels = levelDatabase.Levels;
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                LevelSelectEntryView entry = Instantiate(entryPrefab, entryContainer);
                bool unlocked = progressManager.IsUnlocked(level.levelId);
                int collectedStars = progressManager.GetCollectedStars(level.levelId);

                // Anh xa theo CHI SO, giong ConstellationLookup: level thu i ung voi chom sao thu i.
                // Khong dung ConstellationLookup o day duoc vi no tra theo level DANG choi, con man
                // nay phai tra cho tung level trong danh sach.
                //
                // Chi can SO NODE — do la so manh sao cua man, tuc so o sao phai ve tren the.
                int total = 0;
                if (chapter != null && i < chapter.Constellations.Count)
                {
                    StarSower.Constellations.ConstellationData data = chapter.Constellations[i];
                    if (data != null)
                        total = data.NodeCount;
                }

                // So o sao lay tu SO NODE cua chom sao chu khong tu save: man chua choi lan nao
                // thi save chua co gi, nhung the van phai hien du 5 o rong cho biet man do co 5 sao.
                entry.Setup(level, unlocked, collectedStars, total,
                            ResolveBackground(i), OnLevelSelected);
            }
        }

        // Lop nen dau tien (background_far) la anh nhan dien ro nhat cua khu vuc. Lop thu hai la
        // lop gan, thuong chi la vai cum may/cay o tien canh, nhin rieng ra khong doc duoc la dau.
        private Sprite ResolveBackground(int index)
        {
            if (regions == null || index < 0 || index >= regions.Length || regions[index] == null)
                return null;

            IReadOnlyList<StarSower.Biome.BackgroundLayerData> layers = regions[index].BackgroundLayers;
            return layers.Count > 0 ? layers[0].sprite : null;
        }

        private void OnLevelSelected(LevelDefinition level)
        {
            levelManager.LoadLevel(level);
        }
    }
}
