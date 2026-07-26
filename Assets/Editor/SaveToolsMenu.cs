using System.IO;
using UnityEditor;
using UnityEngine;

namespace StarSower.EditorTools
{
    // CÔNG CỤ DEV — chỉ chạy trong Editor, không vào build (nằm trong Assets/Editor).
    //
    // Lý do tồn tại: tiến trình chòm sao lưu cờ "đã diễn hoạt ảnh" theo từng chòm, mà cờ đó CHỈ
    // được ghi một lần rồi giữ mãi. Khi đang chỉnh hoạt ảnh mà save đã đánh dấu "đã diễn", màn
    // hình sẽ hiện chòm sao hoàn chỉnh tức thì và không có cách nào xem lại nếu không xoá save.
    //
    // Đường dẫn save: Application.persistentDataPath/starsower_save.json
    public static class SaveToolsMenu
    {
        private const string SaveFileName = "starsower_save.json";
        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // Reset file save trong lúc ĐANG Play là vô nghĩa: ProgressManager nạp save vào bộ nhớ ở
        // Awake() và giữ nguyên đối tượng đó cả phiên, nên hoàn thành màn là nó ghi bản CŨ trong
        // bộ nhớ đè lên file vừa dọn. Phải Stop trước, lần Play sau Awake() mới đọc lại từ đĩa.
        private static bool BlockedByPlayMode()
        {
            if (!EditorApplication.isPlaying)
                return false;

            EditorUtility.DisplayDialog(
                "Dang o che do Play",
                "Hay bam Stop truoc roi chay lai muc nay.\n\n" +
                "Ly do: game dang giu mot ban save trong bo nho va se ghi de len file ngay khi " +
                "ban hoan thanh mot man — reset bay gio se bi xoa sach.",
                "Da hieu");
            return true;
        }

        [MenuItem("StarSower/Save/Xoa toan bo save (choi lai tu dau)", priority = 0)]
        private static void DeleteSave()
        {
            if (BlockedByPlayMode())
                return;

            if (!File.Exists(SavePath))
            {
                Debug.Log($"[Save] Khong co file save nao tai:\n{SavePath}");
                return;
            }

            if (!EditorUtility.DisplayDialog("Xoa save?",
                    $"Xoa toan bo tien trinh (level da mo, sao, chom sao)?\n\n{SavePath}",
                    "Xoa", "Huy"))
                return;

            File.Delete(SavePath);
            Debug.Log($"[Save] Da xoa:\n{SavePath}\nVao Play lai la choi tu dau.");
        }

        // Chỉ xoá phần chòm sao, GIỮ NGUYÊN tiến trình level — dùng khi muốn xem lại hoạt ảnh mở
        // khoá mà không phải chơi lại cả chapter.
        [MenuItem("StarSower/Save/Reset rieng chom sao (giu tien trinh level)", priority = 1)]
        private static void ResetConstellations()
        {
            if (BlockedByPlayMode())
                return;

            if (!File.Exists(SavePath))
            {
                Debug.Log($"[Save] Khong co file save nao tai:\n{SavePath}");
                return;
            }

            string json = File.ReadAllText(SavePath);

            // Sửa thẳng trên JSON thay vì nạp qua SaveData: công cụ Editor không nên phụ thuộc vào
            // hình dạng hiện tại của class, để sau này thêm/bớt field không làm hỏng nó.
            string cleaned = System.Text.RegularExpressions.Regex.Replace(
                json, "\"constellations\"\\s*:\\s*\\[[^\\]]*\\]", "\"constellations\":[]");
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned, "\"constellationStarsUnlocked\"\\s*:\\s*\\d+", "\"constellationStarsUnlocked\":0");
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned, "\"constellationStarsAnimated\"\\s*:\\s*\\d+", "\"constellationStarsAnimated\":0");

            File.WriteAllText(SavePath, cleaned);
            Debug.Log("[Save] Da reset chom sao. Tien trinh level giu nguyen — hoan thanh lai " +
                      "mot man bat ky se dien lai hoat anh mo khoa.");
        }

        [MenuItem("StarSower/Save/Mo thu muc save", priority = 20)]
        private static void RevealSave()
        {
            Debug.Log($"[Save] {SavePath}");
            EditorUtility.RevealInFinder(SavePath);
        }
    }
}
