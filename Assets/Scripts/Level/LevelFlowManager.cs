using System.Collections;
using UnityEngine;
using StarSower.Biome;
using StarSower.Core;
using StarSower.Player;
using StarSower.CameraSystem;
using StarSower.Collectibles;
using StarSower.Transition;
using StarSower.UI;

namespace StarSower.Level
{
    // Điều phối TOÀN BỘ trình tự "chạm Goal -> chuyển khu vực" (S1-011) — đây là nơi DUY NHẤT xử
    // lý GameEvents.OnLevelCompleted. Goal/UI/Save đều không tự gọi nhau; LevelFlowManager là nơi
    // gom PlayerController (khoá di chuyển), CameraFollow2D (dừng để tự lái camera lướt lên),
    // SceneTransitionController (che/mở màn hình), ProgressManager (lưu), LevelManager (load
    // scene kế) và RegionIntroUI (hiện tên khu vực) lại thành 1 trình tự liền mạch, không popup.
    public class LevelFlowManager : MonoBehaviour
    {
        [Header("Region")]
        [Tooltip("Tên khu vực hiện tại — CHỈ dùng khi Biome Manager để trống. Có Biome Manager thì " +
                 "tên lấy từ RegionData để không phải khai hai nơi.")]
        [SerializeField] private string regionDisplayName;

        [Tooltip("Tuỳ chọn (S1-013). Để trống thì mọi thứ chạy y như cũ theo Region Display Name.")]
        [SerializeField] private BiomeManager biomeManager;

        [Tooltip("Tuỳ chọn (S1-014). Để trống thì nhạc/âm thanh môi trường cứ tiếp tục chạy tới khi scene bị huỷ.")]
        [SerializeField] private RegionAtmosphereManager regionAtmosphereManager;

        [Tooltip("Tuỳ chọn (S1-014C-008). Để trống thì không hiện tên chòm sao của khu vực.")]
        [SerializeField] private RegionTitleUI regionTitleUI;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LevelTimer levelTimer;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private ProgressManager progressManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SceneTransitionController sceneTransitionController;
        [SerializeField] private RegionIntroUI regionIntroUI;

        [Header("Goal Flow Timing")]
        [Tooltip("Player đứng yên bao lâu trước khi camera bắt đầu lướt lên (0.3~0.5s theo thiết kế).")]
        [SerializeField] private float cameraDelay = 0.4f;
        [SerializeField] private float cameraDriftDistance = 3f;
        [SerializeField] private float cameraDriftDuration = 0.6f;
        [Tooltip("Thời gian giữ màn hình che kín trước khi thực sự load scene kế (che khoảng khựng lúc load).")]
        [SerializeField] private float transitionHoldDuration = 0.3f;
        [SerializeField] private bool autoLoadNextScene = true;

        private void OnEnable()
        {
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void Start()
        {
            StartCoroutine(ArrivalRoutine());
        }

        // Lúc vừa vào scene: khoá di chuyển, đảm bảo màn hình đang che kín (phòng trường hợp vào
        // thẳng scene này mà không qua transition, vd mở trực tiếp trong Editor để test), mở dần,
        // hiện tên khu vực, rồi mới trả lại quyền điều khiển — đúng "Tiếp tục điều khiển ngay"
        // sau khi tên khu vực biến mất.
        private IEnumerator ArrivalRoutine()
        {
            playerController.SetMovementLocked(true);
            sceneTransitionController.SnapCovered();

            yield return sceneTransitionController.PlayOut();
            yield return regionIntroUI.ShowRegionName(ResolveRegionName());

            playerController.SetMovementLocked(false);

            // Tên chòm sao chạy CHỒNG LÊN gameplay, không chặn: gọi SAU khi đã trả quyền điều
            // khiển và KHÔNG yield. Người chơi vừa đi vừa đọc, đúng yêu cầu "do not interrupt
            // gameplay". ShowOnce() tự lo phần chỉ-hiện-một-lần, LevelFlowManager không cần biết.
            regionTitleUI?.ShowOnce();
        }

        // Nguồn sự thật của tên khu vực là RegionData (S1-013). Giữ lại regionDisplayName làm đường
        // lui để scene nào chưa gắn BiomeManager vẫn chạy đúng như trước.
        private string ResolveRegionName()
        {
            if (biomeManager != null && !string.IsNullOrEmpty(biomeManager.RegionName))
                return biomeManager.RegionName;

            return regionDisplayName;
        }

        private void HandleLevelCompleted()
        {
            StartCoroutine(DepartureRoutine());
        }

        // Lúc chạm Goal: khoá di chuyển -> đứng yên ngắn -> camera lướt thêm lên trên -> che màn
        // hình -> lưu tiến trình -> load khu vực kế (nếu có). Không có bước nào chờ người chơi bấm.
        private IEnumerator DepartureRoutine()
        {
            levelTimer.StopTimer();
            playerController.SetMovementLocked(true);

            yield return new WaitForSeconds(cameraDelay);
            yield return DriftCameraUp();

            // Bắt đầu fade âm thanh về im lặng CÙNG LÚC màn hình bắt đầu che — không yield, để nhạc
            // im hẳn trước khi scene bị Unity phá huỷ thay vì bị cắt cụt (xem AudioManager).
            regionAtmosphereManager?.FadeOutForDeparture();

            yield return sceneTransitionController.PlayIn();
            yield return new WaitForSeconds(transitionHoldDuration);

            int starRating = ProgressManager.ComputeStarRating(collectibleManager.CollectedStars, collectibleManager.TotalStars);
            progressManager.CompleteLevel(levelManager.CurrentLevelId, starRating,
                collectibleManager.CollectedStars, levelTimer.ElapsedTime);

            if (autoLoadNextScene && levelManager.HasNextLevel)
            {
                levelManager.LoadNextLevel();
                yield break;
            }

            // Không còn khu vực kế (hoặc autoLoadNextScene tắt) — chưa có màn "Hoàn thành Chapter"
            // nên chỉ mở lại màn hình để không kẹt ở trạng thái che kín.
            yield return sceneTransitionController.PlayOut();
            playerController.SetMovementLocked(false);
        }

        // Tắt CameraFollow2D từ NGOÀI (không sửa file CameraFollow2D.cs) để tự lái camera lướt
        // thẳng lên trong lúc Player đã đứng yên — CameraFollow2D vốn chỉ ghi transform.position
        // trong LateUpdate() của chính nó nên tắt component là đủ để nhường quyền hoàn toàn.
        private IEnumerator DriftCameraUp()
        {
            cameraFollow.enabled = false;

            Vector3 start = cameraTransform.position;
            Vector3 end = start + new Vector3(0f, cameraDriftDistance, 0f);
            float elapsed = 0f;
            while (elapsed < cameraDriftDuration)
            {
                elapsed += Time.deltaTime;
                cameraTransform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / cameraDriftDuration));
                yield return null;
            }
            cameraTransform.position = end;
        }
    }
}
