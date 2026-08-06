using System.Collections;
using UnityEngine;
using StarSower.Biome;
using StarSower.Constellations;
using StarSower.Core;
using StarSower.Player;
using StarSower.CameraSystem;
using StarSower.Cinematic;
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

        [Tooltip("Tuỳ chọn (S1-019). CHỈ chạy ở khu vực cuối cùng — nơi không còn level kế tiếp. " +
                 "Để trống thì màn cuối kết thúc y như cũ (che màn hình rồi mở lại).")]
        [SerializeField] private JourneyCinematic journeyCinematic;

        [Tooltip("Tuỳ chọn (S1-020A). Hiện SAU khi đã lưu tiến trình khu vực, TRƯỚC khi nạp khu kế. " +
                 "Để trống thì bỏ qua hoàn toàn, luồng cũ không đổi.")]
        [SerializeField] private ConstellationScreen constellationScreen;

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

        [Tooltip("Bật (S2-006): chạm Goal là che màn hình NGAY — bỏ qua quãng đứng yên và cú lướt " +
                 "camera lên. Dùng khi ngay sau đó là cảnh chòm sao: cảnh đó tự dựng khung hình " +
                 "riêng nên nấn ná ở khu vực cũ chỉ làm chậm, và Hero lúc đó có thể đang bay lơ " +
                 "lửng — lướt camera theo chỉ tổ khoe ra chỗ dở. Tắt thì luồng cũ y nguyên.")]
        [SerializeField] private bool instantDepartureFade;

        [Tooltip("Thời lượng che màn khi Instant Departure Fade bật. Đây là quãng DUY NHẤT còn " +
                 "nhìn thấy khu vực cũ sau khi chạm Goal, nên để ngắn.")]
        [Min(0f)]
        [SerializeField] private float instantFadeDuration = 1f;

        private void OnEnable()
        {
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
            GameEvents.OnGoalReached += HandleGoalReached;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            GameEvents.OnGoalReached -= HandleGoalReached;
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

        // Cham Astral Gate: khoa di chuyen NGAY, roi de cong dien canh mo ra. Chi rieng viec khoa
        // — khong dung toi camera, man hinh hay tien trinh, vi con cach OnLevelCompleted vai giay.
        // Khong co Astral Gate trong scene thi khong ai phat su kien nay, luong cu chay y nguyen.
        private void HandleGoalReached()
        {
            levelTimer.StopTimer();
            playerController.SetMovementLocked(true);
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

            if (!instantDepartureFade)
                yield return new WaitForSeconds(cameraDelay);

            // Khu vực CUỐI CÙNG: thay vì che màn hình rồi load scene kế (không còn scene nào), chiếu
            // cảnh kết Chapter 1. Chặn bằng HasNextLevel chứ không hardcode tên scene — thêm khu vực
            // mới vào LevelDatabase là cảnh này tự dời sang khu mới, không phải sửa dòng nào.
            //
            // KHÔNG gọi FadeOutForDeparture() ở nhánh này: cảnh kết tự crossfade sang nhạc riêng của
            // nó, gọi thêm sẽ dập nhạc về im lặng ngay giữa lúc đang chuyển.
            if (journeyCinematic != null && !levelManager.HasNextLevel)
            {
                // Lưu TRƯỚC, để màn hình chòm sao ngay dưới đây tính được cả khu vực vừa xong.
                int finalStars = ProgressManager.ComputeStarRating(collectibleManager.CollectedStars, collectibleManager.TotalStars);
                progressManager.CompleteLevel(levelManager.CurrentLevelId, finalStars,
                    collectibleManager.CollectedStars, levelTimer.ElapsedTime,
                    collectibleManager.TotalStars, collectibleManager.CollectedStars);

                // Chòm sao ĐI TRƯỚC cảnh kết (S1-020B): phần thưởng phải đến trước lời tạm biệt.
                // Xem cảnh kết xong rồi mới mở chòm sao thì cảm giác như một khúc đuôi thừa.
                if (constellationScreen != null)
                    yield return constellationScreen.Show();

                yield return journeyCinematic.Play();

                // Canh ket "dung han": man hinh o nguyen khung nhin lai nam khu vuc, nen KHONG
                // tra lai quyen dieu khien — tra lai thi Player van chay duoc trong khi camera
                // da doi cho va HUD da tat.
                if (!journeyCinematic.StaysAtEnd)
                    playerController.SetMovementLocked(false);
                yield break;
            }

            if (!instantDepartureFade)
                yield return DriftCameraUp();

            // Bắt đầu fade âm thanh về im lặng CÙNG LÚC màn hình bắt đầu che — không yield, để nhạc
            // im hẳn trước khi scene bị Unity phá huỷ thay vì bị cắt cụt (xem AudioManager).
            regionAtmosphereManager?.FadeOutForDeparture();

            if (instantDepartureFade)
            {
                // Bỏ luôn quãng giữ màn đen: quãng đó có mặt để che khoảng khựng lúc NẠP SCENE,
                // mà ở đây thứ đến ngay sau là màn chòm sao trong chính scene này — không nạp gì.
                yield return sceneTransitionController.PlayIn(instantFadeDuration);
            }
            else
            {
                yield return sceneTransitionController.PlayIn();
                yield return new WaitForSeconds(transitionHoldDuration);
            }

            int starRating = ProgressManager.ComputeStarRating(collectibleManager.CollectedStars, collectibleManager.TotalStars);
            progressManager.CompleteLevel(levelManager.CurrentLevelId, starRating,
                collectibleManager.CollectedStars, levelTimer.ElapsedTime,
                collectibleManager.TotalStars, collectibleManager.CollectedStars);

            // Chòm sao hiện SAU khi tiến trình đã lưu (nên số sao mở ra luôn tính cả khu vực vừa
            // xong) và TRƯỚC khi nạp scene kế — nạp scene sẽ phá huỷ luôn màn hình này.
            if (constellationScreen != null)
                yield return constellationScreen.Show();

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
