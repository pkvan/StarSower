using System;

namespace StarSower.Core
{
    // Kênh sự kiện toàn cục cho các hệ thống không nên phụ thuộc trực tiếp vào nhau
    // (vd: GameOverManager phát sự kiện, UI/Manager khác sau này lắng nghe).
    public static class GameEvents
    {
        public static event Action OnGameOver;
        public static event Action<float> OnLevelComplete;

        // Sự kiện Goal mới (S1-011): Goal chỉ phát đúng sự kiện này, không mang theo dữ liệu gì —
        // LevelFlowManager tự đọc thời gian/số sao từ LevelTimer/CollectibleManager của chính nó.
        // OnLevelComplete(float) ở trên được giữ lại (không xoá) vì LevelCompleteUI — luồng popup
        // cũ — vẫn còn tồn tại trong code (chỉ đang bị tắt trong scene), không có gì gọi tới nữa.
        public static event Action OnLevelCompleted;

        // S2-008 — Player vua cham Astral Gate. Phat NGAY luc cham, som hon OnLevelCompleted vai
        // giay: quang do la luc cong dang dien canh mo ra, nguoi choi phai het dieu khien duoc
        // nhung chua duoc chuyen man. Gop chung mot su kien thi mat dung mot trong hai.
        public static event Action OnGoalReached;

        public static void RaiseGameOver()
        {
            OnGameOver?.Invoke();
        }

        public static void RaiseLevelComplete(float elapsedTime)
        {
            OnLevelComplete?.Invoke(elapsedTime);
        }

        public static void RaiseGoalReached()
        {
            OnGoalReached?.Invoke();
        }

        public static void RaiseLevelCompleted()
        {
            OnLevelCompleted?.Invoke();
        }
    }
}
