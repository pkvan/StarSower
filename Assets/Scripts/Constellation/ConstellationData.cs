using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Constellations
{
    // Một nét nối giữa 2 ngôi sao trong chòm (theo chỉ số trong danh sách starPoints).
    [Serializable]
    public class StarConnection
    {
        public int fromIndex;
        public int toIndex;
    }

    // Định nghĩa TĨNH của 1 chòm sao: tên, icon, mốc fragment để khôi phục, hình dạng, và cấu hình
    // hiệu ứng riêng của nó (thời lượng, particle, âm thanh, độ hoành tráng). KHÔNG chứa tiến trình
    // người chơi — "đã khôi phục chưa" nằm trong SaveData, để file asset của designer không bị ghi
    // đè mỗi lần chơi và reset save không làm mất cấu hình.
    //
    // requiredFragments là mốc CỘNG DỒN trong chapter (12 / 30 / 53), không phải hạn ngạch riêng.
    [CreateAssetMenu(fileName = "Constellation", menuName = "StarSower/Constellation")]
    public class ConstellationData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Id ổn định dùng để lưu tiến trình — đổi id sẽ mất trạng thái đã khôi phục.")]
        [SerializeField] private string constellationId;
        [SerializeField] private string displayName;

        [Tooltip("Câu mô tả ngắn hiện dưới tên lúc khôi phục, vd \"The Hunter\". Placeholder, sẽ viết lại sau.")]
        [SerializeField] private string description;

        [SerializeField] private Sprite icon;

        [Header("Checkpoint")]
        [Tooltip("Mốc CỘNG DỒN của cả chapter để khôi phục chòm sao này (vd 12, 30, 53).")]
        [SerializeField] private int requiredFragments = 12;

        [Header("Shape")]
        [Tooltip("Vị trí từng ngôi sao, toạ độ chuẩn hoá 0..1 theo màn hình (0,0 = góc dưới trái).")]
        [SerializeField] private List<Vector2> starPoints = new List<Vector2>();

        [Tooltip("Các nét nối giữa 2 ngôi sao, theo chỉ số trong Star Points.")]
        [SerializeField] private List<StarConnection> connections = new List<StarConnection>();

        [Header("Restoration Effect")]
        [Tooltip("Tổng thời lượng trình diễn khôi phục chòm sao này. Mốc sau nên dài hơn mốc trước.")]
        [SerializeField] private float animationDuration = 4f;

        [Tooltip("Độ hoành tráng: 1 = bình thường, tăng dần ở các mốc sau (sao to hơn, nét dày hơn).")]
        [SerializeField] private float effectScale = 1f;

        [Tooltip("Để trống là bỏ qua — chưa có asset particle thật.")]
        [SerializeField] private GameObject particlePrefab;

        [Tooltip("Để trống là bỏ qua — chưa có asset âm thanh thật.")]
        [SerializeField] private AudioClip audioClip;

        public string ConstellationId => constellationId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int RequiredFragments => Mathf.Max(1, requiredFragments);
        public IReadOnlyList<Vector2> StarPoints => starPoints;
        public IReadOnlyList<StarConnection> Connections => connections;
        public float AnimationDuration => Mathf.Max(0.1f, animationDuration);
        public float EffectScale => Mathf.Max(0.1f, effectScale);
        public GameObject ParticlePrefab => particlePrefab;
        public AudioClip AudioClip => audioClip;
    }
}
