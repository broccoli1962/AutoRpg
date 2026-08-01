#if UNITY_EDITOR
using Backend.Object.UI.Shop;
using Backend.Object.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Shop.Editor
{
    /// <summary>
    /// ShopPanel 프리팹을 생성하고 Addressables UI 그룹에 등록한다.
    /// </summary>
    public static class ShopUIPrefabCreator
    {
        private const string PanelPath = "Assets/GameResource/Prefabs/UI/ShopPanel.prefab";
        private const string PanelAddress = "UI/ShopPanel.prefab";

        [MenuItem("Tools/Abyss Chronicle/Ensure Shop UI Prefabs")]
        public static void EnsurePrefabs()
        {
            EnsureFolder("Assets/GameResource/Prefabs/UI");

            var root = BuildShopPanel();
            PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
            GameObject.DestroyImmediate(root);

            EnsureAddressable(PanelPath, PanelAddress);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildShopPanel()
        {
            var root = CreateRect("ShopPanel", null);
            root.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            var panel = root.AddComponent<ShopPanel>();

            var title = CreateText(root.transform, "Title", 36, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(0f, -24f));

            var listRoot = CreateRect("ProductList", root.transform);
            var listRect = listRoot.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.05f, 0.2f);
            listRect.anchorMax = new Vector2(0.95f, 0.85f);
            listRect.offsetMin = Vector2.zero;
            listRect.offsetMax = Vector2.zero;

            var layout = listRoot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var rowTemplate = CreateText(listRoot.transform, "RowTemplate", 24, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            rowTemplate.gameObject.SetActive(false);

            var restore = CreateTouchButton(root.transform, "Btn_Restore", string.Empty, 320f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-170f, 120f), new Vector2(170f, 200f));

            var close = CreateTouchButton(root.transform, "Btn_Close", string.Empty, 200f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100f, 32f), new Vector2(100f, 104f));

            var so = new SerializedObject(panel);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_productListRoot").objectReferenceValue = listRoot.GetComponent<RectTransform>();
            so.FindProperty("_productRowTemplate").objectReferenceValue = rowTemplate;
            so.FindProperty("_restoreButton").objectReferenceValue = restore.GetComponent<CommonButton>();
            so.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<CommonButton>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var slashIndex = path.LastIndexOf('/');
                var parent = path.Substring(0, slashIndex);
                var child = path.Substring(slashIndex + 1);
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void EnsureAddressable(string assetPath, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var group = settings.DefaultGroup;
            if (group == null)
                return;

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
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
            TextAnchor alignment,
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
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = false;
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

            if (anchorMin != default || anchorMax != default)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }
            else if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, 64f);
            }

            go.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.34f, 1f);
            go.AddComponent<CommonButton>();

            CreateText(go.transform, "Label", 22, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).text = label;

            return go;
        }
    }
}
#endif
