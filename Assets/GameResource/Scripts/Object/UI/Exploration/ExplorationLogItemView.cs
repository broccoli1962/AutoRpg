using System.Threading;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using Cysharp.Threading.Tasks;
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
        private CancellationTokenSource _typewriterCts;
        private int _revealedLength;
        private SalienceGrade _salience = SalienceGrade.Trivial;
        private LogCategory _category = LogCategory.Move;

        internal void ConfigureRuntime(Text messageText, Image categoryIcon)
        {
            _messageText = messageText;
            _categoryIcon = categoryIcon;
        }

        private void OnDestroy()
        {
            CancelTypewriter();
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
            _salience = entry.Salience;
            _category = entry.Category;

            if (entry.UsedLlm && entry.IsPending)
            {
                RunTypewriterAsync(entry).Forget();
                return;
            }

            CancelTypewriter();
            _revealedLength = PlainText?.Length ?? 0;
            _baseRichText = LogDisplayUtil.FormatRichText(entry);
            ApplyDisplay(entry.IsPending ? FontStyle.Italic : FontStyle.Normal);
        }

        public void BindTagged(string plainText, int floor, string tag, LogCategory category, bool isDynamicEvent, bool isNarrative)
        {
            CancelTypewriter();
            EventId = null;
            PlainText = plainText;
            Floor = floor;
            IsCombat = false;
            IsDiscovery = false;
            IsDynamicEvent = isDynamicEvent;
            IsNarrative = isNarrative;
            IsBookmarked = LogBookmarkManager.IsBookmarked(PlainText, Floor);
            _salience = SalienceGrade.Notable;
            _category = category;
            _baseRichText = LogDisplayUtil.FormatTaggedLine(tag, plainText, LogDisplayUtil.GetCategoryColor(category));
            ApplyDisplay(FontStyle.Normal);
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

        private async UniTaskVoid RunTypewriterAsync(LogEntry entry)
        {
            CancelTypewriter();
            _typewriterCts = new CancellationTokenSource();
            var token = _typewriterCts.Token;
            var targetText = entry.Text ?? string.Empty;

            if (targetText.Length < _revealedLength)
                _revealedLength = 0;

            try
            {
                while (_revealedLength < targetText.Length)
                {
                    _revealedLength++;
                    var partial = targetText.Substring(0, _revealedLength);
                    var partialEntry = LogTypewriterHelper.WithPartialText(entry, partial, isPending: true);
                    _baseRichText = LogDisplayUtil.FormatRichText(partialEntry);
                    ApplyDisplay(FontStyle.Italic);
                    await UniTask.Delay(LogTypewriterHelper.CharDelayMs, cancellationToken: token);
                }

                if (!entry.IsPending)
                {
                    _baseRichText = LogDisplayUtil.FormatRichText(entry);
                    ApplyDisplay(FontStyle.Normal);
                }
                else
                {
                    var partialEntry = LogTypewriterHelper.WithPartialText(entry, targetText, isPending: true);
                    _baseRichText = LogDisplayUtil.FormatRichText(partialEntry);
                    ApplyDisplay(FontStyle.Italic);
                }
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        private void CancelTypewriter()
        {
            _typewriterCts?.Cancel();
            _typewriterCts?.Dispose();
            _typewriterCts = null;
        }

        private void ApplyDisplay(FontStyle fontStyle)
        {
            RefreshRichText();
            if (_messageText != null)
                _messageText.fontStyle = fontStyle;

            if (_categoryIcon != null)
                _categoryIcon.color = LogDisplayUtil.GetDisplayColor(_category, _salience);
        }
    }
}
