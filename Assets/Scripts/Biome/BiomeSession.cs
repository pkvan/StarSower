using UnityEngine;

namespace StarSower.Biome
{
    // Trí nhớ NGẮN HẠN, chỉ sống trong 1 lần chạy game: Region vừa rời khỏi là Region nào.
    //
    // Vì sao cần: mỗi Region là 1 scene riêng, load scene mới là mất sạch trạng thái cũ. Muốn bầu
    // trời "đổi màu mượt" thay vì nhảy phắt sang màu mới thì scene mới phải biết màu trời của scene
    // trước để lerp từ đó sang.
    //
    // Cố tình KHÔNG ghi vào SaveData: đây là trạng thái của một phiên chơi, không phải tiến trình
    // người chơi. Sprint này không được đụng Save System, và kể cả được thì cũng không nên —
    // lưu xuống đĩa sẽ khiến lần mở game sau blend từ một Region không còn liên quan.
    public static class BiomeSession
    {
        public static RegionData LastRegion { get; private set; }

        public static void Remember(RegionData region)
        {
            LastRegion = region;
        }

        // Reset khi vào Play Mode. Có [RuntimeInitializeOnLoadMethod] thì kể cả khi bật "Enter Play
        // Mode Options" (tắt domain reload) giá trị cũ vẫn bị dọn, không rò rỉ sang lần chạy sau.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            LastRegion = null;
        }
    }
}
