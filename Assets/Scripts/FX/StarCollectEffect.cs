using System.Collections;
using UnityEngine;
using StarSower.Collectibles;

namespace StarSower.FX
{
    // Dieu phoi toan bo man nhat mot manh sao (S2-005). Gan CUNG GameObject voi StarFragment.
    //
    // Ranh gioi trach nhiem: script nay chi lo THU TU va HINH ANH. Viec cong diem van do
    // CollectibleManager.RegisterCollected() lam — khong tao bo dem thu hai. Diem khac duy nhat so
    // voi truoc la THOI DIEM goi: truoc kia cong ngay luc cham, gio doi Fly Core bay toi tui.
    //
    // Manh sao KHONG bi Destroy luc cham nua. No chi tat renderer roi song tiep cho het duong bay,
    // vi neu huy ngay thi callback luc sao toi noi se roi vao hu vo va mat luon phan thuong.
    [RequireComponent(typeof(StarFragment))]
    public class StarCollectEffect : MonoBehaviour, IStarFlightListener
    {
        [Header("Tham chieu")]
        [Tooltip("De trong thi tu tim trong scene luc Awake (mot lan duy nhat, khong tim lai).")]
        [SerializeField] private StarFXPool pool;

        [Tooltip("De trong thi tu tim PocketFXController tren Hero luc Awake.")]
        [SerializeField] private PocketFXController pocket;

        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private StarIdleAnimator idleAnimator;

        [Tooltip("Renderer cua chinh manh sao + quang sang — tat het khi sao bat dau bay.")]
        [SerializeField] private SpriteRenderer[] fragmentRenderers;

        [SerializeField] private Collider2D fragmentCollider;

        [Header("Thoi luong (giay)")]
        [Min(0.01f)] [SerializeField] private float flashDuration = 0.05f;
        [Min(0.01f)] [SerializeField] private float burst01Duration = 0.08f;
        [Min(0.01f)] [SerializeField] private float burst02Duration = 0.1f;
        [Min(0.01f)] [SerializeField] private float burst03Duration = 0.12f;

        [Header("Co bung")]
        [SerializeField] private float flashScale = 1.6f;
        [SerializeField] private float burst01Scale = 1.5f;
        [SerializeField] private float burst02Scale = 1.7f;
        [SerializeField] private float burst03Scale = 2.4f;
        [SerializeField] private float flyCoreScale = 0.7f;

        [Header("Am thanh")]
        [Tooltip("Danh sach tieng nhat sao. Moi lan nhat boc ngau nhien mot bai — nhat lien tay " +
                 "vai ngoi sao ma cung mot tieng lap lai se nghe nhu loi ky thuat. " +
                 "De trong thi khong co tieng, khong phai loi, hieu ung van chay day du.")]
        [SerializeField] private AudioClip[] collectSounds;

        [Range(0f, 1f)] [SerializeField] private float collectVolume = 0.8f;

        [Tooltip("AudioSource dung CHUNG. De trong thi tu lay tren GameObject cua StarFXPool.")]
        [SerializeField] private AudioSource sharedSource;

        // Canh bao thieu AudioSource CHI MOT LAN ca phien: co 53 manh sao trong mot man, canh bao
        // tung cai se lam ngap Console.
        private static bool warnedNoSource;

        // Cache WaitForSeconds mot lan: neu new moi lan nhat thi moi manh sao lai sinh rac.
        private WaitForSeconds waitFlash;
        private WaitForSeconds waitBurst01;
        private WaitForSeconds waitBurst02;

        private bool isCollected;     // chan kich hoat trung — su kien trigger co the da xep hang
        private bool isCommitted;     // chan cong diem hai lan
        private bool sequenceRunning;

        private void Awake()
        {
            if (pool == null)
                pool = FindFirstObjectByType<StarFXPool>();
            if (pocket == null)
                pocket = FindFirstObjectByType<PocketFXController>();
            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();
            if (idleAnimator == null)
                idleAnimator = GetComponent<StarIdleAnimator>();
            if (fragmentCollider == null)
                fragmentCollider = GetComponent<Collider2D>();
            if (sharedSource == null && pool != null)
                sharedSource = pool.GetComponent<AudioSource>();

            if (idleAnimator != null)
                idleAnimator.SetPool(pool);

            waitFlash = new WaitForSeconds(flashDuration);
            waitBurst01 = new WaitForSeconds(burst01Duration);
            waitBurst02 = new WaitForSeconds(burst02Duration);
        }

        // Goi boi StarFragment khi Player cham. Tra ve true neu lan goi nay la lan thuc su bat dau —
        // StarFragment dua vao do de biet co nen chay luong cu hay khong.
        public bool TryBeginCollect()
        {
            if (isCollected)
                return false;

            isCollected = true;

            // Tat va cham NGAY, truoc moi thu khac. Khong chi dua vao co isCollected vi con co
            // huong nguoc lai: nhieu su kien trigger cua cung mot frame da nam san trong hang doi.
            if (fragmentCollider != null)
                fragmentCollider.enabled = false;

            if (idleAnimator != null)
                idleAnimator.StopIdle();

            if (isActiveAndEnabled)
            {
                sequenceRunning = true;
                StartCoroutine(Sequence());
            }
            else
            {
                // Khong chay coroutine duoc (object dang tat) — bo qua phan nhin, trao thuong luon.
                Commit();
            }

            return true;
        }

        private IEnumerator Sequence()
        {
            Vector3 origin = transform.position;

            if (pool != null)
            {
                pool.Spawn(StarFXType.Flash, origin, 0f, flashScale, 1f, flashDuration);
                yield return waitFlash;

                pool.Spawn(StarFXType.Burst01, origin, Random.Range(0f, 360f), burst01Scale, 1f, burst01Duration);
                yield return waitBurst01;

                pool.Spawn(StarFXType.Burst02, origin, Random.Range(0f, 360f), burst02Scale, 1f, burst02Duration);
                yield return waitBurst02;

                pool.Spawn(StarFXType.Burst03, origin, Random.Range(0f, 360f), burst03Scale, 1f, burst03Duration);
            }

            HideFragment();

            Transform anchor = pocket != null ? pocket.PocketAnchor : null;
            PooledStarFX core = pool != null
                ? pool.Spawn(StarFXType.FlyCore, origin, 0f, flyCoreScale, 1f, 999f)
                : null;

            var animator = core != null ? core.GetComponent<StarFlyAnimator>() : null;
            if (animator == null)
            {
                // Khong dung duoc Fly Core (het pool, thieu prefab, thieu script). Khong duoc phep
                // nuot phan thuong chi vi thieu mot hieu ung — trao luon.
                if (core != null)
                    core.ReturnNow();
                OnFlyCoreArrived(anchor != null ? anchor.position : origin);
                yield break;
            }

            animator.Begin(pool, origin, anchor, origin, this);
            sequenceRunning = false;
        }

        private void HideFragment()
        {
            if (fragmentRenderers == null)
                return;

            for (int i = 0; i < fragmentRenderers.Length; i++)
                if (fragmentRenderers[i] != null)
                    fragmentRenderers[i].enabled = false;
        }

        // IStarFlightListener — StarFlyAnimator bao khi sao cham tui. Manh sao chi ban dung
        // MOT ngoi nen khong can doc source.Tag.
        public void OnStarFlightArrived(StarFlyAnimator source, Vector3 position)
        {
            OnFlyCoreArrived(position);
        }

        // Goi boi StarFlyAnimator khi Fly Core cham tui (hoac khi duong bay bi cat giua chung).
        public void OnFlyCoreArrived(Vector3 position)
        {
            if (pocket != null && pool != null)
                pocket.PlayArrival(pool, position);

            Commit();

            // Gio moi tat han manh sao — den luc nay phan thuong da nam chac trong so.
            gameObject.SetActive(false);
        }

        // Cong diem + phat tieng. Chay DUNG MOT LAN, bat ke duoc goi tu bao nhieu duong.
        private void Commit()
        {
            if (isCommitted)
                return;

            isCommitted = true;

            if (collectibleManager != null)
                collectibleManager.RegisterCollected();
            else
                Debug.LogWarning("[StarCollect] Khong tim thay CollectibleManager — mat mot sao.", this);

            PlayCollectSound();
        }

        // Nam TRONG Commit() nen tu dong thua co isCommitted: mot manh sao chi phat dung mot tieng,
        // du trigger co ban vao bao nhieu lan hay Fly Core bao ve tu may duong.
        //
        // Khong cap phat gi: boc chi so nguyen tu mang co san, PlayOneShot khong sinh rac, va
        // KHONG dung PlayClipAtPoint (ham do tu tao roi tu huy mot GameObject moi lan goi — dung
        // hai dieu bi cam trong luot nhat).
        private void PlayCollectSound()
        {
            if (collectSounds == null || collectSounds.Length == 0)
                return;

            AudioClip clip = collectSounds.Length == 1
                ? collectSounds[0]
                : collectSounds[Random.Range(0, collectSounds.Length)];

            if (clip == null)
                return;

            if (sharedSource == null)
            {
                if (!warnedNoSource)
                {
                    warnedNoSource = true;
                    Debug.LogWarning("[StarCollect] Khong co AudioSource dung chung (thuong nam tren " +
                                     "StarFXPool) — sao van duoc cong, chi la khong co tieng.", this);
                }
                return;
            }

            sharedSource.PlayOneShot(clip, collectVolume);
        }

        // Luoi an toan cuoi cung: neu manh sao bi huy trong luc sao con dang bay (doi man, tai lai
        // scene) thi van khong duoc mat phan thuong.
        private void OnDestroy()
        {
            if (isCollected && !isCommitted && collectibleManager != null)
                Commit();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (fragmentCollider == null)
                fragmentCollider = GetComponent<Collider2D>();
            if (idleAnimator == null)
                idleAnimator = GetComponent<StarIdleAnimator>();
        }
#endif
    }
}
