using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarSower.FX;

namespace StarSower.EditorTools
{
    // Tam thoi. Xoa sau khi xong S2-005.
    public static class S2_005_Check
    {
        [InitializeOnLoadMethod]
        private static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.delayCall += () =>
            {
                try { Check(); }
                catch (System.Exception e) { Debug.Log("===== S2-005 CHECK =====\nTOANG: " + e); }
            };
        }

        private static void Check()
        {
            string o = "===== S2-005 CHECK =====\n";

            var frag = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StarFragment.prefab");
            if (frag == null) o += "[FAIL] khong nap duoc StarFragment.prefab\n";
            else
            {
                o += $"[INFO] StarFragment.prefab scale={frag.transform.localScale} " +
                     $"con={frag.transform.childCount}\n";
                foreach (var sr in frag.GetComponentsInChildren<SpriteRenderer>(true))
                    o += $"   {sr.gameObject.name}: enabled={sr.enabled} active={sr.gameObject.activeSelf} " +
                         $"sprite={(sr.sprite != null ? sr.sprite.name : "NULL")} " +
                         $"mat={(sr.sharedMaterial != null ? sr.sharedMaterial.name : "NULL")} " +
                         $"shader={(sr.sharedMaterial != null && sr.sharedMaterial.shader != null ? sr.sharedMaterial.shader.name : "NULL")} " +
                         $"order={sr.sortingOrder} scaleW={sr.transform.lossyScale.x:F3} " +
                         $"boundsW={sr.bounds.size.x:F3}x{sr.bounds.size.y:F3}\n";
                var ce = frag.GetComponent<StarCollectEffect>();
                o += $"[{(ce != null ? "PASS" : "FAIL")}] co StarCollectEffect\n";
                if (ce != null)
                {
                    var so = new SerializedObject(ce);
                    SerializedProperty arr = so.FindProperty("collectSounds");
                    int n = arr != null ? arr.arraySize : -1;
                    string names = "";
                    for (int i = 0; i < n; i++)
                    {
                        var c = arr.GetArrayElementAtIndex(i).objectReferenceValue as AudioClip;
                        names += (c != null ? $" {c.name}({c.length:F2}s,{c.loadType})" : " NULL");
                    }
                    o += $"[{(n == 3 && !names.Contains("NULL") ? "PASS" : "FAIL")}] collectSounds={n}{names}\n";
                    o += $"[INFO] collectVolume={so.FindProperty("collectVolume").floatValue} " +
                         $"sharedSource={(so.FindProperty("sharedSource").objectReferenceValue != null ? "gan san" : "tu lay tu pool")}\n";
                }
                o += $"[{(frag.GetComponent<StarIdleAnimator>() != null ? "PASS" : "FAIL")}] co StarIdleAnimator\n";
            }

            foreach (string path in new[]
            {
                "Assets/Scenes/SampleScene.unity", "Assets/Scenes/Level_02.unity",
                "Assets/Scenes/Level_03.unity", "Assets/Scenes/Level_04.unity",
                "Assets/Scenes/Level_05.unity",
            })
            {
                Scene sc = default;
                try
                {
                    sc = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    int frags = 0, missingPrefab = 0, visible = 0;
                    string first = "";
                    StarFXPool pool = null; PocketFXController pocket = null; AudioSource poolAudio = null;

                    foreach (GameObject root in sc.GetRootGameObjects())
                    {
                        if (pool == null) pool = root.GetComponentInChildren<StarFXPool>(true);
                        if (pool != null && poolAudio == null) poolAudio = pool.GetComponent<AudioSource>();
                        if (pocket == null) pocket = root.GetComponentInChildren<PocketFXController>(true);

                        foreach (var f in root.GetComponentsInChildren<StarSower.Collectibles.StarFragment>(true))
                        {
                            frags++;
                            if (PrefabUtility.IsPrefabAssetMissing(f.gameObject)) missingPrefab++;
                            var srs = f.GetComponentsInChildren<SpriteRenderer>(true);
                            bool vis = false;
                            foreach (var sr in srs)
                                if (sr.enabled && sr.sprite != null) vis = true;
                            if (vis) visible++;
                            if (first == "")
                                first = $" | {f.name} pos={f.transform.position} srCount={srs.Length}" +
                                        (srs.Length > 0 ? $" sprite0={(srs[0].sprite != null ? srs[0].sprite.name : "NULL")}" : "");
                        }
                    }
                    o += $"[{(frags > 0 && missingPrefab == 0 && visible == frags ? "PASS" : "FAIL")}] " +
                         $"{System.IO.Path.GetFileName(path),-20} manh={frags} prefabHong={missingPrefab} " +
                         $"coHinh={visible} pool={(pool != null ? "co" : "THIEU")} " +
                         $"pocket={(pocket != null ? "co" : "THIEU")} " +
                         $"audio={(poolAudio != null ? "co" : "THIEU")}{first}\n";
                }
                catch (System.Exception e) { o += $"[FAIL] {path}: {e.Message}\n"; }
                finally { if (sc.IsValid()) EditorSceneManager.CloseScene(sc, true); }
            }
            Debug.Log(o);
        }
    }
}
