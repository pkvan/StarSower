using System;
using System.Collections.Generic;
using UnityEngine;
using StarSower.Constellations;
using StarSower.Level;

namespace StarSower.Collectibles
{
    // Quản lý tổng số Star Fragment trong level + số đã thu thập. Đây là nguồn dữ liệu duy nhất
    // cho UI (HUD, LevelCompleteUI) — không component nào khác được tự đếm lại.
    //
    // S2-009 — TỔNG SỐ SAO DO CHÒM SAO QUYẾT ĐỊNH. Chòm sao của khu vực có bao nhiêu ngôi thì màn
    // phải có đúng bấy nhiêu sao: Cassiopeia 5 node -> Forgotten Forest 5 sao. Trước đây tổng số
    // là "đếm hết StarFragment có trong scene", nên số sao phụ thuộc vào việc designer đặt tay
    // bao nhiêu ngôi — hai nguồn sự thật cho cùng một con số.
    //
    // Sao THỪA không bị xoá khỏi scene mà chỉ tắt lúc chạy: đổi số node sau này là hệ tự điều
    // chỉnh, không phải đặt lại sao bằng tay.
    public class CollectibleManager : MonoBehaviour
    {
        [Header("Nguon so luong (S2-009)")]
        [Tooltip("Chapter chua danh sach chom sao. De trong thi quay ve luat cu: dem het sao co " +
                 "trong scene.")]
        [SerializeField] private ChapterData chapter;

        [Tooltip("De biet dang o level nao ma tra ra dung chom sao.")]
        [SerializeField] private LevelManager levelManager;

        public int TotalStars { get; private set; }
        public int CollectedStars { get; private set; }

        // Đã nhặt đủ chưa. Astral Gate đọc cái này để biết được mở hay chưa.
        public bool AllCollected => TotalStars > 0 && CollectedStars >= TotalStars;

        // (collected, total) — bắn cả lúc khởi tạo (collected=0) lẫn mỗi lần thu thập, để bên nghe
        // không cần biết thứ tự Start() giữa các script.
        public event Action<int, int> OnCollectedChanged;

        private readonly List<StarFragment> active = new List<StarFragment>();

        private void Start()
        {
            StarFragment[] found = FindObjectsByType<StarFragment>(FindObjectsSortMode.None);

            // Sắp theo ĐỘ CAO chứ không theo thứ tự Unity trả về: thứ tự đó không đảm bảo và có thể
            // đổi giữa các phiên bản, mà việc chọn ngôi nào phải cho ra kết quả giống hệt mỗi lần.
            Array.Sort(found, (a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

            int required = ResolveRequiredStars(found.Length);
            SelectSpread(found, required);

            TotalStars = active.Count;
            OnCollectedChanged?.Invoke(CollectedStars, TotalStars);
        }

        private int ResolveRequiredStars(int available)
        {
            ConstellationData constellation = ConstellationLookup.ForLevel(chapter, levelManager);
            if (constellation == null || constellation.NodeCount <= 0)
                return available;   // chua gan du du lieu -> luat cu, khong lam hong man choi

            if (constellation.NodeCount > available)
            {
                Debug.LogWarning($"[Collectible] '{constellation.DisplayName}' can " +
                                 $"{constellation.NodeCount} sao nhung scene chi co {available}. " +
                                 "Dat them StarFragment, neu khong cong se khong bao gio mo.", this);
                return available;
            }

            return constellation.NodeCount;
        }

        // Giữ N ngôi RẢI ĐỀU theo độ cao, tắt phần dư. Chia hành trình leo thành N chặng và lấy
        // một ngôi mỗi chặng, để người chơi nhặt đều suốt đường lên thay vì dồn cục ở một đoạn.
        private void SelectSpread(StarFragment[] sorted, int keep)
        {
            active.Clear();

            if (keep >= sorted.Length)
            {
                active.AddRange(sorted);
                return;
            }

            var chosen = new HashSet<int>();
            for (int i = 0; i < keep; i++)
            {
                // keep == 1 thì lấy ngôi ở giữa, tránh chia cho 0.
                float t = keep == 1 ? 0.5f : i / (float)(keep - 1);
                int index = Mathf.RoundToInt(t * (sorted.Length - 1));

                // Làm tròn có thể ra trùng chỉ số ở hai chặng liền nhau — dịch lên tới ô trống gần
                // nhất, nếu không sẽ giữ ít hơn số cần.
                while (index < sorted.Length && !chosen.Add(index))
                    index++;

                if (index < sorted.Length)
                    active.Add(sorted[index]);
            }

            for (int i = 0; i < sorted.Length; i++)
            {
                if (chosen.Contains(i))
                    continue;

                sorted[i].gameObject.SetActive(false);
            }
        }

        // Gọi bởi StarFragment khi Player thu thập. StarFragment tự đảm bảo chỉ gọi đúng 1 lần.
        public void RegisterCollected()
        {
            CollectedStars++;
            OnCollectedChanged?.Invoke(CollectedStars, TotalStars);
        }
    }
}
