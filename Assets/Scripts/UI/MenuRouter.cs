using System.Collections;
using UnityEngine;

namespace StarSower.UI
{
    // Chuyen qua lai giua cac man hinh menu trong CUNG mot scene (S2-014):
    // Main Menu -> Chapter Select -> Level Select.
    //
    // Mot scene chua ca ba thay vi ba scene rieng: ba man nay khong co gi nang, ma tach ra thi moi
    // lan bam Back lai phai nap scene — vua khung vua mat trang thai cuon danh sach.
    //
    // Router chi bat/tat panel. No KHONG biet ben trong moi panel co gi, cung khong dung toi tien
    // trinh hay du lieu — nen them mot man moi ve sau chi la them mot o vao mang.
    public class MenuRouter : MonoBehaviour
    {
        [Tooltip("Thu tu trong mang chinh la thu tu dieu huong. Phan tu 0 la man mo dau.")]
        [SerializeField] private CanvasGroup[] screens = new CanvasGroup[0];

        [Min(0.01f)]
        [SerializeField] private float fadeDuration = 0.18f;

        private int current = -1;
        private Coroutine routine;

        // Man hinh se mo khi scene menu VUA duoc nap. Phai la static vi no duoc dat o scene KHAC
        // (nguoi choi bam Quit trong man choi) roi doc o day sau khi LoadScene — moi thu khong
        // static deu bi huy cung scene cu.
        //
        // Day KHONG phai Singleton: khong co doi tuong nao song sot, chi la mot con so va no duoc
        // xoa ngay sau khi doc. Nho vay mo game tu dau van vao Main Menu nhu binh thuong.
        private static int requestedScreen = -1;

        public static void RequestScreen(int index) => requestedScreen = index;
        public static void RequestChapterSelect() => RequestScreen(1);
        public static void RequestLevelSelect() => RequestScreen(2);

        private void Start()
        {
            for (int i = 0; i < screens.Length; i++)
                Apply(screens[i], false);

            int start = requestedScreen >= 0 ? requestedScreen : 0;
            requestedScreen = -1;
            Show(start);
        }

        public void Show(int index)
        {
            if (index < 0 || index >= screens.Length || index == current)
                return;

            if (current >= 0)
                Apply(screens[current], false);

            current = index;
            Apply(screens[current], true);

            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(FadeIn(screens[current]));
        }

        // Cac ham khong tham so de noi thang vao Button.onClick trong Inspector — UnityEvent chi
        // goi duoc ham 0 hoac 1 tham so kieu co ban, ma dat int trong Inspector thi de go nham.
        public void ShowMainMenu() => Show(0);
        public void ShowChapterSelect() => Show(1);
        public void ShowLevelSelect() => Show(2);

        private static void Apply(CanvasGroup g, bool visible)
        {
            if (g == null)
                return;

            g.alpha = visible ? 1f : 0f;
            g.interactable = visible;
            g.blocksRaycasts = visible;
            g.gameObject.SetActive(visible);
        }

        // unscaledDeltaTime: man menu co the mo ra khi timeScale dang bang 0 (thoat tu bang tam
        // dung), luc do deltaTime bang 0 va vong lap se khong bao gio ket thuc.
        private IEnumerator FadeIn(CanvasGroup g)
        {
            if (g == null)
                yield break;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                g.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            g.alpha = 1f;
            routine = null;
        }
    }
}
