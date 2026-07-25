using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Biome
{
    // Rải sprite trang trí TĨNH khắp thế giới của một Region (mây Cloud Garden, sau này có thể là
    // mảnh đá bay Sky Ruins, dải sáng Aurora Cliffs...). Người chơi leo XUYÊN QUA chúng.
    //
    // Khác hoàn toàn AmbientParticleField — hai thứ giải hai bài toán ngược nhau, đừng gộp:
    //   - AmbientParticleField: hạt sống quanh NGƯỜI CHƠI, luôn ở trong khung hình, có vòng đời,
    //     tái sinh liên tục. Đúng cho lá bay lượn quanh nhân vật.
    //   - WorldAmbientField (class này): vật thể có VỊ TRÍ CỐ ĐỊNH trong thế giới, không vòng đời,
    //     không tái sinh. Camera đi qua thì chúng ra khỏi khung hình và biến mất, đúng như platform.
    //
    // Đây là điểm mấu chốt của cảm giác "leo qua tầng mây": nếu mây bám camera (dù chỉ 0.95 qua
    // ParallaxLayer) thì người chơi VÁC theo cả bầu trời, leo 33 unit mà mây chỉ lùi 1 unit — bầu
    // trời chết cứng, phẳng lì. Mây cố định thì mỗi lần leo lên là thật sự bỏ lại một tầng mây.
    //
    // KHÔNG làm con của Camera. KHÔNG dùng ParallaxLayer. KHÔNG bám Player.
    public class WorldAmbientField : MonoBehaviour
    {
        [Header("Sprites")]
        [Tooltip("Mỗi vật thể chọn ngẫu nhiên 1 sprite trong danh sách.")]
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        [Header("Vùng rải (theo transform của chính object này)")]
        [Tooltip("ParticleController gắn object này vào AtmosphereSystem ở gốc toạ độ, nên các số " +
                 "dưới đây trùng luôn với toạ độ world của scene.")]
        [SerializeField] private float worldYMin = -8f;
        [SerializeField] private float worldYMax = 40f;

        [Tooltip("Nửa bề rộng dải rải. Nên rộng hơn nửa bề ngang khung hình (~2.8 lúc dọc) để vật " +
                 "thể có lúc lấp ló ngoài rìa thay vì lúc nào cũng nằm giữa.")]
        [SerializeField] private float xRange = 4f;

        [Header("Số lượng & kích thước")]
        [Tooltip("Rải PHÂN TẦNG: dải Y chia đúng ngần này khoảng, mỗi khoảng một vật thể. Nhờ vậy " +
                 "không có chỗ chụm chỗ trống như khi random thuần.")]
        [SerializeField] private int count = 12;
        [SerializeField] private float scaleMin = 1.8f;
        [SerializeField] private float scaleMax = 3.2f;

        [Range(0f, 1f)]
        [SerializeField] private float opacity = 0.45f;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [Tooltip("Xem PROJECT_CONTEXT.md để biết sortingOrder các lớp khác đang dùng.")]
        [SerializeField] private int sortingOrder;

        [Header("Bố cục")]
        [Tooltip("Cùng seed = bầu trời xếp GIỐNG HỆT nhau qua mọi lần chơi. Chơi lại mà mây xếp " +
                 "khác đi thì khu vực mất cảm giác là một nơi chốn có thật.")]
        [SerializeField] private int seed = 1;

        [Header("Trôi ngang (tuỳ chọn — để 0 là đứng yên hoàn toàn)")]
        [Tooltip("Dao động SIN quanh vị trí gốc, không phải trôi đều một chiều: trôi đều thì sau " +
                 "vài phút vật thể đã lệch hẳn khỏi cột chơi. Biên độ nhỏ (~0.6) để gần như không thấy.")]
        [SerializeField] private float driftAmplitude;
        [SerializeField] private float driftPeriodMin = 40f;
        [SerializeField] private float driftPeriodMax = 80f;

        [Tooltip("Chỉ tính lại vị trí cho vật thể nằm trong khoảng này quanh camera. Việc RENDER " +
                 "ngoài khung hình đã được Unity tự cắt (frustum culling) — đây chỉ để bỏ bớt phép " +
                 "tính, không phải để tắt hiển thị.")]
        [SerializeField] private float driftActiveRange = 14f;

        private readonly List<Decoration> decorations = new List<Decoration>();
        private Transform cameraTransform;

        private class Decoration
        {
            public Transform Transform;
            public float BaseX;
            public float BaseY;
            public float DriftPhase;
            public float DriftFrequency;   // rad/giây
        }

        private void Awake()
        {
            if (sprites.Count == 0 || count <= 0)
            {
                enabled = false;
                return;
            }

            Build();

            // Không có trôi thì không có gì để cập nhật mỗi khung hình — tắt hẳn Update thay vì
            // chạy vòng lặp rỗng. Lớp bụi trời dùng đúng trường hợp này.
            if (driftAmplitude <= 0f)
                enabled = false;
            else
                cameraTransform = Camera.main != null ? Camera.main.transform : null;
        }

        private void Build()
        {
            // Mượn RNG toàn cục rồi TRẢ LẠI nguyên trạng: seed cố định chỉ được ảnh hưởng bố cục
            // của riêng lớp này, không được làm lệch mọi Random.Range khác trong cùng khung hình.
            Random.State previousState = Random.state;
            Random.InitState(seed);

            float band = (worldYMax - worldYMin) / count;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Decoration_{i}", typeof(SpriteRenderer));
                go.transform.SetParent(transform, worldPositionStays: false);

                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = sprites[Random.Range(0, sprites.Count)];
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder;
                renderer.color = new Color(1f, 1f, 1f, opacity);

                // Xê dịch trong khoảng của mình, chừa mép để hai vật thể liền kề không dính nhau.
                float y = worldYMin + band * (i + Random.Range(0.15f, 0.85f));
                float x = Random.Range(-xRange, xRange);

                go.transform.localPosition = new Vector3(x, y, 0f);
                go.transform.localScale = Vector3.one * Random.Range(scaleMin, scaleMax);

                decorations.Add(new Decoration
                {
                    Transform = go.transform,
                    BaseX = x,
                    BaseY = y,
                    DriftPhase = Random.Range(0f, Mathf.PI * 2f),
                    DriftFrequency = Mathf.PI * 2f / Random.Range(driftPeriodMin, driftPeriodMax),
                });
            }

            Random.state = previousState;
        }

        private void Update()
        {
            if (cameraTransform == null)
                return;

            float cameraY = cameraTransform.position.y;
            float t = Time.time;

            foreach (Decoration d in decorations)
            {
                if (Mathf.Abs(d.BaseY - cameraY) > driftActiveRange)
                    continue;

                float x = d.BaseX + Mathf.Sin(t * d.DriftFrequency + d.DriftPhase) * driftAmplitude;
                d.Transform.localPosition = new Vector3(x, d.BaseY, 0f);
            }
        }
    }
}
