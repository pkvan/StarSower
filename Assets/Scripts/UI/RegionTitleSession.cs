using System.Collections.Generic;
using UnityEngine;

namespace StarSower.UI
{
    // Ghi nhớ những Region đã hiện title chòm sao trong PHIÊN CHƠI hiện tại, để mỗi khu vực chỉ
    // giới thiệu đúng một lần.
    //
    // Cố tình KHÔNG ghi vào SaveData — giống hệt lý do của BiomeSession (S1-013):
    //   1. Sprint này là "presentation only", không được đụng Save System.
    //   2. Đây là trạng thái phiên chơi, không phải tiến trình người chơi. Ghi xuống đĩa nghĩa là
    //      người chơi VĨNH VIỄN không bao giờ được xem lại khoảnh khắc đó nữa — mâu thuẫn với
    //      quyết định thiết kế #26 (chơi lại từ đầu chapter thì được xem lại các khoảnh khắc).
    //
    // Hệ quả cần biết: mở lại game là title hiện lại. Muốn nhớ vĩnh viễn thì phải thêm field vào
    // SaveData + ProgressManager — một thay đổi thuộc tầng Save, không thuộc sprint trình bày.
    public static class RegionTitleSession
    {
        private static readonly HashSet<string> shownRegionIds = new HashSet<string>();

        public static bool HasShown(string regionId)
        {
            return !string.IsNullOrEmpty(regionId) && shownRegionIds.Contains(regionId);
        }

        public static void MarkShown(string regionId)
        {
            if (!string.IsNullOrEmpty(regionId))
                shownRegionIds.Add(regionId);
        }

        // Cho phép chủ động reset (vd nút "chơi lại từ đầu" sau này). Đúng ý "unless intentionally
        // reset" trong yêu cầu.
        public static void ResetAll()
        {
            shownRegionIds.Clear();
        }

        // Dọn khi vào Play Mode. Có [RuntimeInitializeOnLoadMethod] thì kể cả khi bật "Enter Play
        // Mode Options" (tắt domain reload), trạng thái lần chạy trước vẫn không rò sang lần sau.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            shownRegionIds.Clear();
        }
    }
}
