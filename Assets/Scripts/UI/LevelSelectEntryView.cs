using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Level;

namespace StarSower.UI
{
    // Mot the level trong danh sach chon man (S1-009, mo rong o S2-014).
    //
    // View THUAN: nhan du lieu da tinh san roi hien ra, khong tu hoi ProgressManager hay
    // LevelDatabase. Nho vay khong co the nao tu quyet dinh "mo khoa" khac voi cac the con lai —
    // luat mo khoa nam duy nhat o LevelSelectController.
    public class LevelSelectEntryView : MonoBehaviour
    {
        [Header("Chu")]
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text statusLabel;

        [Header("Anh nen cua khu vuc")]
        [Tooltip("Anh chu dao cua level, lay tu lop nen dau tien cua RegionData. De trong thi the " +
                 "chi co mau phang nhu truoc.")]
        [SerializeField] private Image backgroundImage;

        [Header("O sao")]
        [Tooltip("Khung chua cac o sao. So o duoc dung LUC CHAY, dung bang so manh sao cua man do " +
                 "(5 o Forgotten Forest, 7 o Cloud Garden...). Dat san 3 o trong prefab la sai, vi " +
                 "moi man mot so khac nhau.")]
        [SerializeField] private RectTransform starContainer;

        [SerializeField] private Sprite starSprite;
        [SerializeField] private float starSize = 34f;
        [SerializeField] private float starSpacing = 6f;

        [Tooltip("Qua nhieu o thi thu nho lai cho vua be ngang nay (pixel).")]
        [SerializeField] private float starRowMaxWidth = 300f;

        [SerializeField] private Image lockIcon;
        [SerializeField] private Button selectButton;

        [Header("Mau o sao")]
        [SerializeField] private Color starFilled = new Color(1f, 0.88f, 0.55f, 1f);
        [SerializeField] private Color starEmpty = new Color(0.62f, 0.66f, 0.72f, 0.3f);

        [Header("Hoat anh")]
        [Min(0.01f)]
        [SerializeField] private float tapDuration = 0.15f;
        [SerializeField] private float tapScale = 1.05f;

        [Tooltip("Bien do lac ngang khi cham vao the dang KHOA (pixel).")]
        [SerializeField] private float lockedShake = 14f;

        private Coroutine routine;

        private Sprite backgroundSprite;

        private readonly List<Image> stars = new List<Image>();

        public void Setup(LevelDefinition level, bool unlocked, int collectedStars,
                          int totalStars, Sprite background, Action<LevelDefinition> onSelected)
        {
            nameLabel.text = level.displayName;
            SetBackground(background);

            // Nut LUON bam duoc, ke ca khi khoa: the khoa phai phan hoi lai cu cham thi nguoi choi
            // moi biet minh da bam trung. Tat interactable di thi cham vao khong co gi xay ra, doc
            // ra giong nhu giao dien bi dung.
            selectButton.interactable = true;
            ApplyStars(unlocked, collectedStars, totalStars);

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(!unlocked);

            if (statusLabel != null)
                statusLabel.text = unlocked ? string.Empty : "Locked";

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                Play(unlocked ? TapRoutine() : LockedRoutine());
                if (unlocked)
                    onSelected(level);
            });
        }

        // Anh khu vuc la anh DOC (cao hon rong nhieu), con the level thi ngang va det. Keo cho vua
        // khung the se bop meo anh, nen thay vao do: keo day BE NGANG the roi de phan thua tran ra
        // ngoai theo chieu doc, va RectMask2D tren the cat bot — ket qua la mot dai ngang lay tu
        // giua buc tranh, khong meo.
        public void SetBackground(Sprite sprite)
        {
            backgroundSprite = sprite;

            if (backgroundImage == null)
                return;

            backgroundImage.gameObject.SetActive(sprite != null);
            if (sprite == null)
                return;

            backgroundImage.sprite = sprite;
            ResizeBackground();
        }

        // Unity goi ham nay moi khi kich thuoc RectTransform doi. Dung no thay vi tinh trong Setup()
        // vi luc Setup chay thi VerticalLayoutGroup CHUA gan be ngang cho the — doc rect.width luc
        // do ra 0 va anh se cao bang 0.
        private void OnRectTransformDimensionsChange()
        {
            ResizeBackground();
        }

        private void ResizeBackground()
        {
            if (backgroundImage == null || backgroundSprite == null)
                return;

            float width = ((RectTransform)transform).rect.width;
            if (width <= 0f)
                return;

            float ratio = backgroundSprite.rect.height / Mathf.Max(backgroundSprite.rect.width, 1f);
            backgroundImage.rectTransform.sizeDelta = new Vector2(0f, width * ratio);
        }

        // Dung dung `total` o sao roi to mau `collected` o dau tien. Man khoa thi an het — chua vao
        // duoc thi khoe so sao cua no cung khong noi len dieu gi.
        private void ApplyStars(bool unlocked, int collected, int total)
        {
            if (starContainer == null)
                return;

            starContainer.gameObject.SetActive(unlocked && total > 0);
            if (!unlocked || total <= 0)
                return;

            while (stars.Count < total)
            {
                var go = new GameObject("star_" + (stars.Count + 1).ToString("00"), typeof(RectTransform));
                go.transform.SetParent(starContainer, worldPositionStays: false);

                var img = go.AddComponent<Image>();
                img.sprite = starSprite;
                img.raycastTarget = false;
                img.preserveAspect = true;

                var r = (RectTransform)go.transform;
                r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
                stars.Add(img);
            }

            for (int i = 0; i < stars.Count; i++)
                stars[i].gameObject.SetActive(i < total);

            // Thu nho o sao khi man co nhieu sao, de hang khong tran ra khoi the.
            float size = starSize;
            float step = size + starSpacing;
            if (total * step > starRowMaxWidth && total > 0)
            {
                step = starRowMaxWidth / total;
                size = Mathf.Max(8f, step - starSpacing);
            }

            float start = -(total - 1) * step * 0.5f;
            for (int i = 0; i < total; i++)
            {
                stars[i].rectTransform.sizeDelta = new Vector2(size, size);
                stars[i].rectTransform.anchoredPosition = new Vector2(start + i * step, 0f);
                stars[i].color = i < collected ? starFilled : starEmpty;
            }
        }

        private void Play(IEnumerator next)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(next);
        }

        // 1 -> 1.05 -> 1. Dung sin(pi*k) nen dinh nam giua roi tra ve dung 1, khong bao gio ket o
        // co lon neu coroutine bi cat giua chung.
        private IEnumerator TapRoutine()
        {
            Transform tr = transform;
            float t = 0f;
            while (t < tapDuration)
            {
                t += Time.unscaledDeltaTime;
                float s = 1f + Mathf.Sin(Mathf.Clamp01(t / tapDuration) * Mathf.PI) * (tapScale - 1f);
                tr.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            tr.localScale = Vector3.one;
            routine = null;
        }

        // The dang khoa: lac ngang tat dan. Khong doi mau, khong keu — chi du de noi "chua vao duoc".
        private IEnumerator LockedRoutine()
        {
            var rect = (RectTransform)transform;
            Vector2 basePos = rect.anchoredPosition;
            float t = 0f;
            while (t < tapDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / tapDuration);
                float offset = Mathf.Sin(k * Mathf.PI * 3f) * lockedShake * (1f - k);
                rect.anchoredPosition = basePos + new Vector2(offset, 0f);
                yield return null;
            }
            rect.anchoredPosition = basePos;
            routine = null;
        }
    }
}
