using Backend.Object.UI.Exploration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// ExplorationHudPanel 프리팹에 레이아웃 metrics 를 런타임 반영 (스테이지 우선·로그 스트립).
    /// </summary>
    public static class ExplorationHudLayoutApplier
    {
        /// <summary>Body/CenterPanel/LogPanel LayoutElement 를 metrics 에 맞춘다.</summary>
        public static void ApplyStageFirstLayout(Transform hudRoot, bool isExploring = false)
        {
            if (hudRoot == null)
                return;

            var center = hudRoot.Find("Body/CenterPanel");
            var logPanel = hudRoot.Find("Body/LogPanel");
            var partyRow = hudRoot.Find("Body/PartyRow");

            ApplyLayoutElement(center, minHeight: ExplorationHudLayoutMetrics.CenterPanelMinHeight, flexibleHeight: 1f);
            var partyHeight = isExploring
                ? ExplorationHudLayoutMetrics.PartyRowCompactHeight
                : ExplorationHudLayoutMetrics.PartyRowHeight;
            ApplyLayoutElement(partyRow, minHeight: partyHeight, preferredHeight: partyHeight);
            ApplyLayoutElement(logPanel, minHeight: ExplorationHudLayoutMetrics.LogStripHeight, preferredHeight: ExplorationHudLayoutMetrics.LogStripHeight, flexibleHeight: 0f);

            var logFeed = hudRoot.GetComponentInChildren<ExplorationLogFeedView>(true);
            logFeed?.ApplyStripMode();
        }

        private static void ApplyLayoutElement(Transform target, float minHeight, float preferredHeight = -1f, float flexibleHeight = -1f)
        {
            if (target == null)
                return;

            var layout = target.GetComponent<LayoutElement>() ?? target.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = minHeight;
            if (preferredHeight >= 0f)
                layout.preferredHeight = preferredHeight;
            if (flexibleHeight >= 0f)
                layout.flexibleHeight = flexibleHeight;
        }
    }
}
