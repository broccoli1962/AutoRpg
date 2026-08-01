#if UNITY_EDITOR
using Backend.Meta.StoreCompliance;
using Backend.Object.UI.StoreCompliance;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.StoreCompliance.Editor
{
    /// <summary>
    /// StoreCompliancePanel 프리팹·StoreComplianceConfig 에셋을 생성하고 Addressables에 등록한다.
    /// </summary>
    public static class StoreComplianceUIPrefabCreator
    {
        private const string PanelPath = "Assets/GameResource/Prefabs/UI/StoreCompliancePanel.prefab";
        private const string ConfigPath = "Assets/GameResource/Data/StoreCompliance/StoreComplianceConfig.asset";
        private const string PanelAddress = "UI/StoreCompliancePanel.prefab";
        private const string ConfigAddress = "StoreCompliance/StoreComplianceConfig.asset";

        [MenuItem("Tools/Abyss Chronicle/Ensure Store Compliance Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Prefabs/UI");
            EnsureFolder("Assets/GameResource/Data/StoreCompliance");

            EnsureConfigAsset();
            var root = BuildPanel();
            PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
            UnityEngine.Object.DestroyImmediate(root);

            EnsureAddressable(PanelPath, PanelAddress, "UI");
            EnsureAddressable(ConfigPath, ConfigAddress, "Backend");
            AssetDatabase.SaveAssets();
        }

        private static void EnsureConfigAsset()
        {
            var config = AssetDatabase.LoadAssetAtPath<StoreComplianceConfig>(ConfigPath);
            if (config != null)
                return;

            config = ScriptableObject.CreateInstance<StoreComplianceConfig>();
            config.ApplySpecDefaults();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }

        private static GameObject BuildPanel()
        {
            var root = CreateRect("StoreCompliancePanel", null);
            root.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            var panel = root.AddComponent<StoreCompliancePanel>();

            var title = CreateText(root.transform, "Title", 36, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(0f, -24f));

            var scrollGo = CreateRect("Scroll", root.transform);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(24f, 120f);
            scrollRect.offsetMax = new Vector2(-24f, -140f);

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;

            var viewport = CreateRect("Viewport", scrollGo.transform);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = Color.white;
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateRect("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            scroll.content = contentRect;

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var teenNotice = CreateNoticeText(content.transform, "TeenPaymentNotice");
            var notChildren = CreateNoticeText(content.transform, "NotChildrenNotice");
            var adConsent = CreateNoticeText(content.transform, "AdConsentStatus");

            var privacy = CreateActionButton(content.transform, "Btn_Privacy");
            var terms = CreateActionButton(content.transform, "Btn_Terms");
            var accountDelete = CreateActionButton(content.transform, "Btn_AccountDelete");
            var gachaRate = CreateActionButton(content.transform, "Btn_GachaRate");
            var adConsentBtn = CreateActionButton(content.transform, "Btn_AdConsent");

            var close = CreateActionButton(root.transform, "Btn_Close", 200f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100f, 32f), new Vector2(100f, 104f));

            var so = new SerializedObject(panel);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_teenPaymentNoticeText").objectReferenceValue = teenNotice;
            so.FindProperty("_notChildrenNoticeText").objectReferenceValue = notChildren;
            so.FindProperty("_adConsentStatusText").objectReferenceValue = adConsent;
            so.FindProperty("_privacyButton").objectReferenceValue = privacy.GetComponent<CommonButton>();
            so.FindProperty("_termsButton").objectReferenceValue = terms.GetComponent<CommonButton>();
            so.FindProperty("_accountDeleteButton").objectReferenceValue = accountDelete.GetComponent<CommonButton>();
            so.FindProperty("_gachaRateButton").objectReferenceValue = gachaRate.GetComponent<CommonButton>();
            so.FindProperty("_adConsentButton").objectReferenceValue = adConsentBtn.GetComponent<CommonButton>();
            so.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<CommonButton>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static Text CreateNoticeText(Transform parent, string name)
        {
            var text = CreateText(parent, name, 22, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var layout = text.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 48f;
            return text;
        }

        private static GameObject CreateActionButton(
            Transform parent,
            string name,
            float width = 0f,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null,
            Vector2? offsetMin = null,
            Vector2? offsetMax = null)
        {
            var go = CreateRect(name, parent);
            var rect = go.GetComponent<RectTransform>();

            if (anchorMin.HasValue)
            {
                rect.anchorMin = anchorMin.Value;
                rect.anchorMax = anchorMax ?? anchorMin.Value;
                rect.offsetMin = offsetMin ?? Vector2.zero;
                rect.offsetMax = offsetMax ?? Vector2.zero;
            }
            else
            {
                var layout = go.AddComponent<LayoutElement>();
                layout.minHeight = 88f;
                if (width > 0f)
                    layout.preferredWidth = width;
            }

            go.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.34f, 1f);
            go.AddComponent<TouchTargetSize>().Apply();

            var labelGo = CreateRect("Label", go.transform);
            var text = labelGo.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 24;
            text.raycastTarget = false;

            go.AddComponent<CommonButton>();
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
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = false;
            return text;
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

        private static void EnsureAddressable(string assetPath, string address, string groupName)
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
                if (candidate != null && candidate.Name == groupName)
                {
                    group = candidate;
                    break;
                }
            }

            if (group == null)
                return;

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }
    }
}
#endif
