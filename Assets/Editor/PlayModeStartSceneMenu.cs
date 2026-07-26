using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StarSower.EditorTools
{
    // CÔNG CỤ DEV — chỉ chạy trong Editor, không vào build (nằm trong thư mục Assets/Editor).
    //
    // Vấn đề: bấm Play trong Unity luôn chạy scene ĐANG MỞ. Thứ tự trong Build Settings chỉ có
    // tác dụng cho bản build thật, nên không thể dùng nó để chọn màn test.
    //
    // playModeStartScene là cơ chế chính thức của Unity cho việc này: ép Play luôn bắt đầu từ một
    // scene chỉ định, bất kể đang mở scene nào. Chọn màn ở menu StarSower > Play Mode Start Scene.
    [InitializeOnLoad]
    public static class PlayModeStartSceneMenu
    {
        // Sửa danh sách này là đổi được lựa chọn. Chỉ số ở comment trùng với thứ tự menu.
        private static readonly string[] Scenes =
        {
            "Assets/Scenes/SampleScene.unity", // 0 - Forgotten Forest
            "Assets/Scenes/Level_02.unity",    // 1 - Cloud Garden
            "Assets/Scenes/Level_03.unity",    // 2 - Sky Ruins
            "Assets/Scenes/Level_04.unity",    // 3 - Aurora Cliffs
            "Assets/Scenes/Level_05.unity",    // 4 - Moon Gate
        };

        private const string PrefKey = "StarSower.PlayModeStartSceneIndex";
        private const string Root = "StarSower/Play Mode Start Scene/";

        // TẠM THỜI ĐANG BẬT SẴN = 4 (Moon Gate) để test S1-018.
        // Trả về -1 là quay lại hành vi mặc định của Unity (chạy scene đang mở) — hoặc chọn
        // "Tat (dung scene dang mo)" trong menu, lựa chọn của menu luôn thắng giá trị mặc định này.
        private const int DefaultIndex = 4;

        private static int Index
        {
            get => EditorPrefs.GetInt(PrefKey, DefaultIndex);
            set { EditorPrefs.SetInt(PrefKey, value); Apply(); }
        }

        static PlayModeStartSceneMenu() => EditorApplication.delayCall += Apply;

        private static void Apply()
        {
            int i = Index;
            if (i < 0 || i >= Scenes.Length)
            {
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(Scenes[i]);
            if (asset == null)
            {
                Debug.LogWarning($"[PlayModeStartScene] Khong tim thay {Scenes[i]} — dung scene dang mo.");
                EditorSceneManager.playModeStartScene = null;
                return;
            }

            EditorSceneManager.playModeStartScene = asset;
            Debug.Log($"[PlayModeStartScene] Play se bat dau tu: {asset.name}");
        }

        [MenuItem(Root + "0 - Forgotten Forest")] private static void S0() => Index = 0;
        [MenuItem(Root + "1 - Cloud Garden")]     private static void S1() => Index = 1;
        [MenuItem(Root + "2 - Sky Ruins")]        private static void S2() => Index = 2;
        [MenuItem(Root + "3 - Aurora Cliffs")]    private static void S3() => Index = 3;
        [MenuItem(Root + "4 - Moon Gate")]        private static void S4() => Index = 4;

        [MenuItem(Root + "Tat (dung scene dang mo)", priority = 100)]
        private static void Off() => Index = -1;

        [MenuItem(Root + "0 - Forgotten Forest", true)] private static bool V0() { Menu.SetChecked(Root + "0 - Forgotten Forest", Index == 0); return true; }
        [MenuItem(Root + "1 - Cloud Garden", true)]     private static bool V1() { Menu.SetChecked(Root + "1 - Cloud Garden", Index == 1); return true; }
        [MenuItem(Root + "2 - Sky Ruins", true)]        private static bool V2() { Menu.SetChecked(Root + "2 - Sky Ruins", Index == 2); return true; }
        [MenuItem(Root + "3 - Aurora Cliffs", true)]    private static bool V3() { Menu.SetChecked(Root + "3 - Aurora Cliffs", Index == 3); return true; }
        [MenuItem(Root + "4 - Moon Gate", true)]        private static bool V4() { Menu.SetChecked(Root + "4 - Moon Gate", Index == 4); return true; }
        [MenuItem(Root + "Tat (dung scene dang mo)", true)] private static bool VOff() { Menu.SetChecked(Root + "Tat (dung scene dang mo)", Index < 0); return true; }
    }
}
