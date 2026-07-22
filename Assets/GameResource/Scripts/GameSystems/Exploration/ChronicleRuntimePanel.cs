using Backend.GameSystems.Character;
using Backend.GameSystems.Exploration.Data;
using Backend.GameSystems.Exploration.Narration;
using Backend.GameSystems.Prestige;
using UnityEngine;
using Backend.Util;
using UnityEngine.UI;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// Phase 6 프로토타입 연대기(회차 회고록 + 즐겨찾기 순간 + 캐릭터별 일지) 런타임 패널.
    /// </summary>
    public sealed class ChronicleRuntimePanel : ExplorationOverlayView
    {
        private const int EntriesPerPage = 4;

        private enum ChronicleTab
        {
            Runs,
            Favorites,
            CharacterJournal,
            LoreCompendium,
            MonsterCompendium
        }

        [SerializeField] private Text _contentText;
        [SerializeField] private Text _pageText;
        private ChronicleTab _tab = ChronicleTab.Runs;
        private int _pageFromEnd;
        private int _characterIndex;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (!IsVisible)
                return;

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha1, KeyCode.Keypad1))
            {
                _tab = ChronicleTab.Runs;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha2, KeyCode.Keypad2))
            {
                _tab = ChronicleTab.Favorites;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha3, KeyCode.Keypad3))
            {
                _tab = ChronicleTab.CharacterJournal;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha4, KeyCode.Keypad4))
            {
                _tab = ChronicleTab.LoreCompendium;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Alpha5, KeyCode.Keypad5))
            {
                _tab = ChronicleTab.MonsterCompendium;
                _pageFromEnd = 0;
                RefreshContent();
            }

            if (_tab == ChronicleTab.CharacterJournal &&
                (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.Q, KeyCode.Comma)))
            {
                CycleCharacter(-1);
            }

            if (_tab == ChronicleTab.CharacterJournal &&
                (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.E, KeyCode.Period)))
            {
                CycleCharacter(1);
            }

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.PageUp, KeyCode.LeftBracket))
                MovePage(older: true);

            if (KeyboardInputUtil.WasAnyKeyPressedThisFrame(KeyCode.PageDown, KeyCode.RightBracket))
                MovePage(older: false);
        }

        private void CycleCharacter(int delta)
        {
            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return;

            _characterIndex = (_characterIndex + delta + members.Count) % members.Count;
            _pageFromEnd = 0;
            RefreshContent();
        }

        private void MovePage(bool older)
        {
            var entryCount = GetEntryCount();
            var totalPages = GetPageCount(entryCount);
            if (totalPages <= 1)
                return;

            if (older)
                _pageFromEnd = Mathf.Min(_pageFromEnd + 1, totalPages - 1);
            else
                _pageFromEnd = Mathf.Max(_pageFromEnd - 1, 0);

            RefreshContent();
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public void OpenTab(ChroniclePanelTab tab)
        {
            _tab = (ChronicleTab)tab;
            _pageFromEnd = 0;
            Show();
        }

        public void ClosePanel()
        {
            Hide();
        }

        public enum ChroniclePanelTab
        {
            Runs = ChronicleTab.Runs,
            Favorites = ChronicleTab.Favorites,
            CharacterJournal = ChronicleTab.CharacterJournal,
            LoreCompendium = ChronicleTab.LoreCompendium,
            MonsterCompendium = ChronicleTab.MonsterCompendium
        }

        protected override void OnBeforeShow()
        {
            _pageFromEnd = 0;
            RefreshContent();
        }

        private void RefreshContent()
        {
            switch (_tab)
            {
                case ChronicleTab.Runs:
                    RefreshRunsTab();
                    break;
                case ChronicleTab.Favorites:
                    RefreshFavoritesTab();
                    break;
                case ChronicleTab.CharacterJournal:
                    RefreshCharacterJournalTab();
                    break;
                case ChronicleTab.LoreCompendium:
                    RefreshLoreCompendiumTab();
                    break;
                case ChronicleTab.MonsterCompendium:
                    RefreshMonsterCompendiumTab();
                    break;
            }
        }

        private void RefreshRunsTab()
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
            {
                _contentText.text = "메타 진행 데이터가 없습니다.";
                _pageText.text = string.Empty;
                return;
            }

            if (meta.ChronicleEntries == null || meta.ChronicleEntries.Count == 0)
            {
                _contentText.text = "아직 기록된 회차가 없습니다.\n탐험을 마치면 연대기가 쌓입니다.";
                _pageText.text = string.Empty;
                return;
            }

            RenderPagedEntries(
                meta.ChronicleEntries,
                "<b>[ 회차 연대기 ]</b>",
                entry => entry,
                bulletPrefix: "• ");
        }

        private void RefreshFavoritesTab()
        {
            var meta = PrestigeManager.GetMeta();
            if (meta == null)
            {
                _contentText.text = "메타 진행 데이터가 없습니다.";
                _pageText.text = string.Empty;
                return;
            }

            if (meta.FavoriteMoments == null || meta.FavoriteMoments.Count == 0)
            {
                _contentText.text = "즐겨찾기한 순간이 없습니다.\n로그에서 B키로 북마크할 수 있습니다.";
                _pageText.text = string.Empty;
                return;
            }

            RenderPagedEntries(
                meta.FavoriteMoments,
                "<b>[ 즐겨찾기 순간 ]</b>",
                entry => $"<color=#ffd966>★</color> {entry}",
                bulletPrefix: null);
        }

        private void RefreshCharacterJournalTab()
        {
            var party = ExplorationManager.GetCurrentState()?.Party;
            var members = party?.Members;
            if (members == null || members.Count == 0)
            {
                _contentText.text = "탐험 중인 파티가 없습니다.\n탐험을 시작하면 캐릭터별 일지를 볼 수 있습니다.";
                _pageText.text = string.Empty;
                return;
            }

            _characterIndex = Mathf.Clamp(_characterIndex, 0, members.Count - 1);
            var member = members[_characterIndex];
            var logs = ExplorationSessionLogArchive.GetEntriesForCharacter(member.CharacterId, member.DisplayName);
            var memoryPreview = CharacterMemoryManager.BuildHudPreview(member.CharacterId);

            if (logs.Count == 0 && string.IsNullOrEmpty(memoryPreview))
            {
                _contentText.text =
                    $"<b>[ {member.DisplayName}의 일지 ]</b>\n아직 기록된 로그나 기억이 없습니다.";
                _pageText.text = members.Count > 1 ? $"캐릭터 {_characterIndex + 1}/{members.Count}" : string.Empty;
                return;
            }

            var lines = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(memoryPreview))
            {
                lines.Add("<color=#8a9ab8><b>최근 기억</b></color>");
                lines.Add(StripRichText(memoryPreview));
            }

            foreach (var log in logs)
            {
                var prefix = log.UsedLlm ? "✦ " : string.Empty;
                lines.Add($"F{log.Floor} | {prefix}{log.Text}");
            }

            RenderPagedEntries(
                lines,
                $"<b>[ {member.DisplayName}의 일지 ]</b>",
                entry => entry,
                bulletPrefix: "• ");
        }

        private void RefreshMonsterCompendiumTab()
        {
            var entries = MonsterCompendiumManager.GetEntries();
            if (entries == null || entries.Count == 0)
            {
                _contentText.text = "아직 등록된 몬스터가 없습니다.\n전투 승리로 도감 항목이 해금됩니다.";
                _pageText.text = string.Empty;
                return;
            }

            var list = new System.Collections.Generic.List<string>(entries.Count);
            foreach (var stored in entries)
            {
                var separator = stored.IndexOf('|');
                list.Add(separator >= 0 && separator < stored.Length - 1
                    ? stored[(separator + 1)..]
                    : stored);
            }

            RenderPagedEntries(
                list,
                "<b>[ 몬스터 도감 ]</b>",
                entry => entry,
                bulletPrefix: "☠ ");
        }

        private void RefreshLoreCompendiumTab()
        {
            var entries = LoreCompendiumManager.GetEntries();
            if (entries == null || entries.Count == 0)
            {
                _contentText.text = "아직 수집한 로어 조각이 없습니다.\n탐험 중 발견·유물 이벤트로 도감이 채워집니다.";
                _pageText.text = string.Empty;
                return;
            }

            var list = new System.Collections.Generic.List<string>(entries);
            RenderPagedEntries(
                list,
                "<b>[ 로어 도감 ]</b>",
                entry => entry,
                bulletPrefix: "◆ ");
        }

        private static string StripRichText(string richText)
        {
            if (string.IsNullOrEmpty(richText))
                return richText;

            return richText
                .Replace("<color=#8a9ab8>", string.Empty)
                .Replace("</color>", string.Empty);
        }

        private void RenderPagedEntries(
            System.Collections.Generic.List<string> entries,
            string header,
            System.Func<string, string> formatEntry,
            string bulletPrefix)
        {
            var totalPages = GetPageCount(entries.Count);
            _pageFromEnd = Mathf.Clamp(_pageFromEnd, 0, Mathf.Max(0, totalPages - 1));

            var endExclusive = entries.Count - _pageFromEnd * EntriesPerPage;
            var startInclusive = Mathf.Max(0, endExclusive - EntriesPerPage);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine(header);
            for (var i = endExclusive - 1; i >= startInclusive; i--)
            {
                if (!string.IsNullOrEmpty(bulletPrefix))
                    builder.Append(bulletPrefix);

                builder.AppendLine(formatEntry(entries[i]));
            }

            _contentText.text = builder.ToString();

            if (_tab == ChronicleTab.CharacterJournal)
            {
                var members = ExplorationManager.GetCurrentState()?.Party?.Members;
                var memberHint = members != null && members.Count > 1
                    ? $" · 캐릭터 {_characterIndex + 1}/{members.Count}"
                    : string.Empty;

                _pageText.text = totalPages > 1
                    ? $"페이지 {totalPages - _pageFromEnd}/{totalPages}{memberHint}"
                    : memberHint.TrimStart(' ', '·');
                return;
            }

            _pageText.text = totalPages > 1
                ? $"페이지 {totalPages - _pageFromEnd}/{totalPages}"
                : string.Empty;
        }

        private int GetEntryCount()
        {
            if (_tab == ChronicleTab.LoreCompendium)
                return LoreCompendiumManager.GetEntries().Count;

            if (_tab == ChronicleTab.MonsterCompendium)
                return MonsterCompendiumManager.GetEntries().Count;

            if (_tab == ChronicleTab.CharacterJournal)
            {
                var member = GetSelectedMember();
                if (member == null)
                    return 0;

                var count = ExplorationSessionLogArchive.GetEntriesForCharacter(member.CharacterId, member.DisplayName).Count;
                if (!string.IsNullOrEmpty(CharacterMemoryManager.BuildHudPreview(member.CharacterId)))
                    count++;

                return count;
            }

            var meta = PrestigeManager.GetMeta();
            if (meta == null)
                return 0;

            if (_tab == ChronicleTab.Runs)
                return meta.ChronicleEntries?.Count ?? 0;

            return meta.FavoriteMoments?.Count ?? 0;
        }

        private CharacterState GetSelectedMember()
        {
            var members = ExplorationManager.GetCurrentState()?.Party?.Members;
            if (members == null || members.Count == 0)
                return null;

            _characterIndex = Mathf.Clamp(_characterIndex, 0, members.Count - 1);
            return members[_characterIndex];
        }

        private static int GetPageCount(int entryCount)
        {
            if (entryCount <= 0)
                return 1;

            return Mathf.CeilToInt(entryCount / (float)EntriesPerPage);
        }
    }
}
