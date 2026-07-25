using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarSower.Constellations
{
    // Trình diễn PLACEHOLDER cho khoảnh khắc khôi phục chòm sao: màn hình tối lại (bầu trời hiện ra)
    // -> từng ngôi sao sáng lên -> các nét nối xuất hiện -> giữ vài giây -> tan dần. Không popup,
    // không bảng điểm, không nút bấm — người chơi chỉ nhìn.
    //
    // Hình dạng chòm sao được DỰNG TỪ DỮ LIỆU (ConstellationData.StarPoints/Connections) chứ không
    // vẽ tay trong scene, nên thêm chòm sao mới không cần đụng scene. Toàn bộ lớp hiển thị được tạo
    // bằng code vì giai đoạn này chưa có art thật; khi có art, viết một component khác implement
    // IConstellationRestoreSequence rồi gán vào ô Restore Sequence Source của ConstellationManager
    // là thay được, không sửa dòng code điều phối nào.
    public class ConstellationRestoreSequence : MonoBehaviour, IConstellationRestoreSequence
    {
        [Header("Timing (tỉ lệ của Animation Duration khai báo trong ConstellationData)")]
        [Tooltip("Phần thời lượng dành cho bầu trời tối dần.")]
        [SerializeField] private float skyFadeInWeight = 0.2f;
        [Tooltip("Phần thời lượng dành cho các ngôi sao sáng lên lần lượt.")]
        [SerializeField] private float starsWeight = 0.3f;
        [Tooltip("Phần thời lượng dành cho các nét nối hiện ra.")]
        [SerializeField] private float linesWeight = 0.2f;

        [Header("Bố cục")]
        [Tooltip("Phần chiều cao màn hình chừa trống ở TRÊN cho thẻ tên chòm sao. 0 = hình vẽ tràn " +
                 "toàn màn hình như cũ (chữ sẽ đè lên hình). 0.18 = chừa 18% phía trên.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float shapeTopMargin = 0.18f;

        [Header("Look (placeholder)")]
        [SerializeField] private Color skyColor = new Color(0.02f, 0.03f, 0.10f, 0.92f);
        [SerializeField] private Color starColor = new Color(1f, 0.97f, 0.85f, 1f);
        [SerializeField] private Color lineColor = new Color(0.65f, 0.80f, 1f, 0.6f);
        [SerializeField] private float starSize = 14f;
        [SerializeField] private float lineThickness = 2f;
        [Tooltip("Sortion order của canvas trình diễn — phải cao hơn HUD nhưng thấp hơn lớp che chuyển cảnh.")]
        [SerializeField] private int sortingOrder = 200;

        private CanvasGroup rootGroup;
        private RectTransform shapeContainer;
        private readonly List<Graphic> spawnedGraphics = new List<Graphic>();

        // Độ hoành tráng của chòm sao đang trình diễn — mốc sau đặt Effect Scale lớn hơn thì sao to
        // hơn và nét dày hơn, không cần thêm component hay code mới.
        private float currentEffectScale = 1f;

        // Animation Duration của mỗi chòm sao giờ là thời gian VẼ, không gồm lúc giữ và lúc tan —
        // hai chặng đó do ConstellationManager giữ nhịp chung cho cả chòm sao lẫn thẻ tên.
        private float WeightSum => Mathf.Max(0.0001f, skyFadeInWeight + starsWeight + linesWeight);

        public IEnumerator Reveal(ConstellationData constellation)
        {
            if (constellation == null)
                yield break;

            Debug.Log($"[Constellation] Bat dau trinh dien khoi phuc: {constellation.DisplayName} " +
                      $"({constellation.StarPoints.Count} sao, {constellation.Connections.Count} net noi)", this);

            EnsureUIBuilt();

            rootGroup.alpha = 0f;
            rootGroup.gameObject.SetActive(true);

            // Bật canvas + ép layout chạy TRƯỚC khi dựng hình, vì độ dài nét nối tính theo kích
            // thước pixel thật của container — lúc canvas còn tắt kích thước đó vẫn là 0.
            Canvas.ForceUpdateCanvases();
            BuildShape(constellation);

            // Mỗi chòm sao tự khai báo tổng thời lượng; các chặng chia theo tỉ lệ nên mốc sau đặt
            // Animation Duration dài hơn là tự động "đẹp hơn" mà không phải chỉnh từng con số.
            float total = constellation.AnimationDuration;
            float sum = WeightSum;

            yield return FadeGroup(0f, 1f, total * skyFadeInWeight / sum);
            yield return RevealStars(constellation, total * starsWeight / sum);
            yield return RevealLines(constellation, total * linesWeight / sum);

            // Dừng ở đây với chòm sao còn nguyên trên màn hình. Việc giữ bao lâu rồi tan lúc nào là
            // của ConstellationManager, vì thẻ tên phải tan cùng nhịp.
        }

        public IEnumerator Dismiss(float duration)
        {
            if (rootGroup == null)
                yield break;

            yield return FadeGroup(1f, 0f, duration);

            rootGroup.gameObject.SetActive(false);
            ClearShape();
        }

        // Dựng lớp hiển thị 1 lần rồi dùng lại — tránh tạo/huỷ Canvas mỗi lần khôi phục.
        private void EnsureUIBuilt()
        {
            if (rootGroup != null)
                return;

            var rootObject = new GameObject("ConstellationRestoreCanvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            rootObject.transform.SetParent(transform, worldPositionStays: false);

            var canvas = rootObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = rootObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            rootGroup = rootObject.GetComponent<CanvasGroup>();
            rootGroup.blocksRaycasts = false;
            rootGroup.interactable = false;

            CreateFullScreenChild(rootObject.transform, "Sky", skyColor);
            shapeContainer = CreateFullScreenChild(rootObject.transform, "Shape", Color.clear).rectTransform;

            rootObject.SetActive(false);
        }

        private Image CreateFullScreenChild(Transform parent, string childName, Color color)
        {
            var child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, worldPositionStays: false);

            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = child.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void BuildShape(ConstellationData constellation)
        {
            ClearShape();
            currentEffectScale = constellation.EffectScale;

            foreach (Vector2 point in constellation.StarPoints)
                spawnedGraphics.Add(CreateStar(MapPoint(point)));

            Vector2 containerSize = shapeContainer.rect.size;
            foreach (StarConnection connection in constellation.Connections)
            {
                if (!IsValidConnection(constellation, connection))
                    continue;

                spawnedGraphics.Add(CreateLine(
                    MapPoint(constellation.StarPoints[connection.fromIndex]),
                    MapPoint(constellation.StarPoints[connection.toIndex]),
                    containerSize));
            }
        }

        // Ép hình chòm sao xuống dưới dải tiêu đề, để thẻ tên (ConstellationNameCard) nằm gọn phía
        // trên mà không đè lên hình. Nhân toạ độ y chuẩn hoá với (1 - shapeTopMargin): y = 1 là đỉnh
        // màn hình, nên nhân nhỏ lại tức là đẩy mọi ngôi sao XUỐNG.
        //
        // Làm ở đây thay vì sửa toạ độ trong từng ConstellationData: dữ liệu hình dạng do designer
        // vẽ (0..1 toàn màn hình) giữ nguyên ý nghĩa, còn việc "chừa chỗ cho chữ" là quyết định bố
        // cục của lớp trình diễn. Thêm chòm sao mới không phải tự nhớ trừ hao phần đầu màn hình.
        private Vector2 MapPoint(Vector2 normalizedPoint)
        {
            float usableHeight = Mathf.Clamp01(1f - shapeTopMargin);
            return new Vector2(normalizedPoint.x, normalizedPoint.y * usableHeight);
        }

        private bool IsValidConnection(ConstellationData constellation, StarConnection connection)
        {
            int count = constellation.StarPoints.Count;
            return connection.fromIndex >= 0 && connection.fromIndex < count
                && connection.toIndex >= 0 && connection.toIndex < count
                && connection.fromIndex != connection.toIndex;
        }

        private Image CreateStar(Vector2 normalizedPoint)
        {
            var star = new GameObject("Star", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            star.transform.SetParent(shapeContainer, worldPositionStays: false);

            var rect = star.GetComponent<RectTransform>();
            rect.anchorMin = normalizedPoint;
            rect.anchorMax = normalizedPoint;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            float scaledStarSize = starSize * currentEffectScale;
            rect.sizeDelta = new Vector2(scaledStarSize, scaledStarSize);

            var image = star.GetComponent<Image>();
            image.color = new Color(starColor.r, starColor.g, starColor.b, 0f);
            image.raycastTarget = false;
            return image;
        }

        private Image CreateLine(Vector2 from, Vector2 to, Vector2 containerSize)
        {
            var line = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            line.transform.SetParent(shapeContainer, worldPositionStays: false);

            // Nét nối neo tại ngôi sao đầu, pivot ở đầu trái để xoay quanh chính điểm đó — độ dài
            // tính theo pixel thật của container nên chòm sao giữ đúng hình ở mọi tỉ lệ màn hình.
            Vector2 delta = new Vector2((to.x - from.x) * containerSize.x, (to.y - from.y) * containerSize.y);

            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = from;
            rect.anchorMax = from;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(delta.magnitude, lineThickness * currentEffectScale);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            var image = line.GetComponent<Image>();
            image.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
            image.raycastTarget = false;
            line.transform.SetAsFirstSibling();
            return image;
        }

        private IEnumerator RevealStars(ConstellationData constellation, float duration)
        {
            int starCount = Mathf.Min(constellation.StarPoints.Count, spawnedGraphics.Count);
            if (starCount == 0)
                yield break;

            float perStar = duration / starCount;
            for (int i = 0; i < starCount; i++)
            {
                StartCoroutine(FadeGraphic(spawnedGraphics[i], starColor.a, perStar));
                yield return new WaitForSeconds(perStar);
            }
        }

        private IEnumerator RevealLines(ConstellationData constellation, float duration)
        {
            int starCount = constellation.StarPoints.Count;
            int lineCount = spawnedGraphics.Count - starCount;
            if (lineCount <= 0)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float perLine = duration / lineCount;
            for (int i = starCount; i < spawnedGraphics.Count; i++)
            {
                StartCoroutine(FadeGraphic(spawnedGraphics[i], lineColor.a, perLine));
                yield return new WaitForSeconds(perLine);
            }
        }

        private IEnumerator FadeGraphic(Graphic graphic, float targetAlpha, float duration)
        {
            Color color = graphic.color;
            float startAlpha = color.a;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration));
                graphic.color = color;
                yield return null;
            }
            color.a = targetAlpha;
            graphic.color = color;
        }

        private IEnumerator FadeGroup(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rootGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            rootGroup.alpha = to;
        }

        private void ClearShape()
        {
            foreach (Graphic graphic in spawnedGraphics)
            {
                if (graphic != null)
                    Destroy(graphic.gameObject);
            }
            spawnedGraphics.Clear();
        }
    }
}
