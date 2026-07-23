using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Util
{
    /// <summary>
    /// Legacy TextAnchor → TMP 정렬·레이아웃 셀 기본 설정.
    /// </summary>
    public static class UiTmpUtil
    {
        private const float LineHeightFactor = 1.28f;

        public static TextAlignmentOptions ToAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.MidlineLeft,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.TopLeft
            };
        }

        public static void ApplyDefaults(
            TextMeshProUGUI text,
            TMP_FontAsset font,
            int fontSize,
            TextAnchor alignment,
            Color? color = null)
        {
            ApplyLayoutCell(text, font, fontSize, alignment, lineCount: 1, color: color);
        }

        /// <summary>LayoutGroup 셀 안에 글자가 칸을 벗어나지 않도록 설정.</summary>
        public static void ApplyLayoutCell(
            TextMeshProUGUI text,
            TMP_FontAsset font,
            int fontSize,
            TextAnchor alignment,
            int lineCount = 1,
            Color? color = null,
            float flexibleWidth = 1f)
        {
            if (text == null)
                return;

            text.font = font ?? RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.alignment = ToAlignment(alignment);
            text.color = color ?? Color.white;
            text.richText = true;
            text.enableAutoSizing = false;
            text.textWrappingMode = lineCount > 1 ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            text.margin = new Vector4(0f, 0f, 0f, 0f);

            var lineHeight = Mathf.Ceil(fontSize * LineHeightFactor);
            var layout = text.GetComponent<LayoutElement>() ?? text.gameObject.AddComponent<LayoutElement>();
            layout.flexibleWidth = flexibleWidth;
            layout.flexibleHeight = 0f;
            layout.minHeight = lineHeight * lineCount;
            if (lineCount > 1)
                layout.preferredHeight = lineHeight * lineCount;
        }

        /// <summary>고정 Rect 안 anchored 텍스트.</summary>
        public static void ApplyAnchoredBox(
            TextMeshProUGUI text,
            TMP_FontAsset font,
            int fontSize,
            TextAnchor alignment,
            Color? color = null)
        {
            if (text == null)
                return;

            text.font = font ?? RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.alignment = ToAlignment(alignment);
            text.color = color ?? Color.white;
            text.richText = true;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;
            text.margin = new Vector4(2f, 2f, 2f, 2f);
        }

        /// <summary>버튼 라벨 — 한 줄 말줄임.</summary>
        public static void ApplyButtonLabel(
            TextMeshProUGUI text,
            TMP_FontAsset font,
            int fontSize,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            if (text == null)
                return;

            text.font = font ?? RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.alignment = ToAlignment(alignment);
            text.richText = false;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
        }

        /// <summary>로그 메시지 — 가로 stretch, 세로는 줄바꿈·텍스트 길이에 맞게 확장.</summary>
        public static void ApplyLogMessageCell(
            TextMeshProUGUI text,
            TMP_FontAsset font,
            int fontSize,
            TextAnchor alignment = TextAnchor.UpperLeft,
            Color? color = null)
        {
            if (text == null)
                return;

            text.font = font ?? RuntimeUiTmpFont.Get();
            text.fontSize = fontSize;
            text.alignment = ToAlignment(alignment);
            text.color = color ?? Color.white;
            text.richText = true;
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.margin = new Vector4(0f, 0f, 0f, 0f);

            var layout = text.GetComponent<LayoutElement>() ?? text.gameObject.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.flexibleHeight = 1f;
            layout.minHeight = LineHeight(fontSize);

            var fitter = text.GetComponent<ContentSizeFitter>() ?? text.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>풀에서 꺼낸 로그 아이템이 Content 폭을 채우도록 anchor·size 재설정.</summary>
        public static void EnsureLogItemStretch(RectTransform itemRoot)
        {
            if (itemRoot == null)
                return;

            itemRoot.anchorMin = new Vector2(0f, 1f);
            itemRoot.anchorMax = new Vector2(1f, 1f);
            itemRoot.pivot = new Vector2(0.5f, 1f);
            itemRoot.sizeDelta = new Vector2(0f, itemRoot.sizeDelta.y);

            var layout = itemRoot.GetComponent<LayoutElement>() ?? itemRoot.gameObject.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
        }

        public static void RebuildLogItemLayout(RectTransform itemRoot, TextMeshProUGUI messageText)
        {
            if (messageText != null)
                messageText.ForceMeshUpdate();

            if (messageText != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.rectTransform);

            if (itemRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemRoot);
        }

        public static float LineHeight(int fontSize) => Mathf.Ceil(fontSize * LineHeightFactor);

        public static FontStyles ToFontStyles(FontStyle fontStyle)
        {
            return fontStyle switch
            {
                FontStyle.Bold => FontStyles.Bold,
                FontStyle.Italic => FontStyles.Italic,
                FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal
            };
        }

        public static void EnsureRectMask(RectTransform rect)
        {
            if (rect == null || rect.GetComponent<RectMask2D>() != null)
                return;

            rect.gameObject.AddComponent<RectMask2D>();
        }
    }
}
