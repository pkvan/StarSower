using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using StarSower.Cinematic;

namespace StarSower.EditorTools
{
    // CÔNG CỤ DEV — chỉ chạy trong Editor, không vào build (nằm trong thư mục Assets/Editor).
    //
    // Cảnh kết Chapter 1 chỉ chiếu sau khi leo hết cả 5 khu vực, nên chỉnh một con số rồi xem lại
    // là chuyện của mười lăm phút. Menu này gọi thẳng JourneyCinematic.Play().
    //
    // Hai mục:
    //   - "Chay thu ngay" : đang ở Play Mode thì chiếu luôn trên scene đang chạy.
    //   - "Vao Play va chay": chưa Play thì ép Play Mode bắt đầu từ Moon Gate rồi tự chiếu.
    //
    // Không sửa JourneyCinematic.cs cho việc này: nó đã có sẵn Play() công khai, gọi từ ngoài là đủ.
    public static class JourneyCinematicMenu
    {
        private const string Root = "StarSower/Canh ket Chapter 1/";
        private const string MoonGate = "Assets/Scenes/Level_05.unity";
        private const string PendingKey = "StarSower.JourneyCinematic.PlayOnEnterPlayMode";

        [MenuItem(Root + "Chay thu ngay (dang Play)", priority = 200)]
        private static void PlayNow()
        {
            JourneyCinematic cinematic = Find();
            if (cinematic == null)
            {
                EditorUtility.DisplayDialog(
                    "Khong tim thay JourneyCinematic",
                    "Scene dang chay khong co JourneyCinematic. No CHI nam trong Level_05 " +
                    "(Moon Gate) — dung muc \"Vao Play va chay\" de tu mo dung scene do.",
                    "OK");
                return;
            }

            cinematic.StartCoroutine(cinematic.Play());
        }

        [MenuItem(Root + "Chay thu ngay (dang Play)", validate = true)]
        private static bool PlayNowValidate() => EditorApplication.isPlaying;

        [MenuItem(Root + "Vao Play va chay (Moon Gate)", priority = 201)]
        private static void EnterPlayAndRun()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MoonGate);
            if (asset == null)
            {
                Debug.LogError($"[CanhKet] Khong tim thay {MoonGate}.");
                return;
            }

            // Ép Play bắt đầu từ Moon Gate bất kể đang mở scene nào — cùng cơ chế
            // PlayModeStartSceneMenu đang dùng.
            EditorSceneManager.playModeStartScene = asset;

            // Đánh dấu để chạy SAU khi scene đã nạp xong. Đặt cờ vào EditorPrefs chứ không giữ
            // biến static: vào Play Mode là domain reload, mọi biến static bị xoá sạch.
            EditorPrefs.SetBool(PendingKey, true);
            EditorApplication.isPlaying = true;
        }

        [MenuItem(Root + "Vao Play va chay (Moon Gate)", validate = true)]
        private static bool EnterPlayValidate() => !EditorApplication.isPlaying;

        [InitializeOnLoadMethod]
        private static void Hook()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            if (!EditorPrefs.GetBool(PendingKey, false))
                return;

            EditorPrefs.SetBool(PendingKey, false);
            EditorApplication.delayCall += RunPending;
        }

        private static void RunPending()
        {
            JourneyCinematic cinematic = Find();
            if (cinematic == null)
            {
                Debug.LogWarning("[CanhKet] Scene da nap nhung khong co JourneyCinematic.");
                return;
            }

            cinematic.StartCoroutine(cinematic.Play());
        }

        // FindObjectsInactive.Include: JourneyCinematic co the dang tat trong scene.
        private static JourneyCinematic Find() =>
            Object.FindObjectsByType<JourneyCinematic>(FindObjectsInactive.Include,
                                                       FindObjectsSortMode.None)
                  .FirstOrDefault();
    }
}
