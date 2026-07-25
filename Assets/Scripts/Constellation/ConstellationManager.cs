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

            yield return sequence.Play(constellation);

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
