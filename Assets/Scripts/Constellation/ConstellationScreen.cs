using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Level;

namespace StarSower.Constellations
{
    // Màn hình chòm sao (S1-020A). Hiện SAU khi hoàn thành một khu vực, không bao giờ trong lúc chơi.
    //
    // Toàn bộ sao và nét nối được DỰNG LÚC CHẠY từ ConstellationData: toạ độ trong asset là 0..1
    // chuẩn hoá theo màn hình, nên cùng một chòm sao tự co giãn đúng trên mọi tỉ lệ màn hình mà
    // không cần bày sẵn từng ngôi trong scene, cũng không cần sửa gì khi designer đổi hình dạng.
    //
    // MỘT LEVEL = MỘT CHÒM SAO RIÊNG (S1-020B): level thứ i mở chòm thứ i trong ChapterData, và
    // chòm đó mở TRỌN VẸN chứ không nhỏ giọt từng ngôi. Trạng thái lưu riêng cho từng chòm nên mở
    // chòm sau không bao giờ ghi đè chòm trước.
    public class ConstellationScreen : MonoBehaviour
    {
        [Header("Dữ liệu")]
        [Tooltip("Danh sách chòm sao theo ĐÚNG thứ tự level trong LevelDatabase: level thứ i mở " +
                 "chòm sao thứ i. Thiếu chòm cho level nào thì level đó đơn giản là không hiện gì.")]
        [SerializeField] private ChapterData chapter;

        [Tooltip("Dùng để biết vừa hoàn thành level nào -> tra ra chòm sao tương ứng.")]
        [SerializeField] private LevelManager levelManager;

        [SerializeField] private ProgressManager progressManager;

        [Tooltip("Tên chòm sao, hiện ở TRÊN CÙNG màn hình.")]
        [SerializeField] private Text nameLabel;

        [Tooltip("Câu mô tả ngắn dưới tên (vd \"The Harp\"). Để trống thì bỏ qua.")]
        [SerializeField] private Text descriptionLabel;

        private ConstellationData constellation;

        [Header("Khung")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Vùng chứa sao + nét nối. Toạ độ 0..1 trong ConstellationData ánh xạ vào đúng ô này, " +
                 "nên chừa lề rộng ở đây là chừa lề cho cả chòm sao.")]
        [SerializeField] private RectTransform field;
        [SerializeField] private Image background;

        [Header("Sprite")]
        [SerializeField] private Sprite starSprite;
        [SerializeField] private Sprite lineSprite;
        [SerializeField] private Sprite sparkleSprite;

        [Header("Kích thước")]
        [SerializeField] private float starSize = 110f;
        [SerializeField] private float lineThickness = 26f;

        [Header("Màu")]
        [Tooltip("Sao chưa mở: rất mờ, không phát sáng — chỉ đủ để thấy chòm sao sẽ thành hình gì.")]
        [Range(0.05f, 0.6f)]
        [SerializeField] private float lockedStarAlpha = 0.2f;
        [SerializeField] private Color unlockedStarColor = Color.white;
        [SerializeField] private Color lineColor = new Color(0.55f, 0.88f, 1f, 1f);

        [Header("Nhịp (giây)")]
        [SerializeField] private float fadeInDuration = 0.8f;
        [SerializeField] private float starPopDuration = 0.55f;
        [SerializeField] private float lineDrawDuration = 0.5f;
        [SerializeField] private float holdAfterUnlock = 1.6f;
        [SerializeField] private float fadeOutDuration = 0.7f;

        [Header("Nhấp nháy khi đã mở")]
        [SerializeField] private float pulseAmplitude = 0.06f;
        [SerializeField] private float pulsePeriod = 2.8f;

        private readonly List<Image> starImages = new List<Image>();
        private readonly List<Image> lineImages = new List<Image>();
        private readonly List<StarConnection> lineOwners = new List<StarConnection>();
        private int unlockedCount;

        // Đang chạy trình tự mở khoá hay không. Bắt buộc phải có: Update() nhấp nháy ghi thẳng vào
        // localScale của mọi sao đã mở, mà UnlockStar() lại tăng unlockedCount TRƯỚC khi phóng sao —
        // nên nhịp nhấp nháy đè chết hoạt ảnh 0.5 -> 1.2 -> 1.0 ngay từ khung hình đầu, nhìn như
        // sao hiện ra tức thì và mất sạch phần "vẽ chòm sao".
        private bool isAnimating;

        private void Awake()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            gameObject.SetActive(false);
        }

        // Gọi bởi LevelFlowManager sau khi đã lưu tiến trình khu vực.
        //
        // MỖI LEVEL MỘT CHÒM SAO RIÊNG: level thứ i trong LevelDatabase mở chòm thứ i trong
        // ChapterData. Không cộng dồn sao vào một chòm chung nữa — đó là lý do trước đây hoàn
        // thành level 2 vẫn thấy đúng chòm của level 1.
        public IEnumerator Show()
        {
            constellation = ResolveConstellationForCurrentLevel();
            if (constellation == null)
            {
                Debug.LogWarning("[Constellation] Khong tra ra chom sao cho level nay — kiem tra " +
                                 "Chapter/Level Manager va thu tu danh sach trong ChapterData.", this);
                yield break;
            }
            if (progressManager == null)
            {
                Debug.LogWarning("[Constellation] Chua gan Progress Manager.", this);
                yield break;
            }

            int totalStars = constellation.StarPoints.Count;
            if (totalStars == 0)
            {
                Debug.LogWarning($"[Constellation] '{constellation.DisplayName}' khong co Star Points nao.", this);
                yield break;
            }

            // Đã diễn rồi thì lần sau chỉ hiện chòm sao hoàn chỉnh, không diễn lại.
            bool alreadyAnimated = progressManager.IsConstellationAnimated(constellation.ConstellationId);

            // Log chẩn đoán: đây là nơi duy nhất quyết định CÓ DIỄN hay KHÔNG, nên khi hoạt ảnh
            // không chạy thì dòng này nói thẳng lý do.
            Debug.Log($"[Constellation] level='{levelManager.CurrentLevelId}' -> " +
                      $"'{constellation.DisplayName}' ({totalStars} sao) | daDien={alreadyAnimated} " +
                      $"=> {(alreadyAnimated ? "HIEN NGAY, khong dien" : "SE DIEN hoat anh")}", this);

            gameObject.SetActive(true);
            Build(totalStars);
            ApplyLabels();

            unlockedCount = alreadyAnimated ? totalStars : 0;
            Repaint(instant: true);

            yield return Fade(0f, 1f, fadeInDuration);

            if (!alreadyAnimated)
            {
                isAnimating = true;
                for (int i = 0; i < totalStars; i++)
                    yield return UnlockStar(i);
                isAnimating = false;

                // Lưu NGAY sau khi diễn xong: mỗi chòm một bản ghi riêng, không đụng chòm nào khác.
                progressManager.MarkConstellationUnlocked(constellation.ConstellationId, true);
            }

            yield return new WaitForSeconds(alreadyAnimated ? holdAfterUnlock * 0.5f : holdAfterUnlock);
            yield return Fade(1f, 0f, fadeOutDuration);
            gameObject.SetActive(false);
        }

        // Trình tự bị cắt giữa chừng (đổi scene, tắt object) thì cờ phải được nhả, nếu không lần
        // sau mở màn hình sẽ mất hẳn nhịp nhấp nháy.
        private void OnDisable()
        {
            isAnimating = false;
        }

        // Level thứ i -> chòm sao thứ i. Tra bằng CHỈ SỐ trong LevelDatabase chứ không hardcode id,
        // nên chèn thêm khu vực vào giữa danh sách là ánh xạ tự dịch theo.
        private ConstellationData ResolveConstellationForCurrentLevel()
        {
            if (chapter == null || levelManager == null || levelManager.Database == null)
                return null;

            IReadOnlyList<LevelDefinition> levels = levelManager.Database.Levels;
            int index = -1;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId == levelManager.CurrentLevelId)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0 || index >= chapter.Constellations.Count)
                return null;

            return chapter.Constellations[index];
        }

        private void ApplyLabels()
        {
            if (nameLabel != null)
                nameLabel.text = constellation.DisplayName;
            if (descriptionLabel != null)
                descriptionLabel.text = constellation.Description;
        }

        private void Build(int totalStars)
        {
            foreach (Image img in starImages)
            {
                if (img != null) Destroy(img.gameObject);
            }
            foreach (Image img in lineImages)
            {
                if (img != null) Destroy(img.gameObject);
            }
            starImages.Clear();
            lineImages.Clear();
            lineOwners.Clear();

            if (background != null && background.sprite == null)
                background.enabled = false;

            // Nét nối dựng TRƯỚC sao để sao luôn nằm đè lên đầu nét — không cần đụng sibling index.
            foreach (StarConnection c in constellation.Connections)
            {
                if (c.fromIndex < 0 || c.toIndex < 0 ||
                    c.fromIndex >= totalStars || c.toIndex >= totalStars)
                    continue;

                Image line = CreateImage("Line", lineSprite);
                line.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
                lineImages.Add(line);
                lineOwners.Add(c);
                LayoutLine(line.rectTransform, c, 0f);
            }

            for (int i = 0; i < totalStars; i++)
            {
                Image star = CreateImage($"Star_{i}", starSprite);
                star.rectTransform.anchoredPosition = ToFieldPosition(constellation.StarPoints[i]);
                star.rectTransform.sizeDelta = new Vector2(starSize, starSize);
                starImages.Add(star);
            }
        }

        private Image CreateImage(string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(field, worldPositionStays: false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        // 0..1 -> toạ độ trong field, gốc ở tâm.
        private Vector2 ToFieldPosition(Vector2 normalized)
        {
            Rect r = field.rect;
            return new Vector2((normalized.x - 0.5f) * r.width, (normalized.y - 0.5f) * r.height);
        }

        // progress 0..1: nét mọc dần từ sao này sang sao kia thay vì hiện ra nguyên vẹn.
        private void LayoutLine(RectTransform rt, StarConnection c, float progress)
        {
            Vector2 a = ToFieldPosition(constellation.StarPoints[c.fromIndex]);
            Vector2 b = ToFieldPosition(constellation.StarPoints[c.toIndex]);
            Vector2 delta = b - a;
            float full = delta.magnitude;

            rt.anchoredPosition = a + delta.normalized * (full * progress * 0.5f);
            rt.sizeDelta = new Vector2(full * progress, lineThickness);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void Repaint(bool instant)
        {
            for (int i = 0; i < starImages.Count; i++)
                PaintStar(i, i < unlockedCount ? 1f : 0f);

            for (int i = 0; i < lineImages.Count; i++)
            {
                bool on = IsConnectionUnlocked(lineOwners[i]);
                lineImages[i].color = new Color(lineColor.r, lineColor.g, lineColor.b, on ? 1f : 0f);
                LayoutLine(lineImages[i].rectTransform, lineOwners[i], on ? 1f : 0f);
            }
        }

        // Nét chỉ hiện khi CẢ HAI đầu đã mở — nếu không sẽ có nét nối tới một ngôi sao chưa tồn tại.
        private bool IsConnectionUnlocked(StarConnection c) =>
            c.fromIndex < unlockedCount && c.toIndex < unlockedCount;

        private void PaintStar(int index, float unlockT)
        {
            Color locked = new Color(unlockedStarColor.r, unlockedStarColor.g, unlockedStarColor.b, lockedStarAlpha);
            starImages[index].color = Color.Lerp(locked, unlockedStarColor, unlockT);
        }

        private IEnumerator UnlockStar(int index)
        {
            unlockedCount = index + 1;

            RectTransform rt = starImages[index].rectTransform;
            Vector3 baseScale = Vector3.one;

            // 0.5 -> 1.2 -> 1.0: nảy lên rồi lắng lại, đúng nhịp yêu cầu.
            float elapsed = 0f;
            while (elapsed < starPopDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / starPopDuration);
                float s = t < 0.55f
                    ? Mathf.Lerp(0.5f, 1.2f, Mathf.SmoothStep(0f, 1f, t / 0.55f))
                    : Mathf.Lerp(1.2f, 1f, Mathf.SmoothStep(0f, 1f, (t - 0.55f) / 0.45f));

                rt.localScale = baseScale * s;
                PaintStar(index, t);
                yield return null;
            }
            rt.localScale = baseScale;
            PaintStar(index, 1f);

            SpawnSparkle(rt.anchoredPosition);

            // Vẽ những nét vừa đủ điều kiện nhờ ngôi sao này.
            for (int i = 0; i < lineImages.Count; i++)
            {
                if (!IsConnectionUnlocked(lineOwners[i]))
                    continue;
                if (lineImages[i].color.a > 0.99f)
                    continue;

                yield return DrawLine(i);
            }
        }

        private IEnumerator DrawLine(int index)
        {
            Image line = lineImages[index];
            StarConnection c = lineOwners[index];
            float elapsed = 0f;
            while (elapsed < lineDrawDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / lineDrawDuration));
                LayoutLine(line.rectTransform, c, t);
                line.color = new Color(lineColor.r, lineColor.g, lineColor.b, t);
                yield return null;
            }
            LayoutLine(line.rectTransform, c, 1f);
            line.color = lineColor;
        }

        // Một hạt sparkle duy nhất mỗi lần mở sao, tự huỷ. Không dùng ParticleSystem: trên mobile
        // một sprite phình to rồi tan là đủ, mà rẻ hơn hẳn.
        private void SpawnSparkle(Vector2 position)
        {
            if (sparkleSprite == null)
                return;

            Image sparkle = CreateImage("Sparkle", sparkleSprite);
            sparkle.rectTransform.anchoredPosition = position;
            sparkle.rectTransform.sizeDelta = new Vector2(starSize * 1.6f, starSize * 1.6f);
            StartCoroutine(SparkleRoutine(sparkle));
        }

        private IEnumerator SparkleRoutine(Image sparkle)
        {
            const float life = 0.7f;
            float elapsed = 0f;
            while (elapsed < life && sparkle != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / life);
                sparkle.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.8f, t);
                sparkle.color = new Color(1f, 1f, 1f, 1f - t);
                yield return null;
            }
            if (sparkle != null)
                Destroy(sparkle.gameObject);
        }

        // Chòm sao đã mở thở nhẹ trong lúc màn hình còn hiện.
        private void Update()
        {
            // Nhường hoàn toàn quyền ghi localScale cho trình tự mở khoá trong lúc nó đang chạy.
            if (isAnimating || unlockedCount <= 0 || pulseAmplitude <= 0f)
                return;

            float s = 1f + Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / Mathf.Max(0.1f, pulsePeriod)) * pulseAmplitude;
            for (int i = 0; i < unlockedCount && i < starImages.Count; i++)
            {
                if (starImages[i] != null)
                    starImages[i].rectTransform.localScale = Vector3.one * s;
            }
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (canvasGroup == null || duration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
