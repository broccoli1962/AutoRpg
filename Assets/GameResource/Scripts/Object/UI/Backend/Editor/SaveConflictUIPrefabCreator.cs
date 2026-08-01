#if UNITY_EDITOR
using Backend.Object.UI.Backend;
using Backend.Object.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Backend.Editor
{
    /// <summary>
    /// SaveConflictPopup 프리팹 생성 및 Addressables 등록.
    /// </summary>
    public static class SaveConflictUIPrefabCreator
    {
        private const string PopupPath = "Assets/GameResource/Prefabs/UI/SaveConflictPopup.prefab";
        private const string PopupAddress = "UI/SaveConflictPopup.prefab";

        [MenuItem("Tools/Abyss Chronicle/Ensure Backend UI Prefabs")]
        public static void EnsurePrefabs()
        {
            EnsureFolder("Assets/GameResource/Prefabs/UI");

            var root = BuildPopup();
            PrefabUtility.SaveAsPrefabAsset(root, PopupPath);
            UnityEngine.Object.DestroyImmediate(root);

            EnsureAddressable(PopupPath, PopupAddress);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildPopup()
        {
            var root = CreateRect("SaveConflictPopup", null);
            root.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            var popup = root.AddComponent<SaveConflictPopup>();

            var title = CreateText(root.transform, "Title", 34, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(0f, -24f));

            var localInfo = CreateText(root.transform, "LocalInfo", 24, TextAnchor.UpperLeft,
                new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.85f), Vector2.zero, Vector2.zero);

            var cloudInfo = CreateText(root.transform, "CloudInfo", 24, TextAnchor.UpperLeft,
                new Vector2(0.05f, 0.25f), new Vector2(0.95f, 0.55f), Vector2.zero, Vector2.zero);

            var useLocal = CreateTouchButton(root.transform, "Btn_UseLocal", "Use Local Save", 320f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-170f, 120f), new Vector2(170f, 200f));

            var useCloud = CreateTouchButton(root.transform, "Btn_UseCloud", "Use Cloud Save", 320f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-170f, 32f), new Vector2(170f, 104f));

            var so = new SerializedObject(popup);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_localInfoText").objectReferenceValue = localInfo;
            so.FindProperty("_cloudInfoText").objectReferenceValue = cloudInfo;
            so.FindProperty("_useLocalButton").objectReferenceValue = useLocal.GetComponent<CommonButton>();
            so.FindProperty("_useCloudButton").objectReferenceValue = useCloud.GetComponent<CommonButton>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var slashIndex = path.LastIndexOf('/');
                var parent = path.Substring(0, slashIndex);
                var name = path.Substring(slashIndex + 1);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void EnsureAddressable(string assetPath, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var group = settings.FindGroup("UI");
            if (group == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
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

        private static Text CreateText(
            Transform parent,
            string name,
            int fontSize,
            TextAnchor anchor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.supportRichText = false;
            return text;
        }

        private static GameObject CreateTouchButton(
            Transform parent,
            string name,
            string label,
            float width,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            go.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.34f, 1f);
            go.AddComponent<CommonButton>();

            var textGo = CreateRect("Label", go.transform);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;

            return go;
        }
    }
}
#endif
