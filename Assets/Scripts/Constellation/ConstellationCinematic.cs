using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSower.CameraSystem;
using StarSower.FX;

namespace StarSower.Constellations
{
    // Doan phim khoi phuc chom sao, dien NGAY TRONG SCENE dang choi (S2-006).
    //
    // Toan bo dien ra trong KHONG GIAN THE GIOI nen tai dung nguyen si ha tang FX cua S2-005:
    // StarFXPool, StarFlyAnimator (Bezier + duoi + bui + lap lanh), PocketFXController. Khong mot
    // dong nao cua ba thu do phai sua, va khong co phep quy doi toa do man hinh nao moi frame.
    //
    // ConstellationScreen goi vao day; chu ky Show() cua no khong doi nen LevelFlowManager khong
    // phai sua gi.
    //
    // BO QUA DUOC: cham vao man bat ky luc nao la nhay thang toi trang thai cuoi. Nguoi choi
    // khong bao gio bi bat ngoi cho.
    public class ConstellationCinematic : MonoBehaviour, IStarFlightListener
    {
        [Header("Tham chieu")]
        [SerializeField] private StarFXPool pool;
        [SerializeField] private PocketFXController pocket;

        [Tooltip("De trong thi tu tim luc Awake. Dung de dong bang vat ly Hero trong luc canh dien.")]
        [SerializeField] private StarSower.Player.PlayerMotor heroMotor;

        [Tooltip("De trong thi tu tim luc Awake. Dung de bat hoat anh chay khi Hero di vao khung.")]
        [SerializeField] private StarSower.Player.PlayerAnimationController heroAnimation;
        [SerializeField] private ConstellationSkyBackdrop sky;
        [SerializeField] private ConstellationLineDrawer lineDrawer;

        [Tooltip("Camera dien canh. De trong thi lay Camera.main mot lan luc Awake.")]
        [SerializeField] private Camera sequenceCamera;

        [Tooltip("Tat trong luc dien de tu lai camera, bat lai khi xong. De trong thi bo qua.")]
        [SerializeField] private CameraFollow2D cameraFollow;

        [Tooltip("Man che den. Dung chinh SpriteRenderer phu kin khung hinh.")]
        [SerializeField] private SpriteRenderer fadeOverlay;

        [Header("Node")]
        [Tooltip("Prefab mot ngoi sao trong chom.")]
        [SerializeField] private ConstellationNode nodePrefab;

        [Tooltip("Node duoc dat lam con cua doi tuong nay.")]
        [SerializeField] private Transform nodeParent;

        [Tooltip("Kich thuoc vung ve chom sao (world units). Star Points 0..1 duoc trai len day.")]
        [SerializeField] private Vector2 fieldSize = new Vector2(4.2f, 5.2f);

        [Tooltip("Lech tam vung ve so voi GIUA khung hinh luc dien xong. (0,0) = chinh giua man " +
                 "hinh. Tinh theo vi tri camera SAU khi da troi len, khong phai luc bat dau.")]
        [SerializeField] private Vector2 fieldCenterOffset = Vector2.zero;

        [Tooltip("Co Fly Core trong doan phim — thuong nho hon luc nhat sao ngoai man choi.")]
        [SerializeField] private float flyCoreScale = 0.7f;

        [Header("Nhan ten")]
        [SerializeField] private CanvasGroup nameGroup;
        [SerializeField] private UnityEngine.UI.Text nameLabel;
        [SerializeField] private UnityEngine.UI.Text descriptionLabel;

        [Header("Nhip (giay)")]
        [SerializeField] private float fadeInDuration = 0.8f;
        [SerializeField] private float pocketGlowDuration = 0.5f;
        [Tooltip("Khoang cach giua hai lan ban sao. Sao phai bay len tu ton — day la nhip cham " +
                 "nhat cua ca doan, dung de no thanh mot trang phao hoa.")]
        [SerializeField] private float launchDelay = 0.55f;

        [Tooltip("Thoi luong bay cua MOT ngoi sao trong doan phim. Ghi de rieng, khong dung khoang " +
                 "0.35-0.45 cua prefab Fly Core — prefab do dung chung voi luc nhat sao ngoai man choi.")]
        [SerializeField] private float flightDuration = 1.1f;
        [SerializeField] private float nodeActivateDuration = 0.15f;
        [SerializeField] private float lineDrawDuration = 1.4f;
        [Tooltip("Thoi gian ca chom sang bung len sau khi noi xong net.")]
        [SerializeField] private float constellationGlowDuration = 1.2f;
        [SerializeField] private float nameRevealDuration = 0.5f;
        [SerializeField] private float holdDuration = 1.4f;
        [SerializeField] private float fadeOutDuration = 1.3f;

        [Tooltip("Quang dau khong nhan lenh bo qua, tinh bang giay. Chan cu cham con sot lai tu " +
                 "man choi khien doan phim bi tua het ngay lap tuc.")]
        [Min(0f)]
        [SerializeField] private float skipGracePeriod = 0.6f;

        [Header("Camera")]
        [Tooltip("Camera troi len bao nhieu trong ca doan (world units). Dat nho thoi: troi nhieu " +
                 "qua thi Hero bi day ra khoi day khung hinh.")]
        [SerializeField] private float cameraRise = 0.5f;

        [Tooltip("Hero dung o dau khung hinh, tinh theo nua chieu cao khung tinh tu tam xuong. " +
                 "0.78 = gan sat day. Ha xuong thi Hero len cao hon trong khung.")]
        [Range(0f, 1f)]
        [SerializeField] private float heroGroundOffset = 0.78f;

        [Header("Hero chay vao")]
        [Tooltip("Bat thi Hero chay tu mep TRAI vao giua khung roi moi bat dau ban sao. Tat thi " +
                 "Hero dung yen tai cho nhu truoc.")]
        [SerializeField] private bool heroRunsIn = true;

        [Min(0.1f)]
        [SerializeField] private float heroRunDuration = 1.4f;

        [Tooltip("Xuat phat cach mep trai khung hinh bao nhieu (world units) — de Hero that su " +
                 "chay TU NGOAI man vao chu khong hien ra o mep.")]
        [SerializeField] private float heroStartMargin = 1.2f;
        [Tooltip("Chom sao va ten to len bao nhieu lan o doan khep, trong khi man hinh mo dan. " +
                 "Chi phong hai thu do — Hero khong bi anh huong.")]
        [SerializeField] private float finalZoom = 1.35f;

        [Header("Am thanh")]
        [Tooltip("Tieng vang luc chom sao hoan thanh. De trong thi bo qua, khong phai loi.")]
        [SerializeField] private AudioClip completionSound;
        [Range(0f, 1f)] [SerializeField] private float completionVolume = 0.9f;

        [Tooltip("AudioSource dung chung. De trong thi lay tren GameObject cua StarFXPool.")]
        [SerializeField] private AudioSource sharedSource;

        [Tooltip("Tieng moi khi mot node sang len. De trong thi dung lai tieng nhat sao cua " +
                 "StarCollectEffect neu co.")]
        [SerializeField] private AudioClip[] nodeSounds;

        private readonly List<ConstellationNode> nodes = new List<ConstellationNode>();
        private readonly List<Vector3> nodeBasePositions = new List<Vector3>();
        private Vector3 fieldCenter;
        private ConstellationData activeData;
        private Camera cam;
        private Vector3 cameraStart;
        private float orthoStart;
        private int arrivedCount;
        private float skipGraceTimer;
        private bool skipRequested;
        private bool running;

        public bool IsRunning => running;

        // Tu tra cac phu thuoc trong scene DUNG MOT LAN luc Awake, giong cach StarCollectEffect
        // dang lam. Nho vay ca bo rig dong goi duoc thanh prefab ma khong can noi tay tham chieu
        // nao cho tung man — chi con moi o Cinematic tren ConstellationScreen la phai gan.
        private void Awake()
        {
            if (pool == null)
                pool = FindFirstObjectByType<StarFXPool>();
            if (pocket == null)
                pocket = FindFirstObjectByType<PocketFXController>();
            if (cameraFollow == null)
                cameraFollow = FindFirstObjectByType<CameraFollow2D>();
            if (heroMotor == null)
                heroMotor = FindFirstObjectByType<StarSower.Player.PlayerMotor>();
            if (heroAnimation == null)
                heroAnimation = FindFirstObjectByType<StarSower.Player.PlayerAnimationController>();

            cam = sequenceCamera != null ? sequenceCamera : Camera.main;

            if (sharedSource == null && pool != null)
                sharedSource = pool.GetComponent<AudioSource>();

            if (sky == null)
                sky = GetComponentInChildren<ConstellationSkyBackdrop>(true);
            if (lineDrawer == null)
                lineDrawer = GetComponentInChildren<ConstellationLineDrawer>(true);
        }

        // Dat man che cua CHINH doan phim ve den kin ngay lap tuc. Goi TRUOC khi mo lop che
        // chuyen canh: neu mo lop kia ra truoc thi nguoi choi thay loe mot khung hinh man choi cu.
        public void SnapCovered()
        {
            SetOverlayAlpha(1f);
        }

        // litBefore = so node da khoi phuc tu nhung lan choi truoc (sang san, khong dien lai).
        // litAfter  = tong so node sang sau luot nay. Hieu hai so la so sao bay ra lan nay.
        //
        // Cho phep litAfter < tong so node: chom sao lanh dan qua nhieu luot choi, dung yeu cau
        // "khong duoc gia dinh moi chom deu hoan thanh trong mot man".
        public IEnumerator Play(ConstellationData data, int litBefore, int litAfter)
        {
            if (data == null || data.StarPoints.Count == 0)
                yield break;

            running = true;
            skipRequested = false;
            skipGraceTimer = skipGracePeriod;

            int total = data.StarPoints.Count;
            litBefore = Mathf.Clamp(litBefore, 0, total);
            litAfter = Mathf.Clamp(litAfter, litBefore, total);

            activeData = data;
            PrepareCamera();
            MoveRigToCamera();
            BuildNodes(data);
            ApplyLabels(data);

            for (int i = 0; i < nodes.Count; i++)
                nodes[i].SetInstant(i < litBefore);

            lineDrawer?.Prepare(data.Connections, nodes);
            lineDrawer?.SetProgress(data.Connections, nodes, 1f);   // net cu giu nguyen tu truoc
            sky?.Begin(pool, cam != null ? cam.transform : transform);

#if UNITY_EDITOR
            Debug.Log($"[Cine] cam={(cam != null ? cam.transform.position.ToString("F2") : "NULL")} " +
                      $"ortho={(cam != null ? cam.orthographicSize : -1f):F2} rig={transform.position:F2} " +
                      $"| litBefore={litBefore} litAfter={litAfter} node={nodes.Count} " +
                      $"node0={(nodes.Count > 0 ? nodes[0].transform.position.ToString("F2") : "-")} " +
                      $"| sky={(sky != null ? sky.transform.localScale.x.ToString("F2") : "NULL")} " +
                      $"cover={(fadeOverlay != null ? fadeOverlay.transform.localScale.x.ToString("F2") : "NULL")} " +
                      $"| pool={(pool != null)} pocket={(pocket != null ? pocket.PocketAnchor.position.ToString("F2") : "NULL")}", this);
#endif
            yield return FadeIn();
            yield return HeroRunIn();
            yield return PocketAwaken();
            yield return LaunchStars(data, litBefore, litAfter);
            yield return DrawLines(data);
            yield return Complete(data, litAfter >= total);
            yield return Hold();
            yield return ZoomAndFadeOut();

            Cleanup();
            running = false;
        }

        // Cham vao man -> nhay thang toi trang thai cuoi.
        //
        // Phai la mot cu cham MOI, khong phai "dang co ngon tay tren man". Luc man choi vua ket
        // thuc, nguoi choi thuong con dang giu joystick hoac nut nhay — Input.touchCount > 0 se
        // dung NGAY o frame dau, moi vong cho sap ve 0 va ca 5 ngoi sao ban ra cung mot luc.
        //
        // Them ca quang an: vai phan muoi giay dau khong nhan bo qua, de cu cham con sot lai tu
        // man choi khong an mat doan mo man.
        private void Update()
        {
            if (!running || skipRequested)
                return;

            skipGraceTimer -= Time.deltaTime;
            if (skipGraceTimer > 0f)
                return;

            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                skipRequested = true;
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase != TouchPhase.Began)
                    continue;

                skipRequested = true;
                return;
            }
        }

        // Cho, nhung cat ngay khi nguoi choi cham man.
        private IEnumerator Wait(float seconds)
        {
            float t = 0f;
            while (t < seconds && !skipRequested)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        private void PrepareCamera()
        {
            if (cam == null)
                cam = Camera.main;
            if (cam == null)
                return;

            // CameraFollow2D tu nhan la chu so huu duy nhat cua transform.position. Phai tat no
            // truoc khi tu lai camera, neu khong hai ben ghi de nhau moi frame.
            if (cameraFollow != null)
                cameraFollow.enabled = false;

            orthoStart = cam.orthographicSize;

            // Camera GIU NGUYEN cho no dang dung, chi keo ve giua theo truc ngang. Khong con canh
            // theo Hero nua: luc cham dich Hero co the dang lo lung tren cao, va doan phim khong
            // quan tam truoc do no o dau — HeroRunIn() se dat no vao dung cho trong khung.
            Vector3 p = cam.transform.position;
            p.x = 0f;
            cam.transform.position = p;
            cameraStart = p;
        }

        // Ca bo rig nam o goc the gioi, con camera luc hoan thanh man lai o tren cao (SampleScene
        // len toi y ~41). Khong keo rig theo thi nen troi, man che va anh hoan thanh deu nam ngoai
        // khung hinh — nguoi choi chi thay moi cai ten. Node thi khong dinh vi ca hai da tinh
        // theo vi tri camera, nen truoc do chung la thu duy nhat hien ra.
        private void MoveRigToCamera()
        {
            if (cam == null)
                return;

            Vector3 c = cam.transform.position;
            Vector3 p = transform.position;

            // Neo o GIUA quang camera se troi qua, khong phai diem bat dau: camera di len
            // cameraRise trong suot doan phim, ma nen troi thi dung yen — neo o diem dau se ho
            // ra mot dai o MEP TREN dung bang cameraRise (thay ro man choi cu loi qua do).
            transform.position = new Vector3(c.x, c.y + cameraRise * 0.5f, p.z);

            // Phu kin khung hinh. Scale cung khong bao gio dung: khung nhin doi theo ty le man
            // hinh va theo orthographicSize ma CameraAspectFitter tinh lai luc chay. Cong them
            // nua quang troi vao chieu cao de hai dau deu kin.
            float halfH = cam.orthographicSize + cameraRise * 0.5f;
            float halfW = cam.orthographicSize * cam.aspect;
            FitToView(sky != null ? sky.GetComponent<SpriteRenderer>() : null, halfW, halfH, 1.08f);
            FitToView(fadeOverlay, halfW, halfH, 1.25f);
        }

        // Keo mot SpriteRenderer cho trum kin nua-khung (halfW x halfH), giu nguyen tam.
        private static void FitToView(SpriteRenderer sr, float halfW, float halfH, float margin)
        {
            if (sr == null || sr.sprite == null)
                return;

            Vector2 size = sr.sprite.bounds.size;
            if (size.x <= 0f || size.y <= 0f)
                return;

            float k = Mathf.Max(halfW * 2f / size.x, halfH * 2f / size.y) * margin;
            sr.transform.localScale = new Vector3(k, k, 1f);
        }

        private void BuildNodes(ConstellationData data)
        {
            if (nodePrefab == null || nodeParent == null)
                return;

            // Tao du node mot lan roi dung lai mai — khong Instantiate giua buoi dien.
            while (nodes.Count < data.StarPoints.Count)
                nodes.Add(Instantiate(nodePrefab, nodeParent));

            // Canh theo vi tri camera SAU khi da troi len het (cameraStart + cameraRise), khong
            // phai vi tri luc bat dau. Node dat mot lan roi dung yen, ma camera thi con di len —
            // canh theo diem dau thi ve cuoi doan chom sao se tut xuong duoi giua khung hinh.
            Vector3 origin = cam != null ? cameraStart + Vector3.up * cameraRise : transform.position;
            origin.z = nodeParent.position.z;
            Vector3 center = origin + new Vector3(fieldCenterOffset.x, fieldCenterOffset.y, 0f);
            fieldCenter = center;
            nodeBasePositions.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                bool used = i < data.StarPoints.Count;
                nodes[i].gameObject.SetActive(used);
                if (!used)
                    continue;

                Vector2 n = data.StarPoints[i];
                Vector3 pos = center + new Vector3(
                    (n.x - 0.5f) * fieldSize.x, (n.y - 0.5f) * fieldSize.y, 0f);
                nodes[i].transform.position = pos;
                nodeBasePositions.Add(pos);
                nodes[i].SetZoomScale(1f);
            }
        }

        private void ApplyLabels(ConstellationData data)
        {
            if (nameLabel != null) nameLabel.text = data.DisplayName;
            if (descriptionLabel != null) descriptionLabel.text = data.Description;
            if (nameGroup != null) nameGroup.alpha = 0f;
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < fadeInDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeInDuration);
                SetOverlayAlpha(1f - k);
                sky?.SetAlpha(k);
                RiseCamera(k);
                yield return null;
            }
            SetOverlayAlpha(0f);
            sky?.SetAlpha(1f);
        }

        // Hero chay tu ngoai mep trai vao giua khung roi dung lai.
        //
        // Dong bang vat ly bang chinh API cua PlayerMotor (SetPhysicsActive) thay vi tu ghi
        // Rigidbody2D: PlayerMotor van la noi duy nhat dung toi rigidbody, dung quy tac cua du an.
        // Vat ly tat thi van toc bang 0 nen Animator se hien Idle — phai bao thang cho no biet
        // dang chay qua SetScriptedMotion.
        private IEnumerator HeroRunIn()
        {
            if (!heroRunsIn || heroMotor == null || cam == null)
                yield break;

            Transform hero = heroMotor.transform;
            heroMotor.SetPhysicsActive(false);

            float halfW = cam.orthographicSize * cam.aspect;
            float halfH = cam.orthographicSize;
            Vector3 c = cam.transform.position;

            // Dat Hero vao mot do cao CO DINH trong khung, khong dung do cao cu cua no. Luc cham
            // dich no co the dang bay lo lung — dung nguyen do cao do thi o man chom sao Hero se
            // chay giua khong trung.
            Vector3 target = new Vector3(c.x, c.y - halfH * heroGroundOffset, hero.position.z);
            Vector3 from = new Vector3(c.x - halfW - heroStartMargin, target.y, target.z);
            hero.position = from;

            heroAnimation?.SetScriptedMotion(true, 1f);

            float t = 0f;
            while (t < heroRunDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / heroRunDuration);
                // Cham dan ve cuoi: dung khuc ngay giua khung chu khong phanh gap.
                float eased = 1f - (1f - k) * (1f - k);
                hero.position = Vector3.Lerp(from, target, eased);
                yield return null;
            }

            hero.position = target;
            heroAnimation?.SetScriptedMotion(true, 0f);
        }

        private IEnumerator PocketAwaken()
        {
            if (pocket != null && pool != null)
                pocket.PlayArrival(pool, pocket.PocketAnchor.position);

            yield return Wait(pocketGlowDuration);
        }

        private IEnumerator LaunchStars(ConstellationData data, int litBefore, int litAfter)
        {
            int newly = litAfter - litBefore;
            if (newly <= 0)
                yield break;

            Transform anchor = pocket != null ? pocket.PocketAnchor : transform;
            arrivedCount = 0;

            for (int k = 0; k < newly; k++)
            {
                int index = litBefore + k;
                if (index >= nodes.Count)
                    break;

                LaunchOne(anchor.position, index);

                if (k < newly - 1)
                    yield return Wait(launchDelay);
            }

            // Cho tat ca ha canh, nhung co tran thoi gian de khong bao gio treo neu mot ngoi
            // sao khong bao ve duoc.
            float guard = 0f;
            while (arrivedCount < newly && guard < 3f && !skipRequested)
            {
                guard += Time.deltaTime;
                yield return null;
            }

            // Bo qua hoac het gio: ep moi node con lai sang ngay.
            for (int k = 0; k < newly; k++)
            {
                int index = litBefore + k;
                if (index < nodes.Count && !nodes[index].IsLit)
                    nodes[index].Activate(pool);
            }
        }

        // Ban mot ngoi sao toi node thu nodeIndex. Dung DUNG StarFlyAnimator cua S2-005, khong
        // sua gi phan Bezier/duoi/bui/lap lanh — chi dat Tag de luc no bao ve con biet la node nao.
        private void LaunchOne(Vector3 from, int nodeIndex)
        {
            if (pool == null)
            {
                ArriveAt(nodeIndex);
                return;
            }

            PooledStarFX core = pool.Spawn(StarFXType.FlyCore, from, 0f, flyCoreScale, 1f, 999f);
            var flight = core != null ? core.GetComponent<StarFlyAnimator>() : null;

            if (flight == null)
            {
                // Thieu Fly Core (het pool, thieu prefab) thi vao thang trang thai cuoi — hieu ung
                // thieu khong bao gio duoc phep chan tien trinh.
                if (core != null) core.ReturnNow();
                ArriveAt(nodeIndex);
                return;
            }

            flight.Tag = nodeIndex;
            flight.SetDurationOverride(flightDuration);
            flight.Begin(pool, from, nodes[nodeIndex].transform, nodes[nodeIndex].transform.position, this);
        }

        // IStarFlightListener — nhieu ngoi sao bay cung luc nen phai doc Tag ra biet la ngoi nao.
        public void OnStarFlightArrived(StarFlyAnimator source, Vector3 position)
        {
            ArriveAt(source != null ? source.Tag : -1);
        }

        private void ArriveAt(int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < nodes.Count && !nodes[nodeIndex].IsLit)
            {
                nodes[nodeIndex].Activate(pool);
                PlayNodeSound();
            }
            arrivedCount++;
        }

        private IEnumerator DrawLines(ConstellationData data)
        {
            if (lineDrawer == null)
                yield break;

            lineDrawer.Prepare(data.Connections, nodes);

            float t = 0f;
            while (t < lineDrawDuration && !skipRequested)
            {
                t += Time.deltaTime;
                lineDrawer.SetProgress(data.Connections, nodes, Mathf.Clamp01(t / lineDrawDuration));
                yield return null;
            }
            lineDrawer.SetProgress(data.Connections, nodes, 1f);
        }

        // Ve xong net thi CA CHOM sang bung len cung luc — day moi la khoanh khac "bau troi vua
        // lanh lai", khong can mot tam anh phu de len. Ten chom sao chi hien SAU do.
        private IEnumerator Complete(ConstellationData data, bool fullyRestored)
        {
            PlayCompletionSound();

            float t = 0f;
            while (t < constellationGlowDuration && !skipRequested)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / constellationGlowDuration);

                // Vong len roi lang xuong mot nua: sang bung roi dium lai, giu duoc mot chut
                // du sang chu khong tro ve nhu cu.
                float boost = Mathf.Sin(k * Mathf.PI) * 0.6f + k * 0.4f;
                ApplyGlow(boost);

                yield return null;
            }
            ApplyGlow(0.4f);

            yield return RevealName();
        }

        private void ApplyGlow(float amount)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].SetGlowBoost(amount);
            lineDrawer?.SetGlowBoost(amount);
        }

        // Ten hien ra: mo dan CONG voi phong tu 0.86 len 1 — chi mo dan khong thoi thi phang va
        // khong co cam giac "hien ra".
        private IEnumerator RevealName()
        {
            if (nameGroup == null)
                yield break;

            Transform tr = nameGroup.transform;
            float n = 0f;
            while (n < nameRevealDuration && !skipRequested)
            {
                n += Time.deltaTime;
                float k = Mathf.Clamp01(n / nameRevealDuration);
                float eased = k * k * (3f - 2f * k);
                nameGroup.alpha = eased;
                float sc = Mathf.Lerp(0.86f, 1f, eased);
                tr.localScale = new Vector3(sc, sc, 1f);
                yield return null;
            }
            nameGroup.alpha = 1f;
            tr.localScale = Vector3.one;
        }

        private IEnumerator Hold()
        {
            yield return Wait(holdDuration);
        }

        // Doan khep: chom sao va ten to dan len trong khi man hinh mo dan ve den, roi tra quyen
        // cho LevelFlowManager nap man ke.
        //
        // Phong TUNG DOI TUONG chu khong dung camera.orthographicSize: doi ortho la phong ca khung
        // hinh, Hero dang dung sat day se bi cat mat. Cach nay Hero nam yen nguyen ven.
        private IEnumerator ZoomAndFadeOut()
        {
            Transform nameTr = nameGroup != null ? nameGroup.transform : null;
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeOutDuration);
                float eased = k * k * (3f - 2f * k);
                float zoom = Mathf.Lerp(1f, finalZoom, eased);

                ScaleConstellation(zoom);
                if (nameTr != null)
                    nameTr.localScale = new Vector3(zoom, zoom, 1f);

                SetOverlayAlpha(eased);
                sky?.SetAlpha(1f - eased);
                yield return null;
            }
            SetOverlayAlpha(1f);
        }

        // Day cac node ra xa tam chom sao va phong to chinh chung; net noi doc lai toa do hai dau
        // moi frame nen bam theo, khong bi roi khoi ngoi sao.
        private void ScaleConstellation(float k)
        {
            for (int i = 0; i < nodes.Count && i < nodeBasePositions.Count; i++)
            {
                nodes[i].transform.position = fieldCenter + (nodeBasePositions[i] - fieldCenter) * k;
                nodes[i].SetZoomScale(k);
            }

            if (lineDrawer == null || activeData == null)
                return;

            lineDrawer.SetThicknessScale(k);
            lineDrawer.RefreshPoints(activeData.Connections, nodes);
            lineDrawer.SetProgress(activeData.Connections, nodes, 1f);
        }


        private void Cleanup()
        {
            sky?.Stop();
            sky?.SetAlpha(0f);
            ApplyGlow(0f);
            lineDrawer?.HideAll();

            for (int i = 0; i < nodes.Count; i++)
                nodes[i].gameObject.SetActive(false);

            if (nameGroup != null)
            {
                nameGroup.alpha = 0f;
                // Tra co ve 1: doan khep de lai o finalZoom, lan dien sau phai bat dau tu goc.
                nameGroup.transform.localScale = Vector3.one;
            }

            // Tra camera ve dung cho cu roi moi bat lai CameraFollow2D — bat truoc thi no se
            // bat dau bam tu vi tri sai va truot mot doan thay duoc.
            if (cam != null)
            {
                cam.transform.position = cameraStart;
                cam.orthographicSize = orthoStart;
            }
            if (cameraFollow != null)
                cameraFollow.enabled = true;

            // Tra Hero ve quyen dieu khien binh thuong. Neu con man ke thi scene nay sap bi huy,
            // nhung o man cuoi Journey Cinematic con dung toi nen phai tra tu te.
            heroAnimation?.SetScriptedMotion(false, 0f);
            heroMotor?.SetPhysicsActive(true);

            // CO Y GIU MAN CHE DEN. Truoc kia cho ve 0 o day, nen sau khi mo dan het thi man choi
            // cu LOE LAI mot nhip roi lop chuyen canh moi che lan nua — nguoi choi thay "quay ve
            // level roi moi sang level 2". Giu den lien mach cho toi khi scene ke duoc nap.
            //
            // Man CUOI khong co scene ke: ConstellationScreen goi ClearCover() de tra man hinh
            // lai cho Journey Cinematic.
        }

        // Mo man che cua doan phim. Chi goi khi KHONG con man ke — luc do khong ai nap scene moi
        // nen phai tu tra lai khung hinh.
        public void ClearCover()
        {
            SetOverlayAlpha(0f);
        }

        // Camera CHI troi len trong luc mo man, sau do dung yen tuyet doi.
        //
        // Truoc kia no troi tiep trong luc ban sao: camera di len thi hinh tut xuong, ma node thi
        // dung yen — thanh ra ca chom sao truot dan xuong duoi khung trong khi sao dang bay toi.
        // Nguoi choi doc ra ngay la "man hinh dang dich xuong", va sao thi bay toi muc tieu dang
        // chay. Doan ket chi con zoom, khong troi nua.
        private void RiseCamera(float t)
        {
            if (cam == null) return;
            Vector3 p = cameraStart;
            p.y += cameraRise * Mathf.Clamp01(t);
            cam.transform.position = p;
        }


        private void SetOverlayAlpha(float a)
        {
            if (fadeOverlay == null) return;
            Color c = fadeOverlay.color;
            c.a = Mathf.Clamp01(a);
            fadeOverlay.color = c;
            fadeOverlay.gameObject.SetActive(c.a > 0.001f);
        }

        private void PlayNodeSound()
        {
            if (sharedSource == null || nodeSounds == null || nodeSounds.Length == 0)
                return;
            AudioClip clip = nodeSounds[Random.Range(0, nodeSounds.Length)];
            if (clip != null)
                sharedSource.PlayOneShot(clip, 0.7f);
        }

        private void PlayCompletionSound()
        {
            if (sharedSource != null && completionSound != null)
                sharedSource.PlayOneShot(completionSound, completionVolume);
        }
    }
}
