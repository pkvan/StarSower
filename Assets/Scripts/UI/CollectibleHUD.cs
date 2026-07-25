using UnityEngine;
using UnityEngine.UI;
using StarSower.Collectibles;

namespace StarSower.UI
{
    // Hiện "⭐ Collected / Total" trên HUD, tự cập nhật qua CollectibleManager.OnCollectedChanged —
    // không tự đếm Star, không biết Goal/Level tồn tại. Đọc giá trị hiện tại ngay khi bật (OnEnable)
    // thay vì chỉ dựa vào event, để không phụ thuộc thứ tự Start() giữa các script.
    public class CollectibleHUD : MonoBehaviour
    {
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private Text starsLabel;

        private void Awake()
        {
            if (collectibleManager == null)
                collectibleManager = FindFirstObjectByType<CollectibleManager>();
        }

        private void OnEnable()
        {
            UpdateDisplay(collectibleManager.CollectedStars, collectibleManager.TotalStars);
            collectibleManager.OnCollectedChanged += UpdateDisplay;
        }

        private void OnDisable()
        {
            collectibleManager.OnCollectedChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int collected, int total)
        {
            starsLabel.text = $"⭐ {collected} / {total}";
        }
    }
}
