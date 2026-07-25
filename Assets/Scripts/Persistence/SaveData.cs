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

    // Toàn bộ dữ liệu lưu của 1 save slot. Plain data — SaveManager chỉ biết đọc/ghi class này,
    // không biết ý nghĩa từng field; ProgressManager mới là nơi diễn giải.
    [Serializable]
    public class SaveData
    {
        public List<LevelSaveData> levels = new List<LevelSaveData>();
        public int totalStarFragmentsCollected;

        // Level mà "Continue" (từ Main Menu, sau này) nên load vào — cập nhật mỗi khi mở khóa
        // level kế tiếp, luôn trỏ tới level mới nhất người chơi có thể tiếp tục hành trình.
        public string lastPlayedLevelId;

        // Thống kê cộng dồn xuyên suốt hành trình (S1-011) — không phải logic gameplay, chỉ để
        // lưu lại "đã chơi bao lâu" cho sau này (vd: hiện trong màn thống kê/profile người chơi).
        public float totalPlayTimeSeconds;
    }
}
