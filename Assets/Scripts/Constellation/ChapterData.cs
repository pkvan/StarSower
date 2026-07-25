using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Constellations
{
    // Cấu hình 1 chapter: tên, tổng số Star Fragment có trong chapter, và danh sách chòm sao theo
    // thứ tự mốc tăng dần. Chapter 1 KHÔNG hardcode ở bất kỳ đâu trong code — thêm chapter mới chỉ
    // là tạo thêm 1 asset ChapterData rồi khai báo chapterId đó ở LevelDatabase.
    [CreateAssetMenu(fileName = "ChapterData", menuName = "StarSower/Chapter Data")]
    public class ChapterData : ScriptableObject
    {
        [Tooltip("Id ổn định, khớp với chapterId khai báo ở từng LevelDefinition trong LevelDatabase.")]
        [SerializeField] private string chapterId;
        [SerializeField] private string chapterName;

        [Tooltip("Tổng Star Fragment có trong toàn chapter — dùng cho UI '12 / 53'.")]
        [SerializeField] private int totalFragments = 53;

        [Tooltip("Các chòm sao của chapter, XẾP THEO MỐC TĂNG DẦN. Mốc lấy từ chính ConstellationData.")]
        [SerializeField] private List<ConstellationData> constellations = new List<ConstellationData>();

        public string ChapterId => chapterId;
        public string ChapterName => chapterName;
        public int TotalFragments => Mathf.Max(1, totalFragments);
        public IReadOnlyList<ConstellationData> Constellations => constellations;

        // Chòm sao có mốc cao nhất — chạm mốc này nghĩa là chapter hoàn thành.
        public ConstellationData FinalConstellation
        {
            get
            {
                ConstellationData highest = null;
                foreach (ConstellationData data in constellations)
                {
                    if (data != null && (highest == null || data.RequiredFragments > highest.RequiredFragments))
                        highest = data;
                }
                return highest;
            }
        }
    }
}
