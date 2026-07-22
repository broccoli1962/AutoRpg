using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    public class ExplorationLogItemView : CachedMonobehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Image _categoryIcon;

        public string EventId { get; private set; }
        public string PlainText { get; private set; }
        public int Floor { get; private set; }
        public bool IsCombat { get; private set; }
        public bool IsDiscovery { get; private set; }
        public bool IsDynamicEvent { get; private set; }
        public bool IsNarrative { get; private set; }
        public bool IsBookmarked { get; private set; }

        private string _baseRichText;

        internal void ConfigureRuntime(Text messageText, Image categoryIcon)
        {
            _messageText = messageText;
            _categoryIcon = categoryIcon;
        }

        public void Bind(LogEntry entry)
        {
            EventId = entry.EventId;
            PlainText = entry.Text;
            Floor = entry.Floor;
            IsCombat = entry.Category == LogCategory.Combat;
            IsDiscovery = entry.Category == LogCategory.Discovery;
            IsDynamicEvent = false;
            IsNarrative = entry.UsedLlm
                || entry.Salience >= SalienceGrade.Significant
                || entry.Category == LogCategory.Milestone;
            IsBookmarked = LogBookmarkManager.IsBookmarked(PlainText, Floor);
            _baseRichText = LogDisplayUtil.FormatRichText(entry);
            ApplyDisplay(entry.IsPending ? FontStyle.Italic : FontStyle.Normal, entry.Category);
        }

        public void BindTagged(string plainText, int floor, string tag, LogCategory category, bool isDynamicEvent, bool isNarrative)
        {
            EventId = null;
            PlainText = plainText;
            Floor = floor;
            IsCombat = false;
            IsDiscovery = false;
            IsDynamicEvent = isDynamicEvent;
            IsNarrative = isNarrative;
            IsBookmarked = LogBookmarkManager.IsBookmarked(PlainText, Floor);
            _baseRichText = LogDisplayUtil.FormatTaggedLine(tag, plainText, LogDisplayUtil.GetCategoryColor(category));
            ApplyDisplay(FontStyle.Normal, category);
        }

        public bool MatchesFilter(LogFeedFilter filter)
        {
            return filter switch
            {
                LogFeedFilter.Combat => IsCombat,
                LogFeedFilter.Discovery => IsDiscovery,
                LogFeedFilter.Event => IsDynamicEvent,
                LogFeedFilter.Narrative => IsNarrative,
                _ => true
            };
        }

        public void SetBookmarked(bool bookmarked)
        {
            IsBookmarked = bookmarked;
            RefreshRichText();
        }

        public void RefreshRichText()
        {
            if (_messageText == null)
                return;

            _messageText.text = LogBookmarkManager.ApplyBookmarkPrefix(_baseRichText, IsBookmarked);
        }

        private void ApplyDisplay(FontStyle fontStyle, LogCategory category)
        {
            RefreshRichText();
            if (_messageText != null)
                _messageText.fontStyle = fontStyle;

            if (_categoryIcon != null)
                _categoryIcon.color = LogDisplayUtil.GetCategoryColor(category);
        }
    }
}
