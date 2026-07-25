using System;
using UnityEngine;

namespace StarSower.Collectibles
{
    // Quản lý tổng số Star Fragment trong level + số đã thu thập. Tổng số được ĐẾM TỰ ĐỘNG từ
    // các StarFragment có trong scene lúc Start (không hardcode số lượng ở đâu cả). Đây là nguồn
    // dữ liệu duy nhất cho UI (HUD, LevelCompleteUI) — không component nào khác được tự đếm lại.
    public class CollectibleManager : MonoBehaviour
    {
        public int TotalStars { get; private set; }
        public int CollectedStars { get; private set; }

        // (collected, total) — bắn cả lúc khởi tạo (collected=0) lẫn mỗi lần thu thập, để bên nghe
        // không cần biết thứ tự Start() giữa các script.
        public event Action<int, int> OnCollectedChanged;

        private void Start()
        {
            TotalStars = FindObjectsByType<StarFragment>(FindObjectsSortMode.None).Length;
            OnCollectedChanged?.Invoke(CollectedStars, TotalStars);
        }

        // Gọi bởi StarFragment khi Player thu thập. StarFragment tự đảm bảo chỉ gọi đúng 1 lần.
        public void RegisterCollected()
        {
            CollectedStars++;
            OnCollectedChanged?.Invoke(CollectedStars, TotalStars);
        }
    }
}
