using System.IO;
using UnityEngine;

namespace StarSower.Persistence
{
    // Đọc/ghi SaveData xuống đĩa (JSON, Application.persistentDataPath). KHÔNG biết gì về
    // level/star/progression — chỉ là I/O thuần, giống vai trò GameEvents cho event nhưng cho
    // persistence. ProgressManager là nơi DUY NHẤT gọi class này, không ai khác nên đụng thẳng.
    public static class SaveManager
    {
        private const string SaveFileName = "starsower_save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // Trả về null nếu chưa từng lưu (lần chơi đầu tiên) — bên gọi tự quyết định giá trị mặc định.
        public static SaveData Load()
        {
            if (!File.Exists(SavePath))
                return null;

            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
    }
}
