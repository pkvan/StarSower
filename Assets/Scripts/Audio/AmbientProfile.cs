using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Audio
{
    // Toàn bộ "không khí âm thanh" của một khu vực, gom thành 1 asset để designer sửa mà không đụng
    // scene hay code — đúng tinh thần RegionData (S1-013). Thêm khu vực mới có ambient riêng = tạo
    // 1 asset mới (Wind/Birds/Leaves cho Cloud Garden, "Magical Wind" cho Aurora Cliffs...) + gán
    // vào RegionData.AmbientProfile của khu vực đó, không sửa dòng code nào.
    //
    // Tách khỏi RegionData.Ambient (S1-014, một clip lặp đơn) vì đây là NHIỀU lớp chồng lên nhau —
    // 2 khái niệm phục vụ 2 mức độ phức tạp khác nhau, region nào chưa cần lớp phức tạp vẫn dùng
    // được Ambient đơn giản, không bị ép nâng cấp.
    [CreateAssetMenu(fileName = "AmbientProfile", menuName = "StarSower/Ambient Profile")]
    public class AmbientProfile : ScriptableObject
    {
        [SerializeField] private List<AmbientLayerData> layers = new List<AmbientLayerData>();

        public IReadOnlyList<AmbientLayerData> Layers => layers;
    }
}
