using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSower.Audio;
using StarSower.CameraSystem;

namespace StarSower.Cinematic
{
    // Cảnh kết Chapter 1 (S1-019): camera lùi xa dần, người chơi nhỏ lại, cả hành trình 5 khu vực
    // hiện ra phía dưới. Không UI thắng cuộc, không chữ, không menu — chỉ có camera và nhạc.
    //
    // VÌ SAO PHẢI DỰNG PHÔNG NỀN: lúc chạy cảnh này chỉ có Level_05 nằm trong bộ nhớ. Bốn khu trước
    // là bốn scene riêng, đã bị Unity phá huỷ từ lâu — zoom xa tới đâu cũng chỉ thấy khoảng không
    // dưới Moon Gate. Nên "nhìn lại hành trình" được dựng bằng chính ảnh background_far của 5 khu,
    // xếp dọc và chồng mép nhau để trời chuyển màu liền mạch từ rừng lên tới cổng trăng.
    //
    // Phông nằm ở sortingOrder -50: che hai lớp nền parallax (-100/-90) nhưng vẫn nằm SAU platform
    // và Player (0), nên hành trình leo thật vẫn hiện rõ chồng lên trên.
    //
    // Không đụng CameraFollow2D.cs — chỉ tắt component từ ngoài rồi tự lái, đúng cách
    // LevelFlowManager.DriftCameraUp() đã làm từ S1-011.
    public class JourneyCinematic : MonoBehaviour
    {
        [Header("Tham chiếu")]
        [SerializeField] private Camera cinematicCamera;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private AudioManager audioManager;

        [Header("Nhạc")]
        [SerializeField] private AudioClip cinematicMusic;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 1f;
        [Tooltip("Nhạc Moon Gate và nhạc cảnh kết chồng lên nhau trong khoảng này. Cả hai cùng ở " +
                 "MỘT scene nên đây là crossfade THẬT (khác lúc chuyển scene, chỗ đó buộc phải " +
                 "fade hẳn về 0 rồi mới fade lên).")]
        [SerializeField] private float musicCrossfade = 2.5f;

        [Header("Nhịp (giây)")]
        [Range(1f, 6f)]
        [SerializeField] private float zoomOutDuration = 2.5f;
        [Range(1f, 8f)]
        [SerializeField] private float holdDuration = 4f;
        [Range(0.5f, 5f)]
        [SerializeField] private float returnDuration = 2f;

        [Header("Khung hình")]
        [Tooltip("Ortho size lúc lùi xa nhất. 44 = nhìn thấy 88 unit chiều cao, tức gần trọn đường " +
                 "leo Moon Gate. Càng lớn càng thấy nhiều nhưng Player càng bé.")]
        [SerializeField] private float targetOrthoSize = 44f;

        [Tooltip("Đẩy Player lên cao trong khung hình để chừa chỗ nhìn xuống hành trình phía dưới. " +
                 "0.75 = Player nằm ở khoảng 88% chiều cao khung.")]
        [Range(0f, 0.95f)]
        [SerializeField] private float framingBias = 0.75f;

        [Header("Phông hành trình (dưới lên trên)")]
        [Tooltip("Đúng thứ tự người chơi đã đi: Forgotten Forest → Cloud Garden → Sky Ruins → " +
                 "Aurora Cliffs → Moon Gate.")]
        [SerializeField] private List<Sprite> journeySprites = new List<Sprite>();

        [SerializeField] private float panelScale = 36f;
        [Tooltip("Khoảng cách giữa hai tấm. Nhỏ hơn chiều cao tấm để chúng CHỒNG MÉP — nhờ vậy trời " +
                 "chuyển màu liền mạch giữa các khu thay vì có đường cắt ngang.")]
        [SerializeField] private float panelSpacing = 23f;
        [SerializeField] private float panelBottomY = 10f;

        [Tooltip("Sau hai lớp nền parallax (-100/-90), trước platform (0).")]
        [SerializeField] private int panelSortingOrder = -50;

        [Header("Che UI")]
        [Tooltip("Tắt trong lúc chiếu, bật lại sau. Yêu cầu thiết kế: không có UI nào chen vào.")]
        [SerializeField] private List<GameObject> hideDuringCinematic = new List<GameObject>();

        private readonly List<SpriteRenderer> panels = new List<SpriteRenderer>();
        private bool isPlaying;

        public IEnumerator Play()
        {
            // Chống chạy chồng: gọi lần hai lúc đang chiếu sẽ dựng thêm một bộ phông nữa và ghi đè
            // vị trí camera giữa chừng — cảnh vỡ mà không có lỗi nào hiện ra.
            if (isPlaying)
                yield break;

            isPlaying = true;
            BuildPanels();

            foreach (GameObject go in hideDuringCinematic)
            {
                if (go != null)
                    go.SetActive(false);
            }

            if (audioManager != null && cinematicMusic != null)
                audioManager.PlayMusic(cinematicMusic, musicVolume, musicCrossfade);

            // Nhường quyền điều khiển camera. CameraFollow2D chỉ ghi transform trong LateUpdate của
            // chính nó nên tắt component là đủ, không cần sửa file đó.
            if (cameraFollow != null)
                cameraFollow.enabled = false;

            float startSize = cinematicCamera.orthographicSize;
            Vector3 startPos = cameraTransform.position;
            float playerY = playerTransform != null ? playerTransform.position.y : startPos.y;
            Vector3 endPos = new Vector3(0f, playerY - targetOrthoSize * framingBias, startPos.z);

            yield return Move(startSize, targetOrthoSize, startPos, endPos, zoomOutDuration, fadeIn: true);
            yield return new WaitForSeconds(holdDuration);
            yield return Move(targetOrthoSize, startSize, endPos, startPos, returnDuration, fadeIn: false);

            ClearPanels();

            foreach (GameObject go in hideDuringCinematic)
            {
                if (go != null)
                    go.SetActive(true);
            }

            if (cameraFollow != null)
                cameraFollow.enabled = true;

            isPlaying = false;
        }

#if UNITY_EDITOR
        // Chạy thử mà không phải leo hết 102 unit của Moon Gate: bấm Play, chọn GameObject
        // JourneyCinematic, chuột phải lên tiêu đề component này rồi chọn mục dưới đây.
        // Chỉ tồn tại trong Editor, không vào bản build.
        [ContextMenu("Chay thu canh ket (chi luc dang Play)")]
        private void DebugPlay()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[JourneyCinematic] Phai dang o che do Play moi chay thu duoc.", this);
                return;
            }

            StartCoroutine(Play());
        }
#endif

        // SmoothStep hai đầu: không giật lúc bắt đầu, không khựng lúc dừng — yêu cầu "smooth easing,
        // no sudden movement".
        private IEnumerator Move(float fromSize, float toSize, Vector3 fromPos, Vector3 toPos,
                                 float duration, bool fadeIn)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

                cinematicCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, t);
                cameraTransform.position = Vector3.Lerp(fromPos, toPos, t);
                SetPanelAlpha(fadeIn ? t : 1f - t);
                yield return null;
            }

            cinematicCamera.orthographicSize = toSize;
            cameraTransform.position = toPos;
            SetPanelAlpha(fadeIn ? 1f : 0f);
        }

        private void BuildPanels()
        {
            ClearPanels();

            for (int i = 0; i < journeySprites.Count; i++)
            {
                if (journeySprites[i] == null)
                    continue;

                var go = new GameObject($"JourneyPanel_{i}", typeof(SpriteRenderer));
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.position = new Vector3(0f, panelBottomY + panelSpacing * i, 0f);
                go.transform.localScale = Vector3.one * panelScale;

                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = journeySprites[i];
                // Tấm trên vẽ đè lên tấm dưới một chút, để chỗ chồng mép hoà dần theo hướng đi lên.
                renderer.sortingOrder = panelSortingOrder + i;
                renderer.color = new Color(1f, 1f, 1f, 0f);
                panels.Add(renderer);
            }
        }

        private void SetPanelAlpha(float alpha)
        {
            foreach (SpriteRenderer r in panels)
            {
                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }
        }

        private void ClearPanels()
        {
            foreach (SpriteRenderer r in panels)
            {
                if (r != null)
                    Destroy(r.gameObject);
            }
            panels.Clear();
        }
    }
}
