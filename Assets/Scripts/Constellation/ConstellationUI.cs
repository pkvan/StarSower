using UnityEngine;
using UnityEngine.UI;

namespace StarSower.Constellations
{
    // UI nhỏ gọn hiện tiến trình khôi phục bầu trời: "★★☆  12 / 53". Không popup, không che gameplay
    // — chỉ một dòng chữ ở góc HUD. Thuần hiển thị: không tính toán, không lưu, chỉ nghe
    // ChapterProgressManager rồi đổi chữ.
    public class ConstellationUI : MonoBehaviour
    {
        [SerializeField] private ChapterProgressManager chapterProgress;
        [SerializeField] private Text progressLabel;

        [Tooltip("{0} = biểu tượng chòm sao, {1} = fragment đã có, {2} = tổng fragment của chapter.")]
        [SerializeField] private string format = "{0}  {1} / {2}";

        [SerializeField] private char restoredSymbol = '★';
        [SerializeField] private char pendingSymbol = '☆';

        private void OnEnable()
        {
            chapterProgress.OnFragmentsChanged += HandleFragmentsChanged;
            chapterProgress.OnCheckpointReached += HandleCheckpointReached;
        }

        private void OnDisable()
        {
            chapterProgress.OnFragmentsChanged -= HandleFragmentsChanged;
            chapterProgress.OnCheckpointReached -= HandleCheckpointReached;
        }

        private void HandleCheckpointReached(ConstellationData constellation)
        {
            Refresh(chapterProgress.FragmentsCollected, chapterProgress.TotalFragments);
        }

        private void HandleFragmentsChanged(int collected, int total)
        {
            Refresh(collected, total);
        }

        private void Refresh(int collected, int total)
        {
            progressLabel.text = string.Format(format, BuildSymbols(), collected, total);
        }

        // Số ký hiệu bằng đúng số chòm sao của chapter — không hardcode 3 ngôi sao.
        private string BuildSymbols()
        {
            if (chapterProgress.Chapter == null)
                return string.Empty;

            int total = chapterProgress.Chapter.Constellations.Count;
            int restored = Mathf.Clamp(chapterProgress.RestoredCount, 0, total);
            return new string(restoredSymbol, restored) + new string(pendingSymbol, total - restored);
        }
    }
}
