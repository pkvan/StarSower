using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Constellations
{
    // Danh sách toàn bộ chapter của game — để ChapterProgressManager tra ra ChapterData từ chapterId
    // của level đang chơi mà không cần khai báo lại ở từng scene. Cùng vai trò với LevelDatabase.
    [CreateAssetMenu(fileName = "ChapterDatabase", menuName = "StarSower/Chapter Database")]
    public class ChapterDatabase : ScriptableObject
    {
        [SerializeField] private List<ChapterData> chapters = new List<ChapterData>();

        public IReadOnlyList<ChapterData> Chapters => chapters;

        public ChapterData GetById(string chapterId)
        {
            foreach (ChapterData chapter in chapters)
            {
                if (chapter != null && chapter.ChapterId == chapterId)
                    return chapter;
            }
            return null;
        }
    }
}
