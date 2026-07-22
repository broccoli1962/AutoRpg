using System.Collections.Generic;
using Backend.GameSystems.Exploration.Data;
using Backend.Util;
using Backend.Util.Management;
using R3;

namespace Backend.GameSystems.Exploration.Narration
{
    /// <summary>
    /// 현재 탐험 세션의 로그를 보관해 연대기 캐릭터별 일지 탭에서 조회한다.
    /// </summary>
    public sealed class ExplorationSessionLogArchive : SingletonGameObject<ExplorationSessionLogArchive>
    {
        private readonly List<string> _order = new();
        private readonly Dictionary<string, LogEntry> _entries = new();
        private CompositeDisposable _disposables;

        public static void EnsureInitialized()
        {
            if (GameStateUtil.IsQuitting)
                return;

            _ = Instance;
        }

        public static void Clear()
        {
            if (GameStateUtil.IsQuitting)
                return;

            Instance._order.Clear();
            Instance._entries.Clear();
        }

        public static IReadOnlyList<LogEntry> GetEntriesForCharacter(string characterId, string displayName)
        {
            if (GameStateUtil.IsQuitting ||
                string.IsNullOrEmpty(characterId))
            {
                return System.Array.Empty<LogEntry>();
            }

            var list = new List<LogEntry>();
            foreach (var eventId in Instance._order)
            {
                if (!Instance._entries.TryGetValue(eventId, out var entry) ||
                    entry == null ||
                    entry.IsPending ||
                    string.IsNullOrEmpty(entry.Text))
                {
                    continue;
                }

                if (entry.PerspectiveCharacterId == characterId ||
                    (!string.IsNullOrEmpty(displayName) && entry.Text.Contains(displayName)))
                {
                    list.Add(entry);
                }
            }

            return list;
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _disposables = new CompositeDisposable();

            ExplorationChannels.OnLogAdded
                .Subscribe(Upsert)
                .AddTo(_disposables);

            ExplorationChannels.OnLogUpdated
                .Subscribe(Upsert)
                .AddTo(_disposables);

            ExplorationChannels.OnLogStreaming
                .Subscribe(Upsert)
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void Upsert(LogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.EventId))
                return;

            if (!_entries.ContainsKey(entry.EventId))
                _order.Add(entry.EventId);

            _entries[entry.EventId] = CloneEntry(entry);
        }

        private static LogEntry CloneEntry(LogEntry source)
        {
            return new LogEntry
            {
                EventId = source.EventId,
                EventType = source.EventType,
                Salience = source.Salience,
                Category = source.Category,
                Text = source.Text,
                TimestampTick = source.TimestampTick,
                Floor = source.Floor,
                IsBookmarked = source.IsBookmarked,
                IsPending = source.IsPending,
                UsedLlm = source.UsedLlm,
                PerspectiveCharacterId = source.PerspectiveCharacterId
            };
        }
    }
}
