using System;
using System.Collections.Generic;

namespace StarSower.Persistence
{
    // Tiến trình 1 level: có mở khóa chưa + số sao cao nhất từng đạt. Plain data, không logic.
    [Serializable]
    public class LevelSaveData
    {
        public string levelId;
        public bool unlocked;
        public int starsEarned;
    }

    // Số Star Fragment đã thu thập trong phạm vi 1 chapter (S1-012). Tách khỏi tổng toàn game vì
    // tiến trình khôi phục chòm sao tính theo chapter, còn tổng toàn game chỉ là thống kê.
    [Serializable]
    public class ChapterSaveData
    {
        public string chapterId;
        public int fragmentsCollected;
        public bool completed;
    }

    // Chòm sao nào đã được khôi phục (S1-012). Số fragment không lưu ở đây vì mốc là giá trị CỘNG
    // DỒN của cả chapter — đã có trong ChapterSaveData.fragmentsCollected, lưu lại là thừa và dễ lệch.
    [Serializable]
    public class ConstellationSaveData
    {
        public string constellationId;
        public bool restored;
    }

    // Toàn bộ dữ liệu lưu của 1 save slot. Plain data — SaveManager chỉ biết đọc/ghi class này,
    // không biết ý nghĩa từng field; ProgressManager mới là nơi diễn giải.
    [Serializable]
    public class SaveData
    {
        public List<LevelSaveData> levels = new List<LevelSaveData>();
        public int totalStarFragmentsCollected;

        // S1-012 — hành trình khôi phục bầu trời.
        public string currentChapterId;
        public List<ChapterSaveData> chapters = new List<ChapterSaveData>();
        public List<ConstellationSaveData> constellations = new List<ConstellationSaveData>();

        // Level mà "Continue" (từ Main Menu, sau này) nên load vào — cập nhật mỗi khi mở khóa
        // level kế tiếp, luôn trỏ tới level mới nhất người chơi có thể tiếp tục hành trình.
        public string lastPlayedLevelId;

        // Thống kê cộng dồn xuyên suốt hành trình (S1-011) — không phải logic gameplay, chỉ để
        // lưu lại "đã chơi bao lâu" cho sau này (vd: hiện trong màn thống kê/profile người chơi).
        public float totalPlayTimeSeconds;
    }
}
