using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Biome
{
    // Người ghi DUY NHẤT lên các SpriteRenderer nền của scene. Nhận RegionData rồi rải sprite + màu
    // xuống từng lớp theo đúng thứ tự khai báo.
    //
    // Không biết Region nào đang chạy, không biết transition, không tự lắng nghe sự kiện — chỉ có
    // đúng một việc: "áp bộ nền này xuống". BiomeManager quyết định KHI NÀO gọi.
    //
    // Số lớp là do scene khai báo (layers), số lớp có dữ liệu là do RegionData quyết định. Lớp thừa
    // sẽ bị tắt renderer, nên thêm lớp thứ 2, thứ 3 sau này chỉ là việc kéo thả trong Inspector.
    public class BackgroundManager : MonoBehaviour
    {
        [Tooltip("Thứ tự phải khớp với Background Layers trong RegionData. Lớp 0 là lớp xa nhất.")]
        [SerializeField] private List<SpriteRenderer> layers = new List<SpriteRenderer>();

        [Tooltip("Chỉ số lớp đóng vai trò mây — alpha của nó sẽ nhân thêm Cloud Density. Để -1 nếu chưa có lớp mây.")]
        [SerializeField] private int cloudLayerIndex = -1;

        public void Apply(RegionData region)
        {
            if (region == null)
                return;

            IReadOnlyList<BackgroundLayerData> data = region.BackgroundLayers;

            for (int i = 0; i < layers.Count; i++)
            {
                SpriteRenderer layer = layers[i];
                if (layer == null)
                    continue;

                if (i >= data.Count || data[i] == null)
                {
                    layer.enabled = false;
                    continue;
                }

                layer.enabled = true;
                ApplyLayer(layer, data[i], i, region.CloudDensity);
            }
        }

        private void ApplyLayer(SpriteRenderer layer, BackgroundLayerData data, int index, float cloudDensity)
        {
            // Sprite để trống = giữ nguyên ảnh đang có trong scene. Giai đoạn placeholder chưa có
            // art thật nên hầu hết Region chỉ đổi màu, và đó là trường hợp phải chạy êm nhất.
            if (data.sprite != null)
                layer.sprite = data.sprite;

            Color color = data.color;
            if (index == cloudLayerIndex)
                color.a *= cloudDensity;

            layer.color = color;
        }
    }
}
