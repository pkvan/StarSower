using System.Collections.Generic;
using UnityEngine;

namespace StarSower.FX
{
    // Kho FX dung lai cho hieu ung nhat sao. Mot doi tuong trong scene, khong Singleton, khong
    // DontDestroyOnLoad — dung quy tac kien truc cua du an. Scene bi huy thi ca kho di theo, khong
    // de lai FX mo coi giua hai man.
    //
    // Tra cuu bang chi so mang theo StarFXType: khong bam chuoi, khong Dictionary, khong LINQ.
    public class StarFXPool : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            [Tooltip("Prefab cua loai FX nay. De trong thi loai do bi bo qua (co canh bao 1 lan).")]
            public PooledStarFX prefab;

            [Min(0)]
            [Tooltip("So luong tao san luc vao man. Du prewarm thi luot nhat sao binh thuong " +
                     "khong bao gio phai Instantiate.")]
            public int prewarm = 8;
        }

        [Tooltip("Khai bao theo DUNG thu tu cua StarFXType. Phan tu thieu se duoc bo qua an toan.")]
        [SerializeField] private Entry[] entries = new Entry[StarFXTypeInfo.Count];

        [Tooltip("Cho phep tao them khi ngan can. Tat thi ngan can se bo qua FX do thay vi giat khung hinh.")]
        [SerializeField] private bool allowGrow = true;

        [Tooltip("Tran cung cua moi loai, ke ca khi duoc phep tao them. Chan ro ri khong gioi han " +
                 "neu co gi do quen tra ve.")]
        [Min(1)]
        [SerializeField] private int hardCap = 64;

        private Stack<PooledStarFX>[] available;
        private int[] liveCount;
        private bool[] warned;
        private Transform holder;

        private void Awake()
        {
            available = new Stack<PooledStarFX>[StarFXTypeInfo.Count];
            liveCount = new int[StarFXTypeInfo.Count];
            warned = new bool[StarFXTypeInfo.Count];

            holder = new GameObject("PooledFX").transform;
            holder.SetParent(transform, worldPositionStays: false);

            for (int i = 0; i < StarFXTypeInfo.Count; i++)
            {
                available[i] = new Stack<PooledStarFX>(16);
                Entry e = i < entries.Length ? entries[i] : null;
                if (e == null || e.prefab == null)
                    continue;

                for (int n = 0; n < e.prewarm; n++)
                    available[i].Push(CreateOne((StarFXType)i, e.prefab));
            }
        }

        private PooledStarFX CreateOne(StarFXType type, PooledStarFX prefab)
        {
            PooledStarFX fx = Instantiate(prefab, holder);
            fx.Bind(this, type);
            fx.ResetState();
            fx.gameObject.SetActive(false);
            liveCount[(int)type]++;
            return fx;
        }

        // Lay mot manh FX ra. Tra ve null khi khong co prefab hoac da cham tran — ben goi phai
        // chiu duoc null, vi hieu ung thieu mot hat bui khong bao gio duoc phep chan phan thuong.
        public PooledStarFX Spawn(StarFXType type, Vector3 position, float rotation = 0f,
                                  float scaleMul = 1f, float alphaMul = 1f,
                                  float lifetimeOverride = 0f, Vector2 extraDrift = default)
        {
            int i = (int)type;
            if (available == null || i < 0 || i >= StarFXTypeInfo.Count)
                return null;

            Entry e = i < entries.Length ? entries[i] : null;
            if (e == null || e.prefab == null)
            {
                WarnOnce(type, "chua gan prefab");
                return null;
            }

            PooledStarFX fx;
            if (available[i].Count > 0)
            {
                fx = available[i].Pop();
            }
            else if (allowGrow && liveCount[i] < hardCap)
            {
                // Chi xay ra khi prewarm dat qua thap. Canh bao DUNG MOT LAN moi loai — canh bao
                // moi frame se lam ngap Console va lam cham hon chinh cai no dang canh bao.
                WarnOnce(type, $"het hang, phai tao them (prewarm={e.prewarm}). Tang prewarm len.");
                fx = CreateOne(type, e.prefab);
            }
            else
            {
                return null;
            }

            fx.gameObject.SetActive(true);
            fx.MarkTaken();
            fx.Play(position, rotation, scaleMul, alphaMul, lifetimeOverride, extraDrift);
            return fx;
        }

        // Goi boi chinh PooledStarFX. Khong dung Destroy bao gio.
        public void Release(StarFXType type, PooledStarFX fx)
        {
            if (fx == null)
                return;

            int i = (int)type;
            fx.ResetState();

            if (available == null || i < 0 || i >= StarFXTypeInfo.Count)
            {
                fx.gameObject.SetActive(false);
                return;
            }

            fx.transform.SetParent(holder, worldPositionStays: false);
            fx.gameObject.SetActive(false);
            available[i].Push(fx);
        }

        private void WarnOnce(StarFXType type, string message)
        {
            int i = (int)type;
            if (warned[i])
                return;

            warned[i] = true;
            Debug.LogWarning($"[StarFX] {type}: {message}", this);
        }

#if UNITY_EDITOR
        // Doc trong Inspector luc chay de biet prewarm dat du chua.
        public string DebugStats()
        {
            if (available == null)
                return "chua khoi tao";

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < StarFXTypeInfo.Count; i++)
                sb.Append((StarFXType)i).Append('=').Append(available[i].Count)
                  .Append('/').Append(liveCount[i]).Append(' ');
            return sb.ToString();
        }

        private void OnValidate()
        {
            if (entries != null && entries.Length != StarFXTypeInfo.Count)
                System.Array.Resize(ref entries, StarFXTypeInfo.Count);
        }
#endif
    }
}
