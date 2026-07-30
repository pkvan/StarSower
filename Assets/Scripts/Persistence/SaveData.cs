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

        // S1-020B — đã CHIẾU hoạt ảnh mở khoá cho chòm này chưa. Tách khỏi `restored` vì hai câu
        // hỏi khác nhau: "đã mở chưa" quyết định VẼ, "đã diễn chưa" quyết định CÓ DIỄN LẠI KHÔNG.
        // Gộp làm một thì mở lại màn hình sẽ diễn lại từ đầu mỗi lần.
        public bool animationPlayed;

        // S2-006 — SỐ ngôi sao trong chòm đã khôi phục. Bầu trời lành DẦN qua nhiều lượt chơi:
        // 1 sao mở ~1/3 chòm, 2 sao ~2/3, 3 sao trọn chòm. Chỉ tăng, không bao giờ giảm.
        //
        // Trường CỘNG THÊM, tương thích ngược: save cũ không có khoá này thì JsonUtility để
        // nguyên giá trị mặc định 0 — không làm hỏng file, không phải chuyển đổi gì.
        // `restored` và `animationPlayed` giữ nguyên ý nghĩa cũ.
        public int nodesRestored;
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

        // S1-020A — chòm sao mở dần theo SỐ KHU VỰC đã hoàn thành, không còn theo mốc Star Fragment.
        //
        // Hai con số tách riêng có chủ ý:
        //   starsUnlocked  = đã mở tới ngôi sao thứ mấy (dùng để VẼ).
        //   starsAnimated  = đã CHIẾU hoạt ảnh mở khoá tới ngôi thứ mấy.
        // Nhờ vậy mở lại màn hình lần sau sẽ hiện chòm sao hoàn chỉnh ngay lập tức thay vì diễn
        // lại từ đầu — yêu cầu "unlock animation only plays the first time".
        //
        // Các field chòm sao CŨ (constellations[]) được giữ nguyên, không xoá: save đang có của
        // người chơi vẫn đọc được, và xoá field khỏi class Serializable sẽ làm JsonUtility bỏ qua
        // dữ liệu đó vĩnh viễn.
        public int constellationStarsUnlocked;
        public int constellationStarsAnimated;

        // Level mà "Continue" (từ Main Menu, sau này) nên load vào — cập nhật mỗi khi mở khóa
        // level kế tiếp, luôn trỏ tới level mới nhất người chơi có thể tiếp tục hành trình.
        public string lastPlayedLevelId;

        // Thống kê cộng dồn xuyên suốt hành trình (S1-011) — không phải logic gameplay, chỉ để
        // lưu lại "đã chơi bao lâu" cho sau này (vd: hiện trong màn thống kê/profile người chơi).
        public float totalPlayTimeSeconds;
    }
}
