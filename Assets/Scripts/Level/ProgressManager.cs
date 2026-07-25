using System;
using System.Collections.Generic;
using UnityEngine;
using StarSower.Persistence;

namespace StarSower.Level
{
    // Diễn giải SaveData thành trạng thái tiến trình game hiểu được (unlocked/stars/tổng Star
    // Fragment) + là nơi DUY NHẤT quyết định khi nào lưu. Không biết Goal/UI tồn tại — chỉ expose
    // API đọc + CompleteLevel() để LevelCompleteUI gọi. LevelSelectController chỉ đọc, không ghi.
    public class ProgressManager : MonoBehaviour
    {
        [SerializeField] private LevelDatabase levelDatabase;

        private SaveData saveData;

        public int TotalStarFragmentsCollected => saveData.totalStarFragmentsCollected;
        public string LastPlayedLevelId => saveData.lastPlayedLevelId;
        public string CurrentChapterId => saveData.currentChapterId;

        // (levelId) — bắn mỗi khi tiến trình đổi (unlock, thêm sao...), để LevelSelectController
        // tự làm mới hiển thị mà không cần biết ai vừa gọi CompleteLevel().
        public event Action OnProgressChanged;

        private void Awake()
        {
            saveData = SaveManager.Load() ?? BuildDefaultSave();
            EnsureAllLevelsPresent();
        }

        // Không bắt buộc thu hết sao để hoàn thành — số sao chỉ đổi rating: đủ 100% -> 3 sao,
        // từ 50% -> 2 sao, còn lại -> 1 sao (hoàn thành level luôn được tối thiểu 1 sao). Đặt ở
        // đây (thay vì trong UI) vì đây là RULE tiến trình, không phải chuyện hiển thị — cả
        // LevelFlowManager lẫn LevelCompleteUI (nếu dùng lại sau này) đều gọi chung 1 công thức.
        public static int ComputeStarRating(int collected, int total)
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

        public bool IsUnlocked(string levelId)
        {
            LevelSaveData entry = FindOrNull(levelId);
            return entry != null && entry.unlocked;
        }

        public int GetStars(string levelId)
        {
            LevelSaveData entry = FindOrNull(levelId);
            return entry != null ? entry.starsEarned : 0;
        }

        // Gọi khi Player hoàn thành 1 level (Goal chạm được, không cần đủ sao). Ghi nhận số sao
        // CAO NHẤT từng đạt (không hạ xuống nếu chơi lại tệ hơn), cộng dồn Star Fragment + thời
        // gian chơi vào thống kê toàn game, mở khóa level kế tiếp (đồng thời là level "Continue"
        // sẽ trỏ tới), rồi lưu NGAY LẬP TỨC — không có bước xác nhận nào ở giữa.
        public void CompleteLevel(string levelId, int starRating, int starFragmentsCollectedThisRun, float elapsedTime)
        {
            LevelSaveData entry = FindOrNull(levelId);
            if (entry == null)
                return;

            entry.starsEarned = Mathf.Max(entry.starsEarned, starRating);
            saveData.totalStarFragmentsCollected += starFragmentsCollectedThisRun;
            saveData.totalPlayTimeSeconds += elapsedTime;

            LevelDefinition next = levelDatabase.GetNext(levelId);
            if (next != null)
            {
                LevelSaveData nextEntry = FindOrNull(next.levelId);
                if (nextEntry != null)
                    nextEntry.unlocked = true;
                saveData.lastPlayedLevelId = next.levelId;
            }
            else
            {
                saveData.lastPlayedLevelId = levelId;
            }

            SaveManager.Save(saveData);
            OnProgressChanged?.Invoke();
        }

        // ---------- S1-012: Chapter & Constellation ----------
        // ProgressManager vẫn là NƠI DUY NHẤT ghi save. ConstellationManager giữ LUẬT (rót fragment
        // vào chòm sao nào, khi nào coi là hoàn thành) rồi đưa kết quả xuống đây để lưu — không tự
        // đụng SaveManager. Nhờ vậy chỉ có 1 chỗ quyết định "khi nào ghi đĩa", giống S1-009.

        public int GetChapterFragments(string chapterId)
        {
            if (string.IsNullOrEmpty(chapterId))
                return 0;

            foreach (ChapterSaveData chapter in saveData.chapters)
            {
                if (chapter.chapterId == chapterId)
                    return chapter.fragmentsCollected;
            }
            return 0;
        }

        public bool IsConstellationRestored(string constellationId)
        {
            foreach (ConstellationSaveData entry in saveData.constellations)
            {
                if (entry.constellationId == constellationId)
                    return entry.restored;
            }
            return false;
        }

        public bool IsChapterCompleted(string chapterId)
        {
            foreach (ChapterSaveData chapter in saveData.chapters)
            {
                if (chapter.chapterId == chapterId)
                    return chapter.completed;
            }
            return false;
        }

        // Ghi TOÀN BỘ trạng thái chapter trong 1 lần rồi lưu ĐÚNG MỘT LẦN — ChapterProgressManager
        // giữ luật cộng dồn và mốc, ProgressManager chỉ nhận kết quả cuối rồi ghi đĩa.
        public void WriteChapterProgress(string chapterId, int fragmentsCollected, bool chapterCompleted,
            IEnumerable<string> restoredConstellationIds)
        {
            if (string.IsNullOrEmpty(chapterId))
                return;

            saveData.currentChapterId = chapterId;

            ChapterSaveData chapter = FindOrCreateChapter(chapterId);
            chapter.fragmentsCollected = fragmentsCollected;
            chapter.completed = chapter.completed || chapterCompleted;

            foreach (string constellationId in restoredConstellationIds)
            {
                ConstellationSaveData entry = null;
                foreach (ConstellationSaveData candidate in saveData.constellations)
                {
                    if (candidate.constellationId == constellationId)
                    {
                        entry = candidate;
                        break;
                    }
                }

                if (entry == null)
                {
                    entry = new ConstellationSaveData { constellationId = constellationId };
                    saveData.constellations.Add(entry);
                }
                entry.restored = true;
            }

            SaveManager.Save(saveData);
            OnProgressChanged?.Invoke();
        }

        private ChapterSaveData FindOrCreateChapter(string chapterId)
        {
            foreach (ChapterSaveData chapter in saveData.chapters)
            {
                if (chapter.chapterId == chapterId)
                    return chapter;
            }

            var created = new ChapterSaveData { chapterId = chapterId, fragmentsCollected = 0 };
            saveData.chapters.Add(created);
            return created;
        }

        private LevelSaveData FindOrNull(string levelId)
        {
            foreach (LevelSaveData entry in saveData.levels)
            {
                if (entry.levelId == levelId)
                    return entry;
            }
            return null;
        }

        private SaveData BuildDefaultSave()
        {
            var data = new SaveData();
            bool isFirst = true;
            foreach (LevelDefinition level in levelDatabase.Levels)
            {
                data.levels.Add(new LevelSaveData
                {
                    levelId = level.levelId,
                    unlocked = isFirst,
                    starsEarned = 0,
                });
                isFirst = false;
            }
            return data;
        }

        // Bảo vệ trường hợp Database có thêm level mới sau khi người chơi đã có file save cũ —
        // level mới xuất hiện sẽ ở trạng thái khóa mặc định thay vì bị bỏ sót hoàn toàn.
        private void EnsureAllLevelsPresent()
        {
            foreach (LevelDefinition level in levelDatabase.Levels)
            {
                if (FindOrNull(level.levelId) == null)
                {
                    saveData.levels.Add(new LevelSaveData
                    {
                        levelId = level.levelId,
                        unlocked = false,
                        starsEarned = 0,
                    });
                }
            }
        }
    }
}
