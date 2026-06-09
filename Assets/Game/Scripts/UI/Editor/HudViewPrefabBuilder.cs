using System.IO;
using AIGame.ShootEmUp.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AIGame.ShootEmUp.UI.Editor
{
    internal static class HudViewPrefabBuilder
    {
        private const string HudPrefabPath = "Assets/Game/Prefabs/UI/HudView.prefab";

        // [MenuItem("Tools/ShootEmUp/UI/Rebuild HUDView Prefab")]
        public static void RebuildHudPrefab()
        {
            EnsureDirectory(Path.GetDirectoryName(HudPrefabPath));

            var panel = LoadSprite("Assets/Game/Art/UI/UI_Panel_Default.png");
            var buttonNormal = LoadSprite("Assets/Game/Art/UI/UI_Button_Normal.png");
            var buttonHover = LoadSprite("Assets/Game/Art/UI/UI_Button_Hover.png");
            var buttonPressed = LoadSprite("Assets/Game/Art/UI/UI_Button_Pressed.png");
            var bossFrame = LoadSprite("Assets/Game/Art/UI/UI_BossHealthBar_Frame.png");
            var bossFill = LoadSprite("Assets/Game/Art/UI/UI_BossHealthBar_Fill.png");

            var root = new GameObject("Hud");
            var hud = root.AddComponent<HudView>();
            hud.SetSkinSpritesForEditor(panel, buttonNormal, buttonHover, buttonPressed, bossFrame, bossFill);
            hud.RebuildLayoutForEditor();
            hud.AutoBindReferencesByName();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (prefab == null)
            {
                Debug.LogError("Failed to build HUDView prefab.");
                return;
            }

            Debug.Log($"HUDView prefab rebuilt: {HudPrefabPath}");
        }

        // [MenuItem("Tools/ShootEmUp/UI/Auto Bind Selected HUDView Refs")]
        public static void AutoBindSelectedHudViewRefs()
        {
            var selected = Selection.activeGameObject;
            if (selected != null)
            {
                var sceneHud = selected.GetComponent<HudView>();
                if (sceneHud != null)
                {
                    Undo.RecordObject(sceneHud, "Auto Bind HUDView Refs");
                    sceneHud.AutoBindReferencesByName();
                    EditorUtility.SetDirty(sceneHud);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(sceneHud);
                    Debug.Log($"HUDView refs auto-bound on '{selected.name}'.");
                    return;
                }
            }

            var prefabAsset = Selection.activeObject as GameObject;
            if (prefabAsset == null)
            {
                Debug.LogError("No HUDView target selected. Select a HudView instance or HudView prefab asset.");
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                Debug.LogError("Selected object has no valid asset path.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var prefabHud = root.GetComponent<HudView>();
                if (prefabHud == null)
                {
                    Debug.LogError($"Prefab '{prefabAsset.name}' has no HudView component.");
                    return;
                }

                prefabHud.AutoBindReferencesByName();
                EditorUtility.SetDirty(prefabHud);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"HUDView refs auto-bound on prefab asset '{prefabAsset.name}'.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // [MenuItem("Tools/ShootEmUp/UI/Auto Bind Selected HUDView Refs", true)]
        public static bool ValidateAutoBindSelectedHudViewRefs()
        {
            var selected = Selection.activeGameObject;
            if (selected != null && selected.GetComponent<HudView>() != null)
            {
                return true;
            }

            var prefabAsset = Selection.activeObject as GameObject;
            return prefabAsset != null && prefabAsset.GetComponent<HudView>() != null;
        }

        [DidReloadScripts]
        private static void AutoCreateHudPrefabIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath) != null)
            {
                return;
            }

            RebuildHudPrefab();
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"HUD sprite not found: {path}");
            }

            return sprite;
        }

        private static void EnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
