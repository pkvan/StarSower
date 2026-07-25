using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Level
{
    // Danh sách level của (chapter) game — 1 asset duy nhất, chỉnh trong Inspector, không có số
    // lượng level nào hardcode trong code. LevelManager/ProgressManager/LevelSelectController đều
    // đọc từ đây, không tự giữ danh sách riêng. Thêm level mới = thêm 1 phần tử vào asset này.
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "StarSower/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

        public IReadOnlyList<LevelDefinition> Levels => levels;
        public int Count => levels.Count;

        public LevelDefinition GetById(string levelId)
        {
            foreach (LevelDefinition level in levels)
            {
                if (level.levelId == levelId)
                    return level;
            }
            return null;
        }

        public LevelDefinition GetNext(string currentLevelId)
        {
            for (int i = 0; i < levels.Count - 1; i++)
            {
                if (levels[i].levelId == currentLevelId)
                    return levels[i + 1];
            }
            return null;
        }

        public LevelDefinition GetFirst()
        {
            return levels.Count > 0 ? levels[0] : null;
        }
    }
}
