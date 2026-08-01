#if UNITY_EDITOR
using Backend.Object.UI.Gacha;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Gacha.Editor
{
    /// <summary>
    /// GachaSummonPanel·GachaRateDisclosurePopup 프리팹을 생성하고 Addressables UI 그룹에 등록한다.
    /// </summary>
    public static class GachaUIPrefabCreator
    {
        private const string SummonPanelPath = "Assets/GameResource/Prefabs/UI/GachaSummonPanel.prefab";
        private const string RatePopupPath = "Assets/GameResource/Prefabs/UI/GachaRateDisclosurePopup.prefab";
        private const string SummonAddress = "UI/GachaSummonPanel.prefab";
        private const string RateAddress = "UI/GachaRateDisclosurePopup.prefab";

        [MenuItem("Tools/Abyss Chronicle/Ensure Gacha UI Prefabs")]
        public static void EnsurePrefabs()
        {
            EnsureFolder("Assets/GameResource/Prefabs/UI");

            var summonRoot = BuildSummonPanel();
            PrefabUtility.SaveAsPrefabAsset(summonRoot, SummonPanelPath);
            UnityEngine.Object.DestroyImmediate(summonRoot);

            var rateRoot = BuildRatePopup();
            PrefabUtility.SaveAsPrefabAsset(rateRoot, RatePopupPath);
            UnityEngine.Object.DestroyImmediate(rateRoot);

            EnsureAddressable(SummonPanelPath, SummonAddress);
            EnsureAddressable(RatePopupPath, RateAddress);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildSummonPanel()
        {
            var root = CreateRect("GachaSummonPanel", null);
            root.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
            var panel = root.AddComponent<GachaSummonPanel>();

            var title = CreateText(root.transform, "Title", 36, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -120f), new Vector2(0f, -24f));

            var rateLink = CreateTouchButton(root.transform, "Btn_RateInfo", "확률 정보", 320f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 280f), new Vector2(0f, 360f));

            var actionBar = CreateRect("ActionBar", root.transform);
            var actionRect = actionBar.GetComponent<RectTransform>();
            actionRect.anchorMin = new Vector2(0f, 0f);
            actionRect.anchorMax = new Vector2(1f, 0f);
            actionRect.offsetMin = new Vector2(24f, 120f);
            actionRect.offsetMax = new Vector2(-24f, 240f);

            var layout = actionBar.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var single = CreateTouchButton(actionBar.transform, "Btn_SingleSummon", "1회 소환", 0f);
            var ten = CreateTouchButton(actionBar.transform, "Btn_TenSummon", "10회 소환", 0f);

            var close = CreateTouchButton(root.transform, "Btn_Close", "닫기", 200f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100f, 32f), new Vector2(100f, 104f));

            var so = new SerializedObject(panel);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_rateInfoButton").objectReferenceValue = rateLink.GetComponent<CommonButton>();
            so.FindProperty("_singleSummonButton").objectReferenceValue = single.GetComponent<CommonButton>();
            so.FindProperty("_tenSummonButton").objectReferenceValue = ten.GetComponent<CommonButton>();
            so.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<CommonButton>();
            so.FindProperty("_rateInfoLabel").objectReferenceValue = rateLink.GetComponentInChildren<Text>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static GameObject BuildRatePopup()
        {
            var root = CreateRect("GachaRateDisclosurePopup", null);
            root.AddComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.96f);
            var popup = root.AddComponent<GachaRateDisclosurePopup>();

            var panel = CreateRect("Panel", root.transform);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.12f, 0.16f, 1f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.08f);
            panelRect.anchorMax = new Vector2(0.95f, 0.92f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var title = CreateText(panel.transform, "Title", 32, TextAnchor.UpperCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -72f), new Vector2(-24f, -16f));

            var scrollGo = CreateRect("Scroll", panel.transform);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(24f, 96f);
            scrollRect.offsetMax = new Vector2(-24f, -88f);

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

            var vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 12f;
            vLayout.padding = new RectOffset(8, 8, 8, 8);
            vLayout.childControlHeight = true;
            vLayout.childControlWidth = true;
            vLayout.childForceExpandHeight = false;
            vLayout.childForceExpandWidth = true;
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var gradeTitle = CreateSectionTitle(content.transform, "GradeSectionTitle");
            var gradeContent = CreateSectionBody(content.transform, "GradeContent");
            var itemTitle = CreateSectionTitle(content.transform, "ItemSectionTitle");
            var itemContent = CreateSectionBody(content.transform, "ItemContent");
            var pityTitle = CreateSectionTitle(content.transform, "PitySectionTitle");
            var pityContent = CreateSectionBody(content.transform, "PityContent");
            var tenPull = CreateSectionBody(content.transform, "TenPullContent");

            var close = CreateTouchButton(panel.transform, "Btn_Close", "닫기", 200f,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100f, 16f), new Vector2(100f, 80f));

            var so = new SerializedObject(popup);
            so.FindProperty("_titleText").objectReferenceValue = title;
            so.FindProperty("_gradeSectionTitle").objectReferenceValue = gradeTitle;
            so.FindProperty("_gradeContent").objectReferenceValue = gradeContent;
            so.FindProperty("_itemSectionTitle").objectReferenceValue = itemTitle;
            so.FindProperty("_itemContent").objectReferenceValue = itemContent;
            so.FindProperty("_pitySectionTitle").objectReferenceValue = pityTitle;
            so.FindProperty("_pityContent").objectReferenceValue = pityContent;
            so.FindProperty("_tenPullContent").objectReferenceValue = tenPull;
            so.FindProperty("_closeButton").objectReferenceValue = close.GetComponent<CommonButton>();
            so.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static Text CreateSectionTitle(Transform parent, string name)
        {
            return CreateText(parent, name, 26, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, bold: true);
        }

        private static Text CreateSectionBody(Transform parent, string name)
        {
            var text = CreateText(parent, name, 22, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var layout = text.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 48f;
            return text;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            bool bold = false)
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
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.supportRichText = false;
            return text;
        }

        private static GameObject CreateTouchButton(
            Transform parent,
            string name,
            string label,
            float width,
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
            else if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, TouchTargetSize.MinButtonHeight);
            }

            go.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.34f, 1f);
            go.AddComponent<TouchTargetSize>().Apply();

            var labelGo = CreateRect("Label", go.transform);
            var text = labelGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 24;
            text.raycastTarget = false;

            go.AddComponent<CommonButton>();
            return go;
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

        private static void EnsureAddressable(string assetPath, string address)
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
            entry.address = address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }
    }
}
#endif
