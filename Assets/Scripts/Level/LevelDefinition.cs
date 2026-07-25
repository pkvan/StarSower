using System;

namespace StarSower.Level
{
    // Khai báo 1 level: id ổn định (dùng để lưu tiến trình), tên hiển thị, tên scene để load.
    // Plain data, sống bên trong LevelDatabase — không phải ScriptableObject riêng vì không cần
    // tham chiếu độc lập từ đâu khác ngoài Database.
    [Serializable]
    public class LevelDefinition
    {
        public string levelId;
        public string displayName;
        public string sceneName;
    }
}
