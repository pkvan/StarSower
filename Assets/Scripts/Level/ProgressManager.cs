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

        // S2-014 — so manh sao DA NHAT duoc o level nay (khong phai hang sao 0..3). Dung cho the
        // level: hien dung so ngoi sao ma man do co, ngoi da nhat thi co mau.
        public int GetCollectedStars(string levelId)
        {
            LevelSaveData entry = FindOrNull(levelId);
            return entry != null ? entry.collectedStars : 0;
        }

        // ---- Chòm sao theo khu vực (S1-020A) ----

        // Số khu vực ĐÃ HOÀN THÀNH. Suy ra từ starsEarned > 0 chứ không thêm field "completed" mới:
        // ComputeStarRating() luôn trả về tối thiểu 1 sao khi hoàn thành, còn mặc định là 0 — nên
        // starsEarned > 0 chính là "đã qua màn này". Cách này đọc đúng cả với save đã tồn tại từ
        // trước, thứ mà một field mới sẽ luôn thấy là false.
        public int CompletedRegionCount
        {
            get
            {
                int count = 0;
                foreach (LevelSaveData entry in saveData.levels)
                {
                    if (entry.starsEarned > 0)
                        count++;
                }
                return count;
            }
        }

        public int TotalRegionCount => saveData.levels.Count;
        public int ConstellationStarsUnlocked => saveData.constellationStarsUnlocked;
        public int ConstellationStarsAnimated => saveData.constellationStarsAnimated;

        // Quy đổi tiến trình khu vực thành số sao. KHÔNG hardcode 5: chòm sao có bao nhiêu ngôi
        // cũng chia đúng tỉ lệ, và luôn mở trọn vẹn khi hoàn thành khu vực cuối.
        public int ComputeUnlockedStars(int totalStars)
        {
            if (totalStars <= 0 || TotalRegionCount <= 0)
                return 0;

            int completed = CompletedRegionCount;
            if (completed >= TotalRegionCount)
                return totalStars;

            return Mathf.Clamp(Mathf.RoundToInt(totalStars * (float)completed / TotalRegionCount), 0, totalStars);
        }

        // ---- Trạng thái RIÊNG của từng chòm sao (S1-020B) ----

        public bool IsConstellationAnimated(string constellationId)
        {
            foreach (ConstellationSaveData entry in saveData.constellations)
            {
                if (entry.constellationId == constellationId)
                    return entry.animationPlayed;
            }
            return false;
        }

        // ---- S2-006: bầu trời lành DẦN ----

        // Số ngôi sao của chòm đã khôi phục. Kẹp về [0, totalNodes] ngay tại đây: save cũ hoặc
        // save hỏng có thể mang số lớn hơn số node hiện tại (designer bớt node đi chẳng hạn),
        // để nguyên sẽ làm màn trình diễn dựng thiếu/thừa node.
        public int GetConstellationNodes(string constellationId, int totalNodes)
        {
            if (string.IsNullOrEmpty(constellationId) || totalNodes <= 0)
                return 0;

            foreach (ConstellationSaveData entry in saveData.constellations)
            {
                if (entry.constellationId == constellationId)
                    return Mathf.Clamp(entry.nodesRestored, 0, totalNodes);
            }
            return 0;
        }

        // Ghi số node đã khôi phục. ĐƠN ĐIỆU: chỉ nhận giá trị lớn hơn, nên chơi lại một màn với
        // hạng sao thấp hơn không bao giờ làm bầu trời tối lại.
        public void SetConstellationNodes(string constellationId, int nodes, int totalNodes)
        {
            if (string.IsNullOrEmpty(constellationId) || totalNodes <= 0)
                return;

            int clamped = Mathf.Clamp(nodes, 0, totalNodes);

            foreach (ConstellationSaveData entry in saveData.constellations)
            {
                if (entry.constellationId != constellationId)
                    continue;

                if (clamped > entry.nodesRestored)
                {
                    entry.nodesRestored = clamped;
                    SaveManager.Save(saveData);
                }
                return;
            }

            saveData.constellations.Add(new ConstellationSaveData
            {
                constellationId = constellationId,
                nodesRestored = clamped,
            });
            SaveManager.Save(saveData);
        }

        // Hạng sao (0..3) đã đạt của một level. Dùng để quy ra số node được khôi phục.
        public int GetLevelStars(string levelId)
        {
            foreach (LevelSaveData entry in saveData.levels)
            {
                if (entry.levelId == levelId)
                    return entry.starsEarned;
            }
            return 0;
        }

        // Ghi CỘNG DỒN, không bao giờ ghi đè: mỗi chòm có bản ghi riêng nên mở chòm 3 không đụng
        // gì tới chòm 1 và 2 — đúng yêu cầu "never overwrite previous progress".
        public void MarkConstellationUnlocked(string constellationId, bool animationPlayed)
        {
            if (string.IsNullOrEmpty(constellationId))
                return;

            foreach (ConstellationSaveData entry in saveData.constellations)
            {
                if (entry.constellationId != constellationId)
                    continue;

                entry.restored = true;
                entry.animationPlayed |= animationPlayed;
                SaveManager.Save(saveData);
                OnProgressChanged?.Invoke();
                return;
            }

            saveData.constellations.Add(new ConstellationSaveData
            {
                constellationId = constellationId,
                restored = true,
                animationPlayed = animationPlayed,
            });
            SaveManager.Save(saveData);
            OnProgressChanged?.Invoke();
        }

        public void WriteConstellationStars(int unlocked, int animated)
        {
            saveData.constellationStarsUnlocked = Mathf.Max(saveData.constellationStarsUnlocked, unlocked);
            saveData.constellationStarsAnimated = Mathf.Max(saveData.constellationStarsAnimated, animated);
            SaveManager.Save(saveData);
            OnProgressChanged?.Invoke();
        }

        // Gọi khi Player hoàn thành 1 level (Goal chạm được, không cần đủ sao). Ghi nhận số sao
        // CAO NHẤT từng đạt (không hạ xuống nếu chơi lại tệ hơn), cộng dồn Star Fragment + thời
        // gian chơi vào thống kê toàn game, mở khóa level kế tiếp (đồng thời là level "Continue"
        // sẽ trỏ tới), rồi lưu NGAY LẬP TỨC — không có bước xác nhận nào ở giữa.
        public void CompleteLevel(string levelId, int starRating, int starFragmentsCollectedThisRun, float elapsedTime)
        {
            CompleteLevel(levelId, starRating, starFragmentsCollectedThisRun, elapsedTime, 0, 0);
        }

        // S2-009 — nap chong co them so sao yeu cau / da nhat cua lan choi nay. Giu nap chong cu de
        // moi cho dang goi khong phai sua theo.
        public void CompleteLevel(string levelId, int starRating, int starFragmentsCollectedThisRun,
                                  float elapsedTime, int requiredStars, int collectedStars)
        {
            LevelSaveData entry = FindOrNull(levelId);
            if (entry == null)
                return;

            entry.starsEarned = Mathf.Max(entry.starsEarned, starRating);

            if (requiredStars > 0)
            {
                entry.requiredStars = requiredStars;
                // Chi ghi len khi lan nay nhat duoc NHIEU hon — choi lai te hon khong lam mat
                // thanh tich cu, dung luat da dung cho starsEarned va nodesRestored.
                entry.collectedStars = Mathf.Max(entry.collectedStars, collectedStars);
                entry.gateUnlocked = entry.gateUnlocked || collectedStars >= requiredStars;
            }
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

        // Xoá sạch tiến trình của MỘT chapter để chơi lại từ đầu: fragment về 0, cờ hoàn thành về
        // false, và các chòm sao của chapter đó về chưa-khôi-phục.
        //
        // Vì sao phải có hàm riêng thay vì dùng WriteChapterProgress: hàm kia CHỈ BIẾT ĐI LÊN
        // (completed dùng ||, constellation chỉ gán restored = true). Đó là chủ ý — tiến trình
        // không được tự tụt trong lúc chơi bình thường. Nhưng vì thế nó không thể diễn tả được
        // hành động "bắt đầu lại", và đó chính là chỗ đã sinh ra bug: fragment bị đưa về 0 còn cờ
        // chòm sao thì kẹt lại true vĩnh viễn.
        //
        // Chỉ nhận đúng danh sách constellationId của chapter này, không quét sạch cả file — chapter
        // khác phải không hề hấn gì.
        public void ResetChapterProgress(string chapterId, IEnumerable<string> constellationIds)
        {
            if (string.IsNullOrEmpty(chapterId))
                return;

            saveData.currentChapterId = chapterId;

            ChapterSaveData chapter = FindOrCreateChapter(chapterId);
            chapter.fragmentsCollected = 0;
            chapter.completed = false;

            foreach (string constellationId in constellationIds)
            {
                foreach (ConstellationSaveData candidate in saveData.constellations)
                {
                    if (candidate.constellationId == constellationId)
                    {
                        candidate.restored = false;
                        break;
                    }
                }
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
