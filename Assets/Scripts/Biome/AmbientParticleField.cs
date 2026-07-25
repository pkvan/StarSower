using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Biome
{
    // Hạt môi trường 2D nhẹ, tự pool, dùng CHUNG cho mọi khu vực (lá rơi Forgotten Forest, bụi nắng
    // Forgotten Forest, và sau này mây trôi Cloud Garden, đom đóm Aurora Cliffs, sao lấp lánh Moon
    // Gate...). Không biết Region/Biome gì cả — chỉ nhận cấu hình rồi tự chạy, y hệt tinh thần
    // ParticleController.
    //
    // KHÔNG dùng Unity ParticleSystem (Shuriken): dự án chưa có Editor để dựng/kiểm tra module
    // Shuriken bằng tay (rất dễ ra asset hỏng khi viết YAML thủ công). Toàn bộ hiệu ứng runtime của
    // dự án từ trước tới giờ (ConstellationRestoreSequence, SkyManager) đều dựng bằng code, không
    // phải asset Editor — component này theo đúng tiền lệ đó: 1 vòng lặp trung tâm dịch chuyển N
    // SpriteRenderer đã tạo sẵn, không Instantiate/Destroy trong lúc chạy, không allocation mỗi khung
    // hình sau khi khởi tạo — đúng yêu cầu hiệu năng mobile.
    //
    // Đặt làm con của Main Camera trong scene (không sửa component nào trên Camera, chỉ thêm con
    // trong hierarchy — đúng tiền lệ SkyPlane ở S1-013) để vùng phát hạt luôn nằm trong khung hình
    // dù người chơi leo cao tới đâu, không cần script nào bám theo camera.
    public class AmbientParticleField : MonoBehaviour
    {
        [Header("Sprites")]
        [Tooltip("Chọn ngẫu nhiên 1 sprite trong danh sách cho mỗi hạt — không dùng cố định 1 sprite.")]
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        [Header("Vùng phát (local space, quanh vị trí của object này)")]
        [Tooltip("Bề rộng x chiều cao vùng hạt được phép xuất hiện. Đặt object này làm con Camera thì vùng luôn khớp khung hình.")]
        [SerializeField] private Vector2 areaSize = new Vector2(12f, 8f);

        [Header("Số lượng")]
        [Tooltip("Số hạt tối đa cùng lúc — cũng là kích thước pool, tạo đúng 1 lần lúc Awake.")]
        [SerializeField] private int maxParticles = 12;

        [Header("Rơi / trôi")]
        [SerializeField] private float fallSpeedMin = 0.2f;
        [SerializeField] private float fallSpeedMax = 0.5f;
        [Tooltip("Trôi NGANG đều một chiều (unit/giây), cộng thêm vào lắc sin. Âm là trôi sang trái. " +
                 "Mặc định 0 = đứng yên theo phương ngang, đúng hành vi lá rơi Forgotten Forest. " +
                 "Dùng cho mây Cloud Garden (S1-015).")]
        [SerializeField] private float horizontalSpeedMin;
        [SerializeField] private float horizontalSpeedMax;

        [Tooltip("Biên độ lắc ngang kiểu sóng sin — tạo cảm giác trôi nhẹ thay vì rơi thẳng.")]
        [SerializeField] private float driftAmplitude = 0.3f;
        [SerializeField] private float driftFrequencyMin = 0.2f;
        [SerializeField] private float driftFrequencyMax = 0.6f;
        [SerializeField] private float rotationSpeedMin = -30f;
        [SerializeField] private float rotationSpeedMax = 30f;

        [Tooltip("Xoay hạt một góc ngẫu nhiên 0-360° lúc sinh ra. ĐÚNG cho lá rơi (lá nằm hướng " +
                 "nào cũng được), SAI cho mây (mây lộn ngược là lộ ngay). Tắt thì hạt luôn giữ " +
                 "đúng hướng của sprite gốc.")]
        [SerializeField] private bool randomizeInitialRotation = true;

        [Header("Kích thước / độ mờ")]
        [SerializeField] private float scaleMin = 0.5f;
        [SerializeField] private float scaleMax = 1f;
        [Tooltip("Độ mờ CAO NHẤT đạt được giữa vòng đời — hạt không bao giờ đục hơn giá trị này.")]
        [Range(0f, 1f)]
        [SerializeField] private float peakOpacity = 0.8f;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;

        [Header("Vòng đời")]
        [SerializeField] private float lifetimeMin = 6f;
        [SerializeField] private float lifetimeMax = 12f;

        [Header("Rendering")]
        [SerializeField] private string sortingLayerName = "Default";
        [Tooltip("Xem PROJECT_CONTEXT.md để biết sortingOrder các lớp khác đang dùng.")]
        [SerializeField] private int sortingOrder;

        private readonly List<Particle> particles = new List<Particle>();

        private class Particle
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public float Elapsed;
            public float Lifetime;
            public float FallSpeed;
            public float HorizontalSpeed;
            public float DriftAmplitude;
            public float DriftFrequency;
            public float DriftPhase;
            public float RotationSpeed;
            public float BaseScale;
            public float StartX;
        }

        private void Awake()
        {
            for (int i = 0; i < maxParticles; i++)
                particles.Add(CreateParticle(i));

            foreach (Particle p in particles)
                Respawn(p, randomizeElapsed: true, randomizeY: true);
        }

        private Particle CreateParticle(int index)
        {
            var go = new GameObject($"Particle_{index}", typeof(SpriteRenderer));
            go.transform.SetParent(transform, worldPositionStays: false);

            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;

            return new Particle { Transform = go.transform, Renderer = renderer };
        }

        // Không có sprite nào thì không có gì để phát — tắt hẳn component thay vì chạy vòng lặp rỗng
        // mỗi khung hình.
        private void OnEnable()
        {
            enabled = sprites.Count > 0;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            foreach (Particle p in particles)
            {
                p.Elapsed += dt;

                if (p.Elapsed >= p.Lifetime)
                {
                    // Hết vòng đời = một chiếc lá MỚI xuất hiện, không phải chiếc lá cũ "dạt ngược
                    // lên đỉnh". randomizeY: true để lá mới hiện ra ở bất kỳ độ cao nào trong vùng —
                    // đây chính là chỗ từng bị bug: trước đây luôn ép về đỉnh, mà vòng đời (6-12s)
                    // lại ngắn hơn nhiều thời gian rơi hết cả vùng (~12 / tốc độ rơi trung bình ≈
                    // 30s+), nên lá không bao giờ kịp rơi xuống được nửa dưới màn hình.
                    Respawn(p, randomizeElapsed: false, randomizeY: true);
                    continue;
                }

                float y = p.Transform.localPosition.y - p.FallSpeed * dt;
                float x = ComputeX(p);

                // Rơi hết xuống đáy vùng phát (hiếm khi xảy ra vì lifetime thường hết trước) — tái
                // sinh lại từ đỉnh để giữ đúng cảm giác "rơi từ trên xuống" liền mạch.
                if (y < -areaSize.y * 0.5f)
                {
                    Respawn(p, randomizeElapsed: false, randomizeY: false);
                    continue;
                }

                p.Transform.localPosition = new Vector3(x, y, 0f);
                p.Transform.Rotate(0f, 0f, p.RotationSpeed * dt);

                Color c = p.Renderer.color;
                c.a = ComputeAlpha(p) * peakOpacity;
                p.Renderer.color = c;
            }
        }

        // Vị trí ngang tại thời điểm hiện tại = điểm gốc + trôi đều một chiều + lắc sin. Tách thành
        // hàm riêng vì Respawn() cũng phải dùng đúng công thức này để đặt vị trí ban đầu — nếu chỉ
        // gán StartX như trước, hạt được hồi sinh với Elapsed ngẫu nhiên (lúc Awake) sẽ nhảy một
        // đoạn ở khung hình đầu tiên.
        //
        // CỐ Ý KHÔNG có wrap-around ở mép trái/phải: StartX đã được Respawn() lùi lại nửa quãng
        // đường của cả vòng đời, nên hạt đi từ ngoài vùng vào giữa rồi ra ngoài, và fade out lo nốt
        // hai đầu. Thêm wrap sẽ tạo cú "pop" nhảy từ mép này sang mép kia — đúng loại lỗi thị giác
        // đã gặp ở S1-014C-002.
        private float ComputeX(Particle p)
        {
            return p.StartX
                   + p.HorizontalSpeed * p.Elapsed
                   + Mathf.Sin((p.Elapsed + p.DriftPhase) * p.DriftFrequency) * p.DriftAmplitude;
        }

        // Fade in đầu vòng đời, giữ, fade out cuối vòng đời — không có hạt nào bật/tắt đột ngột.
        private float ComputeAlpha(Particle p)
        {
            float remaining = p.Lifetime - p.Elapsed;
            float fadeIn = fadeInDuration > 0f ? Mathf.Clamp01(p.Elapsed / fadeInDuration) : 1f;
            float fadeOut = fadeOutDuration > 0f ? Mathf.Clamp01(remaining / fadeOutDuration) : 1f;
            return Mathf.Min(fadeIn, fadeOut);
        }

        // Đặt lại một hạt về trạng thái mới: vị trí ngẫu nhiên trong vùng phát, sprite/tốc độ/vòng
        // đời ngẫu nhiên mới. KHÔNG Instantiate/Destroy — chỉ đổi state của GameObject đã có sẵn.
        //
        // randomizeElapsed và randomizeY là 2 việc KHÁC NHAU, cố tình tách riêng (bài học từ bug
        // "chỉ rơi ở 1/3 màn hình" — gộp chung 1 cờ khiến mọi lần hết vòng đời đều bị ép về đỉnh):
        //   - randomizeElapsed: hạt có "đã rơi được một lúc" khi vừa bật không (chỉ true lúc Awake,
        //     để không phải chờ cả vòng đời mới thấy hạt đầu tiên).
        //   - randomizeY: hạt mới xuất hiện ở ĐỘ CAO NÀO — true nghĩa là bất kỳ đâu trong vùng
        //     (dùng cho lúc bật VÀ mỗi lần hết vòng đời), false nghĩa là luôn tại đỉnh (chỉ dùng khi
        //     hạt vừa rơi chạm đáy, để giữ cảm giác rơi liền mạch từ trên xuống).
        private void Respawn(Particle p, bool randomizeElapsed, bool randomizeY)
        {
            p.Lifetime = Random.Range(lifetimeMin, lifetimeMax);
            p.Elapsed = randomizeElapsed ? Random.Range(0f, p.Lifetime) : 0f;
            p.FallSpeed = Random.Range(fallSpeedMin, fallSpeedMax);
            p.DriftAmplitude = driftAmplitude;
            p.DriftFrequency = Random.Range(driftFrequencyMin, driftFrequencyMax);
            p.DriftPhase = Random.Range(0f, Mathf.PI * 2f);
            p.RotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
            p.BaseScale = Random.Range(scaleMin, scaleMax);
            p.HorizontalSpeed = Random.Range(horizontalSpeedMin, horizontalSpeedMax);

            // Lùi điểm gốc lại NỬA quãng đường của cả vòng đời, để chỗ hạt đi qua lúc giữa đời —
            // tức lúc nó rõ nhất — rơi đúng vào vùng phát. Không lùi thì hạt trôi ngang luôn bắt
            // đầu trong vùng rồi trôi dần ra ngoài và nằm chết ở ngoài suốt nửa sau vòng đời.
            // horizontalSpeed = 0 thì số hạng này bằng 0, hành vi y hệt trước đây.
            p.StartX = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f)
                       - p.HorizontalSpeed * p.Lifetime * 0.5f;

            float startY = randomizeY
                ? Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f)
                : areaSize.y * 0.5f;

            p.Transform.localPosition = new Vector3(ComputeX(p), startY, 0f);
            p.Transform.localRotation = randomizeInitialRotation
                ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
                : Quaternion.identity;
            p.Transform.localScale = Vector3.one * p.BaseScale;

            p.Renderer.sprite = sprites[Random.Range(0, sprites.Count)];
            Color c = p.Renderer.color;
            c.a = 0f;
            p.Renderer.color = c;
        }
    }
}
