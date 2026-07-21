using System.Collections.Generic;
using Backend.GameSystems.Exploration;
using Backend.GameSystems.Exploration.Narration;
using Backend.Util;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Exploration
{
    public class ExplorationLogFeedView : UIView
    {
        private const int MaxVisibleLogs = 80;

        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private ExplorationLogItemView _itemPrefab;

        private readonly List<ExplorationLogItemView> _items = new();
        private CompositeDisposable _disposables;

        protected override void OnShow()
        {
            base.OnShow();
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnLogAdded
                .Subscribe(AddLog)
                .AddTo(_disposables);
        }

        protected override void OnHide()
        {
            base.OnHide();
            _disposables?.Dispose();
            _disposables = null;
        }

        public void ClearLogs()
        {
            foreach (var item in _items)
            {
                if (item != null)
                    Destroy(item.CachedGameObject);
            }

            _items.Clear();
        }

        private void AddLog(LogEntry entry)
        {
            if (_itemPrefab == null || _contentRoot == null)
                return;

            var item = Instantiate(_itemPrefab, _contentRoot);
            item.Bind(entry);
            _items.Add(item);

            while (_items.Count > MaxVisibleLogs)
            {
                var oldest = _items[0];
                _items.RemoveAt(0);
                if (oldest != null)
                    Destroy(oldest.CachedGameObject);
            }

            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
