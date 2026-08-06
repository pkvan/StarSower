using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSower.Audio;
using UnityEngine.UI;
using StarSower.CameraSystem;
using StarSower.Constellations;

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

        [Tooltip("Toạ độ X tâm màn. Trước đây hardcode 0 — đúng với màn dọc cũ (tâm ở 0), nhưng " +
                 "màn S3 có tâm ở 6.5 nên để 0 là khung hình lệch hẳn sang trái.")]
        [SerializeField] private float levelCenterX;

        [Header("Khung đủ 5 phông hành trình (S3)")]
        [Tooltip("Bật: camera tự tính độ lùi vừa đủ để thấy TRỌN cả 5 tấm phông, thay vì dùng " +
                 "Target Ortho Size cố định. Cần bật vì số tấm và khoảng cách giữa chúng quyết " +
                 "định phải lùi bao xa — điền tay một con số là sai ngay khi đổi bố cục.")]
        [SerializeField] private bool fitAllPanels = true;

        [Tooltip("Chừa quanh mép bao nhiêu phần trăm. 1.12 = chừa 12%.")]
        [Range(1f, 1.5f)]
        [SerializeField] private float fitMargin = 1.12f;

        [Tooltip("Lệch NGANG giữa hai tấm liền nhau. Để 0 = xếp thành các DÒNG NGANG thẳng hàng, " +
                 "dòng dưới cùng là khu vực đầu tiên, dòng trên cùng là khu vực cuối.")]
        [SerializeField] private float panelStepX;

        [Tooltip("N dòng CAO BẰNG NHAU, xếp kín khung hình. Mỗi dòng là một dải ngang CẮT ra từ " +
                 "ảnh nền của khu vực đó — cắt chứ không kéo giãn, nên hình không bị bẹp.\n\n" +
                 "Tắt thì quay về kiểu cũ: cả tấm ảnh, chồng mép nhau theo Panel Spacing.")]
        [SerializeField] private bool equalRows = true;

        [Tooltip("Cắt lấy dải nào của ảnh: 0 = mép dưới, 0.5 = giữa, 1 = mép trên.")]
        [Range(0f, 1f)]
        [SerializeField] private float cropBias = 0.5f;

        [Tooltip("Dừng hẳn ở khung nhìn lùi xa, KHÔNG quay về và không trả lại quyền điều khiển. " +
                 "Đây là cảnh cuối của Chapter 1 — hết hành trình thì dừng lại ở đó.\n\n" +
                 "Tắt thì camera quay về chỗ cũ và trả lại HUD như luồng cũ.")]
        [SerializeField] private bool stayAtEnd = true;

        // LevelFlowManager doc de biet co nen tra lai quyen dieu khien hay khong.
        public bool StaysAtEnd => stayAtEnd;

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

        [Header("Nhãn từng dòng: tên khu vực + chòm sao")]
        [Tooltip("Bật thì mỗi dòng có tên khu vực bên TRÁI, tên + hình chòm sao bên PHẢI.")]
        [SerializeField] private bool showRowLabels = true;

        [Tooltip("Tên khu vực, ĐÚNG THỨ TỰ dưới lên trên — khớp với Journey Sprites.")]
        [SerializeField] private List<string> regionNames = new List<string>();

        [Tooltip("Chòm sao của từng khu vực, cùng thứ tự. Hình lấy từ Star Points của asset.")]
        [SerializeField] private List<ConstellationData> constellations = new List<ConstellationData>();

        [SerializeField] private Font labelFont;
        [SerializeField] private Sprite nodeSprite;
        [SerializeField] private Sprite lineSprite;

        [Tooltip("Nền phủ KÍN chiều cao mỗi dòng. Có nó thì chữ trắng đọc được cả trên dải trời " +
                 "sáng lẫn dải tối — không có thì tên trên dải rừng vàng gần như biến mất.")]
        [SerializeField] private Color plateColor = new Color(0.03f, 0.05f, 0.12f, 0.42f);
        [SerializeField] private Color labelColor = new Color(1f, 0.96f, 0.86f, 1f);
        [SerializeField] private Color labelOutlineColor = new Color(0f, 0f, 0f, 0.85f);
        [SerializeField] private Color nodeColor = new Color(1f, 0.97f, 0.85f, 1f);
        [SerializeField] private Color lineColor = new Color(0.85f, 0.9f, 1f, 0.65f);

        [Header("Nhịp hiện nhãn (giây)")]
        [Tooltip("Cách nhau bao lâu giữa hai dòng. Hiện lần lượt TỪ DƯỚI LÊN, đúng thứ tự đã đi.")]
        [Min(0f)]
        [SerializeField] private float rowRevealInterval = 0.55f;
        [Min(0.05f)]
        [SerializeField] private float labelFadeDuration = 0.5f;
        [Min(0.01f)]
        [SerializeField] private float nodePopInterval = 0.07f;
        [Min(0.05f)]
        [SerializeField] private float lineDrawDuration = 0.45f;

        [Header("Thử nhanh trong Unity")]
        [Tooltip("BẬT rồi bấm Play là cảnh kết chiếu ngay, không phải leo hết 5 màn. " +
                 "NHỚ TẮT trước khi build — bật thì vào màn là chạy cảnh kết luôn.")]
        [SerializeField] private bool playOnStartForTesting;

        [Tooltip("Chờ bao lâu sau khi vào màn rồi mới chiếu (chỉ dùng cho Play On Start For Testing).")]
        [Min(0f)]
        [SerializeField] private float testDelay = 0.5f;

        [Header("Che UI")]
        [Tooltip("Tắt trong lúc chiếu, bật lại sau. Yêu cầu thiết kế: không có UI nào chen vào.")]
        [SerializeField] private List<GameObject> hideDuringCinematic = new List<GameObject>();

        private readonly List<SpriteRenderer> panels = new List<SpriteRenderer>();
        // Sprite cắt lúc chạy: phải tự huỷ, Destroy GameObject không dọn hộ.
        private readonly List<Sprite> runtimeSprites = new List<Sprite>();
        private readonly List<CanvasGroup> rowGroups = new List<CanvasGroup>();
        private readonly List<RowShape> rowShapes = new List<RowShape>();
        private GameObject labelCanvas;
        private bool isPlaying;

        private void Start()
        {
            if (playOnStartForTesting)
                StartCoroutine(TestAfterDelay());
        }

        private IEnumerator TestAfterDelay()
        {
            // Cho mot nhip de scene dung xong (CameraAspectFitter chot orthographicSize o Awake,
            // LevelFlowManager mo man o Start) roi moi chieu.
            yield return new WaitForSeconds(testDelay);
            yield return Play();
        }

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

            float endSize = targetOrthoSize;
            Vector3 endPos;
            if (fitAllPanels && panels.Count > 0)
            {
                Bounds b = PanelBounds();
                // Lay canh CANG hon trong hai chieu: lui du de vua chieu cao, va vua chieu ngang.
                float needY = b.extents.y * fitMargin;
                float needX = b.extents.x * fitMargin / Mathf.Max(0.0001f, cinematicCamera.aspect);
                endSize = Mathf.Max(needY, needX);
                endPos = new Vector3(b.center.x, b.center.y, startPos.z);
            }
            else
            {
                float playerY = playerTransform != null ? playerTransform.position.y : startPos.y;
                endPos = new Vector3(levelCenterX, playerY - targetOrthoSize * framingBias, startPos.z);
            }

            yield return Move(startSize, endSize, startPos, endPos, zoomOutDuration, fadeIn: true);

            yield return RevealRowLabels();

            // Dung han o day: het Chapter 1. Khong quay ve, khong xoa phong, khong bat lai HUD —
            // man hinh giu nguyen canh nam khu vuc da di qua.
            if (stayAtEnd)
            {
                isPlaying = false;
                yield break;
            }

            yield return new WaitForSeconds(holdDuration);
            yield return Move(endSize, startSize, endPos, startPos, returnDuration, fadeIn: false);

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

            List<Sprite> source = journeySprites.FindAll(s => s != null);
            if (source.Count == 0)
                return;

            if (equalRows)
            {
                BuildEqualRows(source);
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                var go = new GameObject($"JourneyPanel_{i}", typeof(SpriteRenderer));
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.position = new Vector3(levelCenterX + panelStepX * i,
                                                   panelBottomY + panelSpacing * i, 0f);
                go.transform.localScale = Vector3.one * panelScale;

                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = source[i];
                // Tấm trên vẽ đè lên tấm dưới một chút, để chỗ chồng mép hoà dần theo hướng đi lên.
                renderer.sortingOrder = panelSortingOrder + i;
                renderer.color = new Color(1f, 1f, 1f, 0f);
                panels.Add(renderer);
            }
        }

        // N dòng CAO BẰNG NHAU, xếp kín khung hình.
        //
        // Ảnh nguồn tỉ lệ 16:9, mà một dòng trong N dòng lấp kín màn hình lại có tỉ lệ N*aspect
        // (5 dòng trên màn 16:9 = 8.9:1). Nên phải CẮT lấy một dải ngang giữa ảnh — kéo giãn cho
        // vừa sẽ bẹp dí hình. Sprite.Create chỉ khai lại vùng UV trên đúng texture đó, không sao
        // chép pixel và không cần bật Read/Write.
        private void BuildEqualRows(List<Sprite> source)
        {
            int n = source.Count;
            float rowAspect = n * Mathf.Max(0.0001f, cinematicCamera.aspect);
            float rowHeight = panelScale / rowAspect;

            for (int i = 0; i < n; i++)
            {
                Sprite cropped = CropRow(source[i], rowAspect);
                runtimeSprites.Add(cropped);

                var go = new GameObject($"JourneyRow_{i}", typeof(SpriteRenderer));
                go.transform.SetParent(transform, worldPositionStays: false);
                go.transform.position = new Vector3(levelCenterX,
                                                    panelBottomY + rowHeight * i, 0f);
                go.transform.localScale = Vector3.one;

                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = cropped;
                renderer.sortingOrder = panelSortingOrder + i;
                renderer.color = new Color(1f, 1f, 1f, 0f);
                panels.Add(renderer);
            }

            BuildRowLabels(n, rowHeight);
        }

        private Sprite CropRow(Sprite src, float rowAspect)
        {
            Rect r = src.rect;                            // vùng của sprite trong texture
            float cropH = Mathf.Min(r.width / rowAspect, r.height);
            float y = r.y + (r.height - cropH) * Mathf.Clamp01(cropBias);
            // pixelsPerUnit chọn sao cho bề rộng thế giới đúng bằng panelScale.
            float ppu = r.width / Mathf.Max(0.0001f, panelScale);
            return Sprite.Create(src.texture, new Rect(r.x, y, r.width, cropH),
                                 new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        }

        // Hộp bao TRỌN cả chồng phông, lấy từ chính SpriteRenderer nên không phải tự tính lại
        // kích thước sprite — đổi ảnh khác tỉ lệ vẫn đúng.
        private Bounds PanelBounds()
        {
            Bounds b = panels[0].bounds;
            for (int i = 1; i < panels.Count; i++)
                b.Encapsulate(panels[i].bounds);
            return b;
        }


        // ================= Nhãn từng dòng =================
        //
        // Dùng MỘT Canvas World Space cho cả 5 dòng thay vì mỗi dòng một canvas: chữ UI nét hơn
        // TextMesh, và gom một canvas thì chỉ có một lần dựng batch.
        //
        // Toạ độ bên trong canvas tính bằng "pixel ảo" (CanvasPx), rồi thu nhỏ cả canvas về đúng
        // bề rộng thế giới. Nhờ vậy bố cục không phụ thuộc panelScale.
        // Canvas World Space bi thu nho lai rat nhieu (36 unit / CanvasPx). Font duoc rasterise
        // theo fontSize TRONG canvas, nen CanvasPx nho => glyph nho => phong to len man hinh =>
        // nhoe. Dat 3000 cho glyph duoc ve o do phan giai cao roi thu nho xuong, chu se net.
        private const float CanvasPx = 3000f;

        private void BuildRowLabels(int n, float rowHeightWorld)
        {
            if (!showRowLabels || labelFont == null || n == 0)
                return;

            float rowPx = CanvasPx / Mathf.Max(0.0001f, panelScale) * rowHeightWorld;
            float totalPx = rowPx * n;

            var go = new GameObject("JourneyLabels", typeof(Canvas));
            go.transform.SetParent(transform, worldPositionStays: false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cinematicCamera;
            canvas.sortingOrder = panelSortingOrder + 10;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(CanvasPx, totalPx);
            rt.position = new Vector3(levelCenterX, panelBottomY + rowHeightWorld * (n - 1) * 0.5f, 0f);
            rt.localScale = Vector3.one * (panelScale / CanvasPx);
            labelCanvas = go;

            for (int i = 0; i < n; i++)
            {
                float cy = (i + 0.5f) * rowPx - totalPx * 0.5f;
                var row = new GameObject($"Row_{i}", typeof(RectTransform), typeof(CanvasGroup));
                var rrt = (RectTransform)row.transform;
                rrt.SetParent(rt, worldPositionStays: false);
                rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f);
                rrt.sizeDelta = new Vector2(CanvasPx, rowPx);
                rrt.anchoredPosition = new Vector2(0f, cy);

                var group = row.GetComponent<CanvasGroup>();
                group.alpha = 0f;
                rowGroups.Add(group);

                MakePlate(rrt, rowPx);
                MakeText(rrt, Name(regionNames, i), rowPx * 0.20f,
                         TextAnchor.MiddleLeft, new Vector2(-CanvasPx * 0.5f + CanvasPx * 0.034f, 0f),
                         new Vector2(CanvasPx * 0.42f, rowPx * 0.6f), new Vector2(0f, 0.5f));
                MakeText(rrt, Name(ConstellationNames(), i), rowPx * 0.16f,
                         TextAnchor.MiddleRight, new Vector2(CanvasPx * 0.5f - CanvasPx * 0.30f, 0f),
                         new Vector2(CanvasPx * 0.30f, rowPx * 0.6f), new Vector2(1f, 0.5f));
                rowShapes.Add(MakeShape(rrt, i, rowPx));
            }
        }

        private List<string> ConstellationNames()
        {
            var names = new List<string>();
            foreach (ConstellationData c in constellations)
                names.Add(c != null ? c.DisplayName : string.Empty);
            return names;
        }

        private static string Name(List<string> list, int i) =>
            list != null && i < list.Count ? list[i] : string.Empty;

        private void MakePlate(RectTransform parent, float rowPx)
        {
            var go = new GameObject("Plate", typeof(RectTransform), typeof(Image));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, worldPositionStays: false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(CanvasPx, rowPx);
            r.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = plateColor;
            img.raycastTarget = false;
        }

        private void MakeText(RectTransform parent, string content, float size, TextAnchor anchor,
                              Vector2 pos, Vector2 sizeDelta, Vector2 pivot)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Outline));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, worldPositionStays: false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = pivot;
            r.sizeDelta = sizeDelta;
            r.anchoredPosition = pos;

            var txt = go.GetComponent<Text>();
            txt.font = labelFont;
            txt.text = content;
            txt.fontSize = Mathf.Max(8, Mathf.RoundToInt(size));
            txt.alignment = anchor;
            txt.color = labelColor;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            // Viền tối: nền mỗi dòng mỗi khác (trời đêm tím tới rừng vàng chói), một màu chữ
            // không thể đọc được trên cả năm. Viền giữ chữ tách khỏi nền ở mọi dải.
            var outline = go.GetComponent<Outline>();
            outline.effectColor = labelOutlineColor;
            outline.effectDistance = new Vector2(CanvasPx * 0.0018f, -CanvasPx * 0.0018f);
        }

        // Hình chòm sao: chấm + nét nối, dựng từ Star Points (toạ độ chuẩn hoá 0..1) của asset.
        private RowShape MakeShape(RectTransform parent, int index, float rowPx)
        {
            var shape = new RowShape();
            ConstellationData data = index < constellations.Count ? constellations[index] : null;
            if (data == null || data.NodeCount == 0)
                return shape;

            float box = rowPx * 0.78f;
            var go = new GameObject("Shape", typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, worldPositionStays: false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(box * 1.6f, box);
            r.anchoredPosition = new Vector2(CanvasPx * 0.5f - box * 0.8f - CanvasPx * 0.03f, 0f);

            var pts = new List<Vector2>();
            foreach (Vector2 p in data.StarPoints)
                pts.Add(new Vector2((p.x - 0.5f) * r.sizeDelta.x, (p.y - 0.5f) * r.sizeDelta.y));

            foreach (StarConnection c in data.Connections)
            {
                if (c.fromIndex < 0 || c.fromIndex >= pts.Count ||
                    c.toIndex < 0 || c.toIndex >= pts.Count)
                    continue;
                shape.lines.Add(MakeLine(r, pts[c.fromIndex], pts[c.toIndex], rowPx * 0.035f));
            }
            foreach (Vector2 p in pts)
                shape.nodes.Add(MakeNode(r, p, rowPx * 0.16f));
            return shape;
        }

        private Image MakeNode(RectTransform parent, Vector2 pos, float size)
        {
            var go = new GameObject("Node", typeof(RectTransform), typeof(Image));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, worldPositionStays: false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = new Vector2(size, size);
            r.anchoredPosition = pos;
            r.localScale = Vector3.zero;
            var img = go.GetComponent<Image>();
            img.sprite = nodeSprite;
            img.color = nodeColor;
            img.raycastTarget = false;
            return img;
        }

        private Image MakeLine(RectTransform parent, Vector2 a, Vector2 b, float thickness)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, worldPositionStays: false);
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            Vector2 d = b - a;
            r.sizeDelta = new Vector2(d.magnitude, Mathf.Max(1f, thickness));
            r.anchoredPosition = a;
            r.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            var img = go.GetComponent<Image>();
            img.sprite = lineSprite;
            img.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            img.raycastTarget = false;
            return img;
        }

        // Hiện lần lượt TỪ DƯỚI LÊN: đúng thứ tự người chơi đã đi qua.
        private IEnumerator RevealRowLabels()
        {
            for (int i = 0; i < rowGroups.Count; i++)
            {
                StartCoroutine(RevealRow(i));
                if (rowRevealInterval > 0f)
                    yield return new WaitForSeconds(rowRevealInterval);
            }
            yield return new WaitForSeconds(labelFadeDuration + lineDrawDuration);
        }

        private IEnumerator RevealRow(int i)
        {
            CanvasGroup group = rowGroups[i];
            float t = 0f;
            while (t < labelFadeDuration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / labelFadeDuration));
                yield return null;
            }
            group.alpha = 1f;

            RowShape shape = rowShapes[i];
            foreach (Image node in shape.nodes)
            {
                if (node != null)
                    StartCoroutine(PopNode(node));
                yield return new WaitForSeconds(nodePopInterval);
            }

            float e = 0f;
            while (e < lineDrawDuration)
            {
                e += Time.deltaTime;
                float a = Mathf.SmoothStep(0f, lineColor.a, Mathf.Clamp01(e / lineDrawDuration));
                foreach (Image line in shape.lines)
                {
                    if (line != null)
                        line.color = new Color(lineColor.r, lineColor.g, lineColor.b, a);
                }
                yield return null;
            }
        }

        private static IEnumerator PopNode(Image node)
        {
            const float dur = 0.22f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                // Vọt quá 1 rồi lắng lại: chấm sao "bật" ra thay vì phình đều.
                float s = Mathf.Sin(k * Mathf.PI * 0.5f) * (1f + 0.35f * (1f - k));
                node.transform.localScale = Vector3.one * s;
                yield return null;
            }
            node.transform.localScale = Vector3.one;
        }

        private class RowShape
        {
            public readonly List<Image> nodes = new List<Image>();
            public readonly List<Image> lines = new List<Image>();
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

            foreach (Sprite s in runtimeSprites)
            {
                if (s != null)
                    Destroy(s);
            }
            runtimeSprites.Clear();

            if (labelCanvas != null)
                Destroy(labelCanvas);
            labelCanvas = null;
            rowGroups.Clear();
            rowShapes.Clear();
        }
    }
}
