using System.Collections.Generic;
using System.Text;
using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Prestige;
using UnityEngine;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// 연대기 탭 페이지 데이터 조합 System.
    /// </summary>
    public static class ChronicleSystem
    {
        public const int EntriesPerPage = 4;

        public enum Tab
        {
            Runs,
            Favorites,
            CharacterJournal,
            LoreCompendium,
            MonsterCompendium
        }

        public readonly struct PageResult
        {
            public PageResult(string contentText, string pageText)
            {
                ContentText = contentText;
                PageText = pageText;
            }

            public string ContentText { get; }
            public string PageText { get; }
        }

        /// <summary>탭·페이지·캐릭터 인덱스로 페이지를 구성한다.</summary>
        public static PageResult BuildPage(Tab tab, int pageFromEnd, int characterIndex)
        {
            return tab switch
            {
                Tab.Runs => BuildRunsPage(pageFromEnd),
                Tab.Favorites => BuildFavoritesPage(pageFromEnd),
                Tab.CharacterJournal => BuildCharacterJournalPage(pageFromEnd, characterIndex),
                Tab.LoreCompendium => BuildLorePage(pageFromEnd),
                Tab.MonsterCompendium => BuildMonsterPage(pageFromEnd),
                _ => new PageResult(string.Empty, string.Empty)
            };
        }

        /// <summary>탭의 전체 항목 수.</summary>
        public static int GetEntryCount(Tab tab, int characterIndex)
        {
            if (tab == Tab.LoreCompendium)
                return LoreCompendiumSystem.GetEntries().Count;

            if (tab == Tab.MonsterCompendium)
                return MonsterCompendiumSystem.GetEntries().Count;

            if (tab == Tab.CharacterJournal)
            {
                var member = GetSelectedMember(characterIndex);
                if (member == null)
                    return 0;

                var count = ExplorationSessionLogArchive.GetEntriesForCharacter(member.CharacterId, member.DisplayName).Count;
                if (!string.IsNullOrEmpty(CharacterMemorySystem.BuildHudPreview(member.CharacterId)))
                    count++;

                return count;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta == null)
                return 0;

            return tab == Tab.Runs
                ? meta.ChronicleEntries?.Count ?? 0
                : meta.FavoriteMoments?.Count ?? 0;
        }

        /// <summary>항목 수 기준 페이지 수.</summary>
        public static int GetPageCount(int entryCount)
        {
            if (entryCount <= 0)
                return 1;

            return Mathf.CeilToInt(entryCount / (float)EntriesPerPage);
        }

        private static PageResult BuildRunsPage(int pageFromEnd)
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
                return new PageResult("메타 진행 데이터가 없습니다.", string.Empty);

            if (meta.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
                return new PageResult("아직 기록된 회차가 없습니다.\n탐험을 마치면 연대기가 쌓입니다.", string.Empty);

            return RenderPagedEntries(meta.ChronicleEntries, "<b>[ 회차 연대기 ]</b>", entry => entry, "• ", pageFromEnd, Tab.Runs, 0);
        }

        private static PageResult BuildFavoritesPage(int pageFromEnd)
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
                return new PageResult("메타 진행 데이터가 없습니다.", string.Empty);

            if (meta.FavoriteMoments == null || meta.FavoriteMoments.Count == 0)
                return new PageResult("즐겨찾기한 순간이 없습니다.\n로그에서 B키로 북마크할 수 있습니다.", string.Empty);

            return RenderPagedEntries(
                meta.FavoriteMoments,
                "<b>[ 즐겨찾기 순간 ]</b>",
                entry => $"<color=#ffd966>★</color> {entry}",
                null,
                pageFromEnd,
                Tab.Favorites,
                0);
        }

        private static PageResult BuildCharacterJournalPage(int pageFromEnd, int characterIndex)
        {
            var party = ExplorationSystem.GetCurrentState()?.Party;
            var members = party?.Members;
            if (members == null || members.Count == 0)
                return new PageResult("탐험 중인 파티가 없습니다.\n탐험을 시작하면 캐릭터별 일지를 볼 수 있습니다.", string.Empty);

            characterIndex = Mathf.Clamp(characterIndex, 0, members.Count - 1);
            var member = members[characterIndex];
            var logs = ExplorationSessionLogArchive.GetEntriesForCharacter(member.CharacterId, member.DisplayName);
            var memoryPreview = CharacterMemorySystem.BuildHudPreview(member.CharacterId);

            if (logs.Count == 0 && string.IsNullOrEmpty(memoryPreview))
            {
                return new PageResult(
                    $"<b>[ {member.DisplayName}의 일지 ]</b>\n아직 기록된 로그나 기억이 없습니다.",
                    members.Count > 1 ? $"캐릭터 {characterIndex + 1}/{members.Count}" : string.Empty);
            }

            var lines = new List<string>();
            if (!string.IsNullOrEmpty(memoryPreview))
            {
                lines.Add("<color=#8a9ab8><b>최근 기억</b></color>");
                lines.Add(StripRichText(memoryPreview));
            }

            foreach (var log in logs)
            {
                var prefix = log.UsedLlm ? "+ " : string.Empty;
                lines.Add($"F{log.Floor} | {prefix}{log.Text}");
            }

            return RenderPagedEntries(lines, $"<b>[ {member.DisplayName}의 일지 ]</b>", entry => entry, "• ", pageFromEnd, Tab.CharacterJournal, characterIndex);
        }

        private static PageResult BuildMonsterPage(int pageFromEnd)
        {
            var entries = MonsterCompendiumSystem.GetEntries();
            if (entries == null || entries.Count == 0)
                return new PageResult("아직 등록된 몬스터가 없습니다.\n전투 승리로 도감 항목이 해금됩니다.", string.Empty);

            var list = new List<string>(entries.Count);
            foreach (var stored in entries)
            {
                var separator = stored.IndexOf('|');
                list.Add(separator >= 0 && separator < stored.Length - 1
                    ? stored[(separator + 1)..]
                    : stored);
            }

            return RenderPagedEntries(list, "<b>[ 몬스터 도감 ]</b>", entry => entry, "☠ ", pageFromEnd, Tab.MonsterCompendium, 0);
        }

        private static PageResult BuildLorePage(int pageFromEnd)
        {
            var entries = LoreCompendiumSystem.GetEntries();
            if (entries == null || entries.Count == 0)
                return new PageResult("아직 수집한 로어 조각이 없습니다.\n탐험 중 발견·유물 이벤트로 도감이 채워집니다.", string.Empty);

            return RenderPagedEntries(new List<string>(entries), "<b>[ 로어 도감 ]</b>", entry => entry, "◆ ", pageFromEnd, Tab.LoreCompendium, 0);
        }

        private static PageResult RenderPagedEntries(
            List<string> entries,
            string header,
            System.Func<string, string> formatEntry,
            string bulletPrefix,
            int pageFromEnd,
            Tab tab,
            int characterIndex)
        {
            var totalPages = GetPageCount(entries.Count);
            pageFromEnd = Mathf.Clamp(pageFromEnd, 0, Mathf.Max(0, totalPages - 1));

            var endExclusive = entries.Count - pageFromEnd * EntriesPerPage;
            var startInclusive = Mathf.Max(0, endExclusive - EntriesPerPage);

            var builder = new StringBuilder();
            builder.AppendLine(header);
            for (var i = endExclusive - 1; i >= startInclusive; i--)
            {
                if (!string.IsNullOrEmpty(bulletPrefix))
                    builder.Append(bulletPrefix);

                builder.AppendLine(formatEntry(entries[i]));
            }

            if (tab == Tab.CharacterJournal)
            {
                var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
                var memberHint = members != null && members.Count > 1
                    ? $" · 캐릭터 {characterIndex + 1}/{members.Count}"
                    : string.Empty;

                var pageText = totalPages > 1
                    ? $"페이지 {totalPages - pageFromEnd}/{totalPages}{memberHint}"
                    : memberHint.TrimStart(' ', '·');

                return new PageResult(builder.ToString(), pageText);
            }

            return new PageResult(
                builder.ToString(),
                totalPages > 1 ? $"페이지 {totalPages - pageFromEnd}/{totalPages}" : string.Empty);
        }

        private static CharacterState GetSelectedMember(int characterIndex)
        {
            var members = ExplorationSystem.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return null;

            characterIndex = Mathf.Clamp(characterIndex, 0, members.Count - 1);
            return members[characterIndex];
        }

        private static string StripRichText(string richText)
        {
            if (string.IsNullOrEmpty(richText))
                return richText;

            return richText
                .Replace("<color=#8a9ab8>", string.Empty)
                .Replace("</color>", string.Empty);
        }
    }
}
