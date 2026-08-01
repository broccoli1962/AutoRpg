#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Offline.Editor
{
    /// <summary>
    /// OfflineReturnSummaryPopup 프리팹을 생성하고 Addressables UI 그룹에 등록한다.
    /// </summary>
    public static class OfflineReturnSummaryPrefabCreator
    {
        private const string PopupPath = "Assets/GameResource/Prefabs/UI/OfflineReturnSummaryPopup.prefab";
        private const string PopupAddress = "UI/OfflineReturnSummaryPopup.prefab";

        [MenuItem("Tools/Abyss Chronicle/Ensure Offline Return Summary Popup")]
        public static void EnsurePrefab()
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
            var root = CreateRect("OfflineReturnSummaryPopup", null);
            root.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.96f);
            var popup = root.AddComponent<OfflineReturnSummaryPopup>();

            var panel = CreateRect("Panel", root.transform);
            panel.AddComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 1f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.06f, 0.12f);
            panelRect.anchorMax = new Vector2(0.94f, 0.88f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var title = CreateText(panel.transform, "Title", 34, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -80f), new Vector2(-24f, -20f));
            var elapsed = CreateText(panel.transform, "Elapsed", 24, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -130f), new Vector2(-24f, -90f));

            var resourcesTitle = CreateText(panel.transform, "ResourcesTitle", 26, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -190f), new Vector2(-32f, -150f));
            var resourcesContent = CreateText(panel.transform, "ResourcesContent", 22, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(48f, -250f), new Vector2(-32f, -190f));

            var highlightsTitle = CreateText(panel.transform, "HighlightsTitle", 26, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -300f), new Vector2(-32f, -260f));
            var highlightsContent = CreateText(panel.transform, "HighlightsContent", 20, TextAnchor.UpperLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(48f, 120f), new Vector2(-32f, -320f));
            highlightsContent.alignment = TextAnchor.UpperLeft;

            var confirm = CreateTouchButton(panel.transform, "Btn_Confirm", "확인", 240f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 24f), new Vector2(120f, 96f));

            var so = new SerializedObject(popup);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_elapsedText").objectReferenceValue = elapsed;
            so.FindProperty("_resourcesSectionTitle").objectReferenceValue = resourcesTitle;
            so.FindProperty("_resourcesContent").objectReferenceValue = resourcesContent;
            so.FindProperty("_highlightsSectionTitle").objectReferenceValue = highlightsTitle;
            so.FindProperty("_highlightsContent").objectReferenceValue = highlightsContent;
            so.FindProperty("_confirmButton").objectReferenceValue = confirm.GetComponent<CommonButton>();
            so.ApplyModifiedPropertiesWithoutUndo();

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
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static GameObject CreateTouchButton(
            Transform parent,
            string name,
            string label,
            float width,
            Vector2 anchorMin = default,
            Vector2 anchorMax = default,
            Vector2 offsetMin = default,
            Vector2 offsetMax = default)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();
            if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, 72f);
            }
            else
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }

            go.AddComponent<Image>().color = new Color(0.18f, 0.28f, 0.42f, 1f);
            var button = go.AddComponent<CommonButton>();

            var labelGo = CreateRect("Label", go.transform);
            var labelText = labelGo.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 24;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;

            return go;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
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
            if (string.IsNullOrEmpty(guid))
                return;

            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = address;
            EditorUtility.SetDirty(settings);
        }
    }
}
#endif
