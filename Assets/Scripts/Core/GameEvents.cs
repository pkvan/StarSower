using System;

namespace StarSower.Core
{
    // Kênh sự kiện toàn cục cho các hệ thống không nên phụ thuộc trực tiếp vào nhau
    // (vd: GameOverManager phát sự kiện, UI/Manager khác sau này lắng nghe).
    public static class GameEvents
    {
        public static event Action OnGameOver;

        public static void RaiseGameOver()
        {
            OnGameOver?.Invoke();
        }
    }
}
