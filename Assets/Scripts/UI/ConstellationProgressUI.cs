using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Collectibles;
using StarSower.Constellations;
using StarSower.FX;
using StarSower.Level;

namespace StarSower.UI
{
    // Widget tien do chom sao hien TRONG luc choi (S2-010). KHONG phai man hinh khoi phuc chom sao —
    // cai do la ConstellationCinematic, chay sau khi qua man.
    //
    // Dung so lieu tu CollectibleManager (nguon duy nhat cua so sao) va ten tu ConstellationLookup.
    // Khong tu dem lai gi ca: dem lai la co hai nguon cho cung mot con so, va som muon gi cung lech.
    //
    // Cac o sao duoc DUNG LUC CHAY, mot o cho moi node cua chom sao. Khong dat san trong scene vi
    // moi khu vuc co so node khac nhau (5 / 7 / 6 / 7 / 5), dat san la phai sua tay 5 scene moi lan
    // designer them bot mot ngoi.
    public class ConstellationProgressUI : MonoBehaviour
    {
        [Header("Nguon du lieu")]
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private ChapterData chapter;
        [SerializeField] private LevelManager levelManager;

        [Header("Tham chieu UI")]
        [SerializeField] private Text nameLabel;
        [SerializeField] private RectTransform starContainer;
        [SerializeField] private Text progressLabel;

        [Header("O sao")]
        [Tooltip("Anh dung cho moi o sao.")]
        [SerializeField] private Sprite starSprite;

        [Tooltip("Be ngang/doc mot o sao (pixel).")]
        [SerializeField] private float starSize = 44f;

        [Tooltip("Khoang cach giua hai o sao (pixel), khi khong phai thu nho.")]
        [SerializeField] private float starSpacing = 12f;

        [Tooltip("Be ngang toi da cua ca hang sao. De 0 thi lay be ngang cua Star Container. " +
                 "Chom sao lon (Draco 14, Orion 18) se tran ra khoi bang neu khong co gioi han nay.")]
        [SerializeField] private float starRowMaxWidth;

        [Header("Mau")]
        [Tooltip("Ngoi CHUA nhat: xam nhat, mo. Khong dung shader chuyen xam — chi ha bao hoa va " +
                 "alpha, du de doc la 'chua co' ma khong phai them material rieng cho UI.")]
        [SerializeField] private Color lockedColor = new Color(0.62f, 0.66f, 0.72f, 0.32f);

        [Tooltip("Ngoi DA nhat: vang am.")]
        [SerializeField] private Color unlockedColor = new Color(1f, 0.88f, 0.55f, 1f);

        [Header("Hoat anh luc nhat")]
        [Min(0.01f)]
        [SerializeField] private float popDuration = 0.2f;
        [SerializeField] private float popScale = 1.25f;

        [Header("Hat (tuy chon)")]
        [Tooltip("De trong thi tu tim. Khong co pool thi widget van chay, chi thieu hat.")]
        [SerializeField] private StarFXPool pool;
        [SerializeField] private int sparkleBurst = 3;
        [SerializeField] private int dustBurst = 4;

        private readonly List<Image> stars = new List<Image>();
        private int lastCollected;
        private int popIndex = -1;
        private float popTimer;

        private void Awake()
        {
            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();
            if (pool == null)
                pool = FindFirstObjectByType<StarFXPool>();
        }

        private void OnEnable()
        {
            if (collectibleManager != null)
                collectibleManager.OnCollectedChanged += HandleCollectedChanged;
        }

        private void OnDisable()
        {
            if (collectibleManager != null)
                collectibleManager.OnCollectedChanged -= HandleCollectedChanged;
        }

        private void Start()
        {
            ConstellationData data = ConstellationLookup.ForLevel(chapter, levelManager);
            if (nameLabel != null)
                nameLabel.text = data != null ? data.DisplayName : string.Empty;
        }

        // CollectibleManager ban su kien nay ca luc khoi tao (0/N) lan moi lan nhat, nen khong can
        // biet thu tu Start() giua hai script — lan ban dau tien da dung so o sao roi.
        private void HandleCollectedChanged(int collected, int total)
        {
            EnsureStarCount(total);

            for (int i = 0; i < stars.Count; i++)
                stars[i].color = i < collected ? unlockedColor : lockedColor;

            if (progressLabel != null)
                progressLabel.text = collected + " / " + total;

            // Chi bung o VUA sang len, khong bung lai ca hang: bung ca hang moi lan nhat se thanh
            // mot manh UI nhay lien tuc, doc kho hon chu khong ro hon.
            if (collected > lastCollected && collected - 1 < stars.Count)
            {
                popIndex = collected - 1;
                popTimer = 0f;
                SpawnBurst(stars[popIndex].rectTransform);
            }

            lastCollected = collected;
        }

        private void Update()
        {
            if (popIndex < 0 || popIndex >= stars.Count)
                return;

            popTimer += Time.deltaTime;
            float k = Mathf.Clamp01(popTimer / popDuration);

            // Len roi ve: sin(pi*k) dat dinh o giua roi tra ve 1, nen khong bao gio ket o co lon.
            float scale = 1f + Mathf.Sin(k * Mathf.PI) * (popScale - 1f);
            stars[popIndex].rectTransform.localScale = new Vector3(scale, scale, 1f);

            if (k < 1f)
                return;

            stars[popIndex].rectTransform.localScale = Vector3.one;
            popIndex = -1;
        }

        // Dung du so o sao roi xep hang ngang, can giua. Goi lai duoc nhieu lan: chi tao them phan
        // thieu, phan thua thi tat — de neu so node doi giua chung cung khong sinh rac.
        private void EnsureStarCount(int count)
        {
            if (starContainer == null)
                return;

            while (stars.Count < count)
            {
                var go = new GameObject("star_" + (stars.Count + 1).ToString("00"), typeof(RectTransform));
                go.transform.SetParent(starContainer, worldPositionStays: false);

                var img = go.AddComponent<Image>();
                img.sprite = starSprite;
                img.raycastTarget = false;
                img.preserveAspect = true;

                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                stars.Add(img);
            }

            for (int i = 0; i < stars.Count; i++)
                stars[i].gameObject.SetActive(i < count);

            // Thu nho ca o sao LAN khoang cach khi chom sao dong sao. Giu ti le khoang cach / o
            // sao khong doi (0.25) nen hang van deu mat, chu khong bi dinh vao nhau.
            float limit = starRowMaxWidth > 0f ? starRowMaxWidth : starContainer.rect.width;
            float size = starSize;
            float spacing = starSpacing;

            if (limit > 0f && count > 0)
            {
                float needed = count * starSize + (count - 1) * starSpacing;
                if (needed > limit)
                {
                    size = limit / (count + 0.25f * (count - 1));
                    spacing = size * 0.25f;
                }
            }

            float step = size + spacing;
            float start = -(count - 1) * step * 0.5f;
            for (int i = 0; i < count; i++)
            {
                stars[i].rectTransform.sizeDelta = new Vector2(size, size);
                stars[i].rectTransform.anchoredPosition = new Vector2(start + i * step, 0f);
            }
        }

        // Hat nha ra o dung cho o sao vua sang. Canvas la Screen Space Overlay nen rect.position da
        // la toa do MAN HINH — quy nguoc ve world de pool (von chay trong khong gian the gioi) nha
        // hat dung cho. Thieu camera hoac pool thi bo qua, khong bao gio chan luong choi.
        private void SpawnBurst(RectTransform rect)
        {
            Camera cam = Camera.main;
            if (pool == null || cam == null)
                return;

            Vector3 screen = rect.position;
            screen.z = -cam.transform.position.z;
            Vector3 world = cam.ScreenToWorldPoint(screen);
            world.z = 0f;

            for (int i = 0; i < sparkleBurst; i++)
            {
                Vector3 p = world + (Vector3)(Random.insideUnitCircle * 0.18f);
                pool.Spawn((StarFXType)((int)StarFXType.Sparkle01 + Random.Range(0, 3)),
                           p, Random.Range(0f, 360f), Random.Range(0.5f, 0.8f));
            }

            for (int i = 0; i < dustBurst; i++)
            {
                Vector3 p = world + (Vector3)(Random.insideUnitCircle * 0.22f);
                pool.Spawn((StarFXType)((int)StarFXType.Dust01 + Random.Range(0, 3)),
                           p, Random.Range(0f, 360f), Random.Range(0.4f, 0.7f),
                           Random.Range(0.25f, 0.45f), 0f, Random.insideUnitCircle * 0.5f);
            }
        }
    }
}
