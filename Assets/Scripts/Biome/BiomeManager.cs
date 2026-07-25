using UnityEngine;

namespace StarSower.Biome
{
    // Nhạc trưởng của bản sắc hình ảnh một khu vực: đọc RegionData rồi giao việc cho
    // BackgroundManager (nền) và SkyManager (trời), phát nhạc/ambient nếu có, và ghi nhớ khu vực
    // này để scene sau biết mà chuyển màu trời cho mượt.
    //
    // Chạy trong Awake() nên toàn bộ nền và trời đã đúng TRƯỚC khung hình đầu tiên — không có cảnh
    // loé một frame màu cũ. Vì thế hệ thống này KHÔNG cần ai gọi và không phải sửa transition:
    // LevelFlowManager cứ che/mở màn hình như cũ, biome đã sẵn sàng từ trước đó.
    //
    // Không đụng gì tới Player, Platform, Goal, Constellation hay Save.
    public class BiomeManager : MonoBehaviour
    {
        [Tooltip("Khu vực mà scene này thuộc về. Đổi asset là đổi toàn bộ diện mạo scene.")]
        [SerializeField] private RegionData region;

        [Header("Systems")]
        [SerializeField] private BackgroundManager backgroundManager;
        [SerializeField] private SkyManager skyManager;

        [Header("Transition")]
        [Tooltip("Thời gian bầu trời chuyển từ màu khu vực trước sang khu vực này.")]
        [SerializeField] private float skyTransitionDuration = 1.5f;

        [Header("Audio (placeholder — chưa có clip nào)")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;

        public RegionData Region => region;

        // LevelFlowManager lấy tên này để đưa cho RegionIntroUI, nhờ vậy tên khu vực chỉ tồn tại ở
        // MỘT chỗ duy nhất là RegionData.
        public string RegionName => region != null ? region.RegionName : string.Empty;

        private void Awake()
        {
            if (region == null)
            {
                Debug.LogError("[Biome] Chua gan Region cho BiomeManager cua scene nay.", this);
                return;
            }

            RegionData previous = BiomeSession.LastRegion;

            if (backgroundManager != null)
                backgroundManager.Apply(region);

            ApplySky(previous);
            PlayAudio();

            BiomeSession.Remember(region);
        }

        private void ApplySky(RegionData previous)
        {
            if (skyManager == null)
                return;

            // Vào thẳng scene này từ Editor, hoặc chơi lại chính khu vực vừa chơi: không có "màu
            // trước" nào đáng để chuyển mượt, đặt luôn màu đúng.
            if (previous == null || previous == region)
            {
                skyManager.ApplyImmediate(region);
                return;
            }

            skyManager.ApplyImmediate(previous);
            StartCoroutine(skyManager.BlendTo(previous, region, skyTransitionDuration));
        }

        // Chưa có clip thật nên hiện tại luôn im lặng. Designer gắn clip vào RegionData là chạy,
        // không phải sửa dòng code nào.
        private void PlayAudio()
        {
            PlayLoop(musicSource, region.DefaultMusic);
            PlayLoop(ambientSource, region.Ambient);
        }

        private static void PlayLoop(AudioSource source, AudioClip clip)
        {
            if (source == null || clip == null)
                return;

            source.clip = clip;
            source.loop = true;
            source.Play();
        }
    }
}
