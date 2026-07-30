using UnityEngine;
using StarSower.Audio;

namespace StarSower.Biome
{
    // Nhạc trưởng cho KHÔNG KHÍ của Region: nhạc nền, âm thanh môi trường, hiệu ứng hạt. Tách khỏi
    // BiomeManager (chỉ lo nền + trời) để giữ một class một trách nhiệm — BiomeManager không cần
    // biết AudioManager/ParticleController tồn tại, và ngược lại.
    //
    // Đọc Region từ BiomeManager thay vì tự có field RegionData riêng: chỉ có MỘT nơi quyết định
    // "scene này thuộc Region nào" (BiomeManager.Region), tránh 2 chỗ khai báo lệch nhau.
    //
    // Chạy trong Awake() giống BiomeManager — nhạc + âm thanh môi trường bắt đầu fade in TRƯỚC khi
    // transition mở màn hình. Đây là chủ ý: nhạc dẫn trước hình ảnh một nhịp là kỹ thuật thường
    // gặp, và tránh phải sửa LevelFlowManager/SceneTransitionController cho bước "vào scene" —
    // giống hệt lý do BiomeManager áp nền/trời trong Awake() ở S1-013.
    public class RegionAtmosphereManager : MonoBehaviour
    {
        [SerializeField] private BiomeManager biomeManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private ParticleController particleController;

        [Tooltip("Tuỳ chọn (S1-014C-001). Để trống thì Region này không có ambient nhiều lớp — vẫn dùng Ambient đơn giản như cũ nếu RegionData có gán.")]
        [SerializeField] private LayeredAmbientPlayer layeredAmbientPlayer;

        [Header("Fade Timing")]
        [Tooltip("Thời gian nhạc + âm thanh môi trường fade in lúc vào Region.")]
        [SerializeField] private float fadeInDuration = 2f;

        [Tooltip("Thời gian fade out lúc rời Region. Gọi từ LevelFlowManager ngay khi màn hình bắt đầu che.")]
        [SerializeField] private float fadeOutDuration = 1f;

        private void Awake()
        {
            RegionData region = biomeManager != null ? biomeManager.Region : null;
            if (region == null)
                return;

            if (audioManager != null)
            {
                WarnIfMissing(region, region.DefaultMusic, "Music Clip");

                // Chỉ cảnh báo thiếu Ambient Clip khi Region KHÔNG có Ambient Profile. Có Profile
                // nghĩa là khu vực đã dùng ambient nhiều lớp (gió lặp + chim/lá ngẫu nhiên), ô
                // Ambient Clip đơn giản để trống là CỐ Ý — cảnh báo lúc đó là báo động giả.
                if (region.AmbientProfile == null)
                    WarnIfMissing(region, region.Ambient, "Ambient Clip");

                audioManager.PlayMusic(region.DefaultMusic, region.MusicVolume, fadeInDuration);
                audioManager.PlayAmbient(region.Ambient, region.AmbientVolume, fadeInDuration);
            }

            if (particleController != null)
                particleController.Switch(region.ParticlePrefabs);

            if (layeredAmbientPlayer != null)
                layeredAmbientPlayer.Play(region.AmbientProfile);
        }

        // Không throw, không chặn gameplay — thiếu clip là trạng thái HỢP LỆ ở giai đoạn placeholder
        // (xem PROJECT_CONTEXT.md §9.1). Cảnh báo chỉ để designer biết còn khu vực nào chưa gắn asset.
        private void WarnIfMissing(RegionData region, AudioClip clip, string fieldName)
        {
            if (clip == null)
                Debug.LogWarning($"[Atmosphere] Region '{region.RegionName}' chua gan {fieldName} " +
                                  "— se im lang, khong phai loi.", this);
        }

        // Gọi bởi LevelFlowManager ngay khi màn hình bắt đầu che (PlayIn), để nhạc im hẳn TRƯỚC lúc
        // scene bị Unity phá huỷ thay vì bị cắt cụt. Không có coroutine nào để yield — cố tình chạy
        // song song với transition, không kéo dài thời gian chờ của người chơi.
        public void FadeOutForDeparture()
        {
            if (audioManager != null)
                audioManager.FadeOutAll(fadeOutDuration);

            layeredAmbientPlayer?.Stop();
        }
    }
}
