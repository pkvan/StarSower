using System;
using System.Collections.Generic;
using UnityEngine;
using StarSower.Level;
using StarSower.Collectibles;

namespace StarSower.Constellations
{
    // Giữ tiến trình Star Fragment CỘNG DỒN của cả chapter và phát hiện lúc chạm mốc. Fragment KHÔNG
    // reset khi qua level: số đã tích luỹ ở các level trước được cộng với số nhặt được trong level
    // hiện tại, nên thanh tiến trình chạy liên tục suốt hành trình.
    //
    // Không tự vẽ gì, không tự phát hiệu ứng, không tự ghi đĩa — chỉ tính rồi báo ra sự kiện.
    // ConstellationManager nghe để trình diễn, ConstellationUI nghe để hiện số, ProgressManager lo lưu.
    public class ChapterProgressManager : MonoBehaviour
    {
        [SerializeField] private ChapterDatabase chapterDatabase;
        [SerializeField] private ProgressManager progressManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private CollectibleManager collectibleManager;

        [Tooltip("Bắt đầu lại hành trình khi vào level ĐẦU của chapter: fragment về 0 và các chòm sao " +
                 "được khôi phục lại từ đầu. Tắt đi thì tiến trình giữ vĩnh viễn theo save.")]
        [SerializeField] private bool restartChapterOnFirstLevel = true;

        // (fragmentsCollected, totalFragments)
        public event Action<int, int> OnFragmentsChanged;

        // Bắn đúng lúc vượt mốc của 1 chòm sao, theo thứ tự mốc tăng dần.
        public event Action<ConstellationData> OnCheckpointReached;

        public ChapterData Chapter { get; private set; }
        public int FragmentsCollected { get; private set; }
        public int TotalFragments => Chapter != null ? Chapter.TotalFragments : 0;
        public bool IsChapterCompleted { get; private set; }

        // Số fragment đã tích luỹ TRƯỚC level hiện tại — cộng với số nhặt trong level này ra tổng.
        private int fragmentsBeforeThisLevel;
        private readonly HashSet<string> restoredIds = new HashSet<string>();

        private void OnEnable()
        {
            collectibleManager.OnCollectedChanged += HandleCollectedChanged;
        }

        private void OnDisable()
        {
            collectibleManager.OnCollectedChanged -= HandleCollectedChanged;
        }

        // Khởi tạo ở Start(): ProgressManager nạp save trong Awake() của chính nó, mà thứ tự Awake
        // giữa các GameObject là không đảm bảo.
        private void Start()
        {
            LevelDefinition level = levelManager.Database.GetById(levelManager.CurrentLevelId);
            string chapterId = level != null ? level.chapterId : progressManager.CurrentChapterId;
            Chapter = chapterDatabase.GetById(chapterId);

            if (Chapter == null)
            {
                Debug.LogError($"[Constellation] Khong tim thay ChapterData cho chapterId '{chapterId}'. " +
                               "Kiem tra ChapterDatabase va chapterId trong LevelDatabase.", this);
                enabled = false;
                return;
            }

            bool restarting = restartChapterOnFirstLevel && IsFirstLevelOfChapter(level);
            fragmentsBeforeThisLevel = restarting ? 0 : progressManager.GetChapterFragments(Chapter.ChapterId);

            if (!restarting)
            {
                foreach (ConstellationData data in Chapter.Constellations)
                {
                    if (data != null && progressManager.IsConstellationRestored(data.ConstellationId))
                        restoredIds.Add(data.ConstellationId);
                }
                IsChapterCompleted = progressManager.IsChapterCompleted(Chapter.ChapterId);
            }

            FragmentsCollected = fragmentsBeforeThisLevel;

            // Bắt đầu lại chapter phải xoá sạch CẢ HAI THỨ trong save: fragment VÀ cờ chòm sao đã
            // khôi phục. Trước đây chỉ Persist() nên fragment về 0 còn cờ chòm sao kẹt lại true —
            // sang level sau, restoredIds nạp lại đủ 3 chòm sao cũ và không mốc nào bắn được nữa.
            if (restarting)
                progressManager.ResetChapterProgress(Chapter.ChapterId, CollectConstellationIds());
            else
                Persist();

            OnFragmentsChanged?.Invoke(FragmentsCollected, TotalFragments);
        }

        private List<string> CollectConstellationIds()
        {
            var ids = new List<string>();
            foreach (ConstellationData data in Chapter.Constellations)
            {
                if (data != null)
                    ids.Add(data.ConstellationId);
            }
            return ids;
        }

        // Level đầu của chapter = level đầu tiên trong LevelDatabase mang chapterId này. Suy ra từ
        // dữ liệu chứ không hardcode "level_01".
        private bool IsFirstLevelOfChapter(LevelDefinition level)
        {
            if (level == null)
                return false;

            foreach (LevelDefinition candidate in levelManager.Database.Levels)
            {
                if (candidate.chapterId == level.chapterId)
                    return candidate.levelId == level.levelId;
            }
            return false;
        }

        private void HandleCollectedChanged(int collectedThisLevel, int totalThisLevel)
        {
            int updated = fragmentsBeforeThisLevel + collectedThisLevel;
            if (updated == FragmentsCollected)
                return;

            int previous = FragmentsCollected;
            FragmentsCollected = updated;
            OnFragmentsChanged?.Invoke(FragmentsCollected, TotalFragments);

            RaiseCrossedCheckpoints(previous, FragmentsCollected);
            Persist();
        }

        // Duyệt theo thứ tự mốc tăng dần và chỉ bắn cho những mốc VỪA vượt qua trong lần nhặt này —
        // nhặt 1 lần mà nhảy qua 2 mốc thì cả 2 đều được báo, đúng thứ tự.
        private void RaiseCrossedCheckpoints(int previous, int current)
        {
            var ordered = new List<ConstellationData>(Chapter.Constellations);
            ordered.Sort((a, b) => a.RequiredFragments.CompareTo(b.RequiredFragments));

            foreach (ConstellationData data in ordered)
            {
                if (data == null || restoredIds.Contains(data.ConstellationId))
                    continue;
                if (current < data.RequiredFragments || previous >= data.RequiredFragments)
                    continue;

                restoredIds.Add(data.ConstellationId);

                ConstellationData final = Chapter.FinalConstellation;
                if (final != null && data.ConstellationId == final.ConstellationId)
                    IsChapterCompleted = true;

                OnCheckpointReached?.Invoke(data);
            }
        }

        public bool IsRestored(ConstellationData constellation)
        {
            return constellation != null && restoredIds.Contains(constellation.ConstellationId);
        }

        public int RestoredCount => restoredIds.Count;

        private void Persist()
        {
            progressManager.WriteChapterProgress(Chapter.ChapterId, FragmentsCollected, IsChapterCompleted, restoredIds);
        }
    }
}
