using System.Threading;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    public class ExplorationLogItemView : CachedMonobehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _categoryIcon;
        [SerializeField] private Image _accentImage;

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

        internal void ConfigureRuntime(TextMeshProUGUI messageText, Image categoryIcon)
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
            IsBookmarked = LogBookmarkSystem.IsBookmarked(PlainText, Floor);
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
            IsBookmarked = LogBookmarkSystem.IsBookmarked(PlainText, Floor);
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

        /// <summary>로그 스트립 모드 — 글자 크기·한 줄 높이 축소.</summary>
        public void ApplyStripTypography(int fontSize)
        {
            if (_messageText == null)
                return;

            _messageText.fontSize = fontSize;
            UiTmpUtil.ApplyLogMessageCell(_messageText, RuntimeUiTmpFont.Get(), fontSize);
            UiTmpUtil.RebuildLogItemLayout(transform as RectTransform, _messageText);
        }

        public void RefreshRichText()
        {
            if (_messageText == null)
                return;

            _messageText.text = LogBookmarkSystem.ApplyBookmarkPrefix(_baseRichText, IsBookmarked);
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
            {
                _messageText.fontStyle = UiTmpUtil.ToFontStyles(fontStyle);
                _messageText.color = ModernUiStyle.BodyText;
            }

            ApplyCategoryIcon();
            UiTmpUtil.RebuildLogItemLayout(transform as RectTransform, _messageText);
        }

        private void ApplyCategoryIcon()
        {
            if (_categoryIcon != null)
            {
                var iconCategory = ResolveIconCategory();
                RuntimeUiSprites.ApplySimpleImage(
                    _categoryIcon,
                    RuntimeUiSprites.GetLogIconKey(iconCategory),
                    Color.white);
            }

            if (_accentImage == null)
                return;

            _accentImage.color = ResolveIconCategory() switch
            {
                RuntimeUiSprites.LogIconCategory.Combat => ModernUiStyle.DangerRed,
                RuntimeUiSprites.LogIconCategory.Discovery => ModernUiStyle.AccentGreen,
                RuntimeUiSprites.LogIconCategory.Event => ModernUiStyle.TitleGold,
                _ => ModernUiStyle.AccentCyan
            };
        }

        private RuntimeUiSprites.LogIconCategory ResolveIconCategory()
        {
            if (IsDynamicEvent)
                return RuntimeUiSprites.LogIconCategory.Event;

            if (IsCombat || _category == LogCategory.Combat)
                return RuntimeUiSprites.LogIconCategory.Combat;

            if (IsDiscovery || _category == LogCategory.Discovery)
                return RuntimeUiSprites.LogIconCategory.Discovery;

            return RuntimeUiSprites.LogIconCategory.Narrative;
        }
    }
}
