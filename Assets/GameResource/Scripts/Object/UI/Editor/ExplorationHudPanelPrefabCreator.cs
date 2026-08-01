#if UNITY_EDITOR
using Backend.Object.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Editor
{
    /// <summary>
    /// ExplorationHudPanel 프리팹을 생성·등록한다. 터치 버튼 최소 높이 88px 를 보장한다.
    /// </summary>
    public static class ExplorationHudPanelPrefabCreator
    {
        private const string PrefabPath = "Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab";
        private const string Address = "UI/ExplorationHudPanel.prefab";

        [MenuItem("Tools/Abyss Chronicle/Ensure Exploration HUD Prefab")]
        public static void EnsurePrefab()
        {
            EnsureFolder("Assets/GameResource/Prefabs/UI");

            var root = BuildHierarchy();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            EnsureAddressable(PrefabPath);
            Selection.activeObject = prefab;
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildHierarchy()
        {
            var root = CreateRect("ExplorationHudPanel", null);
            root.AddComponent<ExplorationHudPanel>();

            var topBar = CreateBand(root.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -128f), new Vector2(0f, 0f));

            var stageArea = CreateBand(root.transform, "StageArea", new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(0f, 320f), new Vector2(0f, -128f));

            var logStrip = CreateBand(root.transform, "LogStrip", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 128f), new Vector2(0f, 192f));

            var actionBar = CreateBand(root.transform, "ActionBar", new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, new Vector2(0f, 128f));

            var actionLayout = CreateHorizontalLayout(actionBar);
            var dispatch = CreateTouchButton(actionLayout.transform, "Btn_Dispatch", string.Empty, 240f);
            var enhance = CreateTouchButton(actionLayout.transform, "Btn_Enhance", string.Empty, 240f);
            var summon = CreateTouchButton(actionLayout.transform, "Btn_Summon", string.Empty, 240f);
            var ret = CreateTouchButton(actionLayout.transform, "Btn_Return", string.Empty, 240f);

            var panel = root.GetComponent<ExplorationHudPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("_dispatchButton").objectReferenceValue = dispatch;
            so.FindProperty("_enhanceButton").objectReferenceValue = enhance;
            so.FindProperty("_summonButton").objectReferenceValue = summon;
            so.FindProperty("_returnButton").objectReferenceValue = ret;
            so.ApplyModifiedPropertiesWithoutUndo();

            CreateRect("StagePlaceholder", stageArea.transform).AddComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 1f);
            CreateRect("LogPlaceholder", logStrip.transform).AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.92f);
            CreateRect("TopPlaceholder", topBar.transform).AddComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 1f);

            return root;
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
                go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateBand(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return go;
        }

        private static GameObject CreateHorizontalLayout(GameObject parent)
        {
            var layoutGo = CreateRect("ActionButtons", parent.transform);
            var layout = layoutGo.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 16f;
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return layoutGo;
        }

        private static CommonButton CreateTouchButton(Transform parent, string name, string label, float width)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, TouchTargetSize.MinButtonHeight);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.34f, 1f);

            var touch = go.AddComponent<TouchTargetSize>();

            var labelGo = CreateRect("Label", go.transform);
            var text = labelGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 28;
            text.raycastTarget = false;

            var button = go.AddComponent<CommonButton>();
            touch.Apply();
            return button;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void EnsureAddressable(string assetPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            AddressableAssetGroup group = null;
            foreach (var candidate in settings.groups)
            {
                if (candidate != null && candidate.Name == "UI")
                {
                    group = candidate;
                    break;
                }
            }

            if (group == null)
                return;

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = Address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }
    }
}
#endif
