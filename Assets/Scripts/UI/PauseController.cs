using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Core;
using StarSower.Level;
using StarSower.Player;

namespace StarSower.UI
{
    // He tam dung (S2-013). Mo bang nut goc tren-phai, phim Escape, hoac khi app mat tieu diem.
    //
    // Dung Time.timeScale = 0 lam co che chinh: vat ly, Animator va moi thu chay theo Time.deltaTime
    // deu dung theo — khong phai di tat tung he thong mot. Hai thu KHONG dung theo va phai lo tay:
    //   - Input cham: joystick van nhan su kien vi UI khong phu thuoc timeScale -> tat ca cum
    //     MobileInput di, viec do cung ep joystick nha ngon tay dang giu.
    //   - Chinh hoat anh cua bang tam dung: neu dung Time.deltaTime thi no dung luon vi timeScale
    //     bang 0, nen phai dung unscaledDeltaTime.
    public class PauseController : MonoBehaviour
    {
        [Header("Bang")]
        [SerializeField] private CanvasGroup pausePanel;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private GameObject pauseButton;

        [Header("Doi tuong bi dung")]
        [Tooltip("Cum joystick + nut nhay. Tat di la joystick tu nha ngon tay dang giu, khong bi " +
                 "ket huong di sau khi choi tiep.")]
        [SerializeField] private GameObject mobileInputRoot;

        [Tooltip("De trong thi tu tim. Khoa di chuyen phong truong hop co he nao doc input theo " +
                 "thoi gian khong theo timeScale.")]
        [SerializeField] private PlayerController playerController;

        [SerializeField] private LevelManager levelManager;

        [Header("Hoat anh")]
        [Min(0.01f)]
        [SerializeField] private float openDuration = 0.2f;
        [SerializeField] private float openStartScale = 0.9f;

        [Header("Hanh vi")]
        [Tooltip("Tu tam dung khi app mat tieu diem (cuoc goi den, chuyen app).")]
        [SerializeField] private bool pauseOnFocusLost = true;

        public bool IsPaused { get; private set; }

        // Sau khi cham Astral Gate thi khong cho tam dung nua: tu do tro di la doan phim cong,
        // man chom sao va chuyen scene — dung timeScale giua chung se treo ca trinh tu o mot khung
        // hinh khong bao gio chay tiep.
        private bool canPause = true;
        private Coroutine openRoutine;

        private void Awake()
        {
            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();
            if (levelManager == null)
                levelManager = FindFirstObjectByType<LevelManager>();

            ApplyPanel(pausePanel, false);
            ApplyPanel(settingsPanel, false);
        }

        private void OnEnable()
        {
            GameEvents.OnGoalReached += HandleGoalReached;
        }

        private void OnDisable()
        {
            GameEvents.OnGoalReached -= HandleGoalReached;

            // Scene bi huy trong luc dang tam dung (bam Restart/Quit) thi timeScale phai tra ve 1,
            // neu khong scene moi mo ra dung hinh va khong ai biet tai sao.
            if (IsPaused)
                Time.timeScale = 1f;
        }

        private void HandleGoalReached() => canPause = false;

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (settingsPanel != null && settingsPanel.gameObject.activeSelf)
                CloseSettings();
            else if (IsPaused)
                Resume();
            else
                Pause();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!pauseOnFocusLost || hasFocus || IsPaused)
                return;

            Pause();
        }

        // ---- nut bam ----

        public void Pause()
        {
            if (IsPaused || !canPause)
                return;

            IsPaused = true;
            Time.timeScale = 0f;

            if (mobileInputRoot != null)
                mobileInputRoot.SetActive(false);
            playerController?.SetMovementLocked(true);
            if (pauseButton != null)
                pauseButton.SetActive(false);

            ApplyPanel(pausePanel, true);
            if (openRoutine != null)
                StopCoroutine(openRoutine);
            openRoutine = StartCoroutine(OpenAnimation(pausePanel));
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            Time.timeScale = 1f;

            ApplyPanel(pausePanel, false);
            ApplyPanel(settingsPanel, false);

            if (mobileInputRoot != null)
                mobileInputRoot.SetActive(true);
            playerController?.SetMovementLocked(false);
            if (pauseButton != null)
                pauseButton.SetActive(true);
        }

        public void Restart()
        {
            // Tra timeScale TRUOC khi nap scene: LoadScene khong tu dat lai, va scene moi se dung
            // hinh ngay tu khung dau tien.
            Time.timeScale = 1f;
            IsPaused = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void OpenSettings()
        {
            ApplyPanel(pausePanel, false);
            ApplyPanel(settingsPanel, true);
            if (openRoutine != null)
                StopCoroutine(openRoutine);
            openRoutine = StartCoroutine(OpenAnimation(settingsPanel));
        }

        public void CloseSettings()
        {
            ApplyPanel(settingsPanel, false);
            ApplyPanel(pausePanel, true);
            if (openRoutine != null)
                StopCoroutine(openRoutine);
            openRoutine = StartCoroutine(OpenAnimation(pausePanel));
        }

        public void QuitToLevelSelect()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            levelManager?.LoadLevelSelect();
            ApplyPanel(pausePanel, false);
            ApplyPanel(settingsPanel, false);
            if (mobileInputRoot != null)
                mobileInputRoot.SetActive(true);
            if (pauseButton != null)
                pauseButton.SetActive(true);
        }

        // ---- noi bo ----

        private static void ApplyPanel(CanvasGroup group, bool visible)
        {
            if (group == null)
                return;

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
            group.gameObject.SetActive(visible);
        }

        // unscaledDeltaTime chu KHONG phai deltaTime: luc nay timeScale dang bang 0 nen deltaTime
        // cung bang 0, vong lap se chay mai mai ma khong nhich.
        private IEnumerator OpenAnimation(CanvasGroup group)
        {
            if (group == null)
                yield break;

            Transform tr = group.transform;
            float t = 0f;
            while (t < openDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / openDuration);
                float eased = k * k * (3f - 2f * k);

                group.alpha = eased;
                float s = Mathf.Lerp(openStartScale, 1f, eased);
                tr.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            group.alpha = 1f;
            tr.localScale = Vector3.one;
            openRoutine = null;
        }
    }
}
