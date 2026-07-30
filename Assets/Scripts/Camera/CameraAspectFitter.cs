using UnityEngine;

namespace StarSower.CameraSystem
{
    // Canh khung hinh doc: bao dam TOAN BO be rong choi duoc luon nam trong man hinh, tren moi ty
    // le portrait (9:16 -> 9:20), khong hardcode do phan giai nao.
    //
    //     requiredSize = playableWidth / (2 * aspect)
    //     orthographicSize = Max(requiredSize, minOrthographicSize)
    //
    // Lay Max chu khong lay thang requiredSize: tren may 9:16 (rong tuong doi) requiredSize chi
    // 4.62, ap vao se ZOOM VAO, khung nhin doc hep lai so voi hien tai. minOrthographicSize giu
    // nguyen khung doc da can chinh; may cang cao thi cang zoom ra de du be rong.
    //
    // CHI ghi orthographicSize KHI aspect doi (frame dau + luc xoay/doi cua so), khong ghi moi
    // frame. Ly do: CameraZoom va JourneyCinematic cung ghi thuoc tinh nay. Ghi moi frame se thanh
    // ba nguoi cung ghi mot thuoc tinh va dap len hieu ung cua nhau — dung loai loi da tung xay ra
    // voi localScale cua ngoi sao chom sao. O day Fitter chi dat MOC, hai cai kia toan quyen dieu
    // khien trong luc dien hieu ung.
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(-60)]
    public class CameraAspectFitter : MonoBehaviour
    {
        [Tooltip("Be rong the gioi phai luon nhin thay tron ven (world units). Noi dung gameplay " +
                 "cua ca 5 man nam trong X [-2.40, +2.50] = 4.90, cong le 0.15 moi ben = 5.20.")]
        [SerializeField] private float playableWidth = 5.2f;

        [Tooltip("Toa do X tam man. Camera bi khoa cung o day, khong bao gio truot ngang.")]
        [SerializeField] private float levelCenterX;

        [Tooltip("Khung nhin doc toi thieu — tren may rong (9:16) giu nguyen gia tri nay thay vi " +
                 "zoom vao. Bang dung orthographic size von co cua du an.")]
        [SerializeField] private float minOrthographicSize = 5f;

        private Camera cam;
        private float lastAspect = -1f;
        private int lastWidth, lastHeight;

        // Cho he thong khac doc lai mocs da tinh (vd: gioi han di chuyen cua Player).
        public float PlayableWidth => playableWidth;
        public float LevelCenterX => levelCenterX;

        private void Awake()
        {
            cam = GetComponent<Camera>();

            // Dat X ve tam man DUNG MOT LAN, trong Awake — truoc khi CameraFollow2D.Start() chup
            // lai vi tri lam moc. Tu do tro di CameraFollow2D la nguoi ghi transform.position duy
            // nhat (voi followX = false no giu nguyen X nay mai mai). Fitter khong dong vao position
            // them lan nao nua, chi con lo orthographicSize.
            Vector3 p = transform.position;
            transform.position = new Vector3(levelCenterX, p.y, p.z);
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            // Doi do phan giai / xoay may moi tinh lai. Khong co gi doi thi khong dong vao
            // orthographicSize, nhuong hoan toan cho CameraZoom va JourneyCinematic.
            if (Screen.width == lastWidth && Screen.height == lastHeight
                && Mathf.Approximately(cam.aspect, lastAspect))
                return;

            Apply();
        }

        private void Apply()
        {
            if (cam == null)
                cam = GetComponent<Camera>();

            if (!cam.orthographic || playableWidth <= 0f)
                return;

            float aspect = cam.aspect;
            if (aspect <= 0f)
                return;

            float requiredSize = playableWidth / (2f * aspect);
            cam.orthographicSize = Mathf.Max(requiredSize, minOrthographicSize);

            lastAspect = aspect;
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
}
