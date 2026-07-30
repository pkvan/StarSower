using System.Collections;
using UnityEngine;
using StarSower.Player;

namespace StarSower.Constellations
{
    // Nghe ChapterProgressManager báo chạm mốc rồi chạy SỰ KIỆN KHÔI PHỤC ngay giữa gameplay:
    // khoá điều khiển ~1 giây -> trình diễn chòm sao -> particle + âm thanh -> trả lại điều khiển.
    // Không chuyển scene, không mở menu, không popup.
    //
    // Việc khoá điều khiển dùng API SetMovementLocked() sẵn có của PlayerController — không sửa
    // PlayerController, chỉ gọi. Camera không bị đụng tới (chưa có hệ thống camera nhìn lên bầu trời).
    public class ConstellationManager : MonoBehaviour
    {
        [SerializeField] private ChapterProgressManager chapterProgress;
        [SerializeField] private PlayerController playerController;

        [Tooltip("Component implement IConstellationRestoreSequence — đổi component là đổi phong cách trình diễn.")]
        [SerializeField] private MonoBehaviour restoreSequenceSource;

        [Tooltip("Thời gian gameplay đứng lại trước khi chòm sao bắt đầu hiện ra.")]
        [SerializeField] private float pauseBeforeRestore = 1f;

        [Tooltip("Giữ chòm sao hoàn chỉnh + tên bao lâu sau khi vẽ xong, trước khi cả hai cùng tan.")]
        [SerializeField] private float holdAfterReveal = 1f;

        [Tooltip("Thời lượng tan, dùng CHUNG cho cả chòm sao lẫn thẻ tên nên hai thứ biến mất cùng lúc.")]
        [SerializeField] private float fadeOutDuration = 0.8f;

        [Tooltip("Thẻ tên hiện sau khi vẽ xong chòm sao. Để trống thì bỏ qua bước này, phần còn lại chạy như cũ.")]
        [SerializeField] private ConstellationNameCard nameCard;

        [Tooltip("Để trống thì tự tạo một AudioSource lúc chạy — chưa có clip nào nên vẫn im lặng.")]
        [SerializeField] private AudioSource audioSource;

        private IConstellationRestoreSequence sequence;
        private bool isPlaying;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            sequence = restoreSequenceSource as IConstellationRestoreSequence;
            if (sequence == null)
                Debug.LogError("[Constellation] Restore Sequence Source chua gan hoac khong implement " +
                               "IConstellationRestoreSequence.", this);
        }

        private void OnEnable()
        {
            chapterProgress.OnCheckpointReached += HandleCheckpointReached;
        }

        private void OnDisable()
        {
            chapterProgress.OnCheckpointReached -= HandleCheckpointReached;
        }

        private void HandleCheckpointReached(ConstellationData constellation)
        {
            if (sequence == null || isPlaying)
                return;

            StartCoroutine(RestorationRoutine(constellation));
        }

        private IEnumerator RestorationRoutine(ConstellationData constellation)
        {
            isPlaying = true;
            Debug.Log($"[Constellation] Khoi phuc {constellation.DisplayName} tai moc " +
                      $"{constellation.RequiredFragments} (dang co {chapterProgress.FragmentsCollected}).", this);

            playerController.SetMovementLocked(true);
            yield return new WaitForSeconds(pauseBeforeRestore);

            PlayParticle(constellation);
            PlayAudio(constellation);

            // Tên hiện lên CÙNG LÚC với nét vẽ đầu tiên, không phải sau khi vẽ xong: người chơi
            // đang chứng kiến chòm sao được khôi phục và biết luôn nó là ai, thay vì xem xong mới
            // được cho biết tên. StartCoroutine (không yield) để hai thứ chạy song song.
            Coroutine nameFadeIn = nameCard != null
                ? StartCoroutine(nameCard.FadeIn(constellation))
                : null;

            yield return sequence.Reveal(constellation);

            // Bình thường fade in tên (0.6s) xong sớm hơn phần vẽ (vài giây) nên chỗ này không chờ
            // gì cả. Chỉ có tác dụng nếu ai đó chỉnh fade in dài hơn cả phần vẽ — khi ấy vẫn phải
            // đợi tên hiện đủ rồi mới bắt đầu tính giờ giữ.
            if (nameFadeIn != null)
                yield return nameFadeIn;

            yield return new WaitForSeconds(holdAfterReveal);

            // Tan cùng một nhịp: khởi động fade out của tên trước, rồi chờ chòm sao tan, rồi chờ
            // nốt tên. Cả hai bắt đầu mờ đi trong cùng một khung hình.
            Coroutine nameFadeOut = nameCard != null
                ? StartCoroutine(nameCard.FadeOut(fadeOutDuration))
                : null;

            yield return sequence.Dismiss(fadeOutDuration);

            if (nameFadeOut != null)
                yield return nameFadeOut;

            playerController.SetMovementLocked(false);
            isPlaying = false;
        }

        // Particle/âm thanh chưa có asset thật — để trống trong ConstellationData là bỏ qua êm, không
        // cần sửa code khi designer gắn asset vào sau.
        private void PlayParticle(ConstellationData constellation)
        {
            if (constellation.ParticlePrefab == null)
                return;

            GameObject spawned = Instantiate(constellation.ParticlePrefab, transform.position, Quaternion.identity);
            Destroy(spawned, constellation.AnimationDuration + 1f);
        }

        private void PlayAudio(ConstellationData constellation)
        {
            if (constellation.AudioClip == null || audioSource == null)
                return;

            audioSource.PlayOneShot(constellation.AudioClip);
        }
    }
}
