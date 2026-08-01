using UnityEngine;
using UnityEngine.UI;
using StarSower.Biome;

namespace StarSower.UI
{
    // Tô màu chữ cho các nhãn nổi TRỰC TIẾP trên gameplay theo khu vực đang chơi (S2-007).
    //
    // Vì sao phải theo khu vực: nền 5 khu trải từ trắng gần tinh (Cloud Garden, độ sáng 0.82) tới
    // xanh đêm (Aurora Cliffs 0.37). Một màu chữ dùng chung thì hoặc mất chữ ở khu sáng, hoặc mất
    // chữ ở khu tối — chỉ là dời lỗi sang chỗ khác chứ không sửa được.
    //
    // Vì sao là component RIÊNG chứ không nhét vào CollectibleHUD/RegionIntroUI/RegionTitleUI: bốn
    // nhãn nằm ở ba script khác nhau, nhét vào từng cái thì cùng một luật màu bị chép ba lần. Ở đây
    // nó là NƠI DUY NHẤT ghi Text.color — đúng Single-Writer. Ba script kia chỉ ghi .text hoặc
    // alpha của CanvasGroup nên không giẫm chân.
    //
    // KHÔNG đụng tới chữ nằm trên bảng có nền riêng (Level Select, Level Complete): chỗ đó nền do
    // mình kiểm soát, màu chữ cố định là đúng.
    public class RegionTextTint : MonoBehaviour
    {
        [Tooltip("Nguồn màu. Để trống thì không tô gì cả, chữ giữ nguyên màu đặt sẵn trong scene.")]
        [SerializeField] private BiomeManager biomeManager;

        [Tooltip("Các nhãn cần tô. Chỉ đưa vào đây thứ nổi trực tiếp trên nền gameplay.")]
        [SerializeField] private Graphic[] targets;

        [Tooltip("Giữ nguyên alpha đang có của từng nhãn thay vì lấy alpha của màu khu vực. Bật khi " +
                 "nhãn nào đó cố tình mờ hơn các nhãn khác.")]
        [SerializeField] private bool preserveAlpha = true;

        // Start chứ không phải Awake: BiomeManager gán RegionData ở Awake của chính nó, đọc quá sớm
        // sẽ vớ phải null ở scene mà thứ tự Awake không như mong đợi.
        private void Start()
        {
            Apply();
        }

        public void Apply()
        {
            if (biomeManager == null || biomeManager.Region == null || targets == null)
                return;

            Color tint = biomeManager.Region.UITextColor;

            for (int i = 0; i < targets.Length; i++)
            {
                Graphic g = targets[i];
                if (g == null)
                    continue;

                Color c = tint;
                if (preserveAlpha)
                    c.a = g.color.a;

                g.color = c;
            }
        }
    }
}
