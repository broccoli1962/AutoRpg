using System;
using System.Collections.Generic;
using Backend.Simulation;

namespace Backend.Meta.Characters
{
    /// <summary>
    /// 탐험가 도감(Compendium)과 보유 목록(Roster)을 관리한다.
    /// </summary>
    public sealed class ExplorerCatalog
    {
        private const string INVALID_CHARACTER_ID = "CharacterId must not be empty.";
        private const string NOT_OWNED = "Explorer is not owned.";
        private const string ALREADY_MAX_LIMIT_BREAK = "Limit break stage is already at maximum.";
        private const string INSUFFICIENT_FRAGMENTS = "Insufficient fragments for limit break.";

        private readonly Dictionary<string, OwnedExplorerEntry> _owned = new();
        private readonly HashSet<string> _compendium = new();

        /// <summary>
        /// 보유 탐험가 수를 반환한다.
        /// </summary>
        public int OwnedCount => _owned.Count;

        /// <summary>
        /// 도감 등재 수를 반환한다.
        /// </summary>
        public int CompendiumCount => _compendium.Count;

        /// <summary>
        /// 탐험가 보유 여부를 반환한다.
        /// </summary>
        public bool IsOwned(string characterId)
        {
            return !string.IsNullOrEmpty(characterId) && _owned.ContainsKey(characterId);
        }

        /// <summary>
        /// 도감 등재 여부를 반환한다.
        /// </summary>
        public bool IsInCompendium(string characterId)
        {
            return !string.IsNullOrEmpty(characterId) && _compendium.Contains(characterId);
        }

        /// <summary>
        /// 보유 탐험가 항목을 반환한다. 없으면 null.
        /// </summary>
        public OwnedExplorerEntry GetOwned(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return null;

            return _owned.TryGetValue(characterId, out var entry) ? entry : null;
        }

        /// <summary>
        /// 보유 탐험가의 조각 수를 반환한다.
        /// </summary>
        public int GetFragmentCount(string characterId)
        {
            return GetOwned(characterId)?.FragmentCount ?? 0;
        }

        /// <summary>
        /// 보유 탐험가의 한계돌파 단계를 반환한다.
        /// </summary>
        public int GetLimitBreakStage(string characterId)
        {
            return GetOwned(characterId)?.LimitBreakStage ?? 0;
        }

        /// <summary>
        /// 탐험가 획득을 처리한다. 미보유 시 캐릭터 지급, 보유 시 등급별 조각 전환.
        /// </summary>
        public ExplorerAcquireResult TryAcquire(
            string characterId,
            ExplorerGrade grade,
            BalanceTable balanceTable)
        {
            if (string.IsNullOrEmpty(characterId))
                return ExplorerAcquireResult.Failed(characterId, INVALID_CHARACTER_ID);

            if (balanceTable == null)
                throw new ArgumentNullException(nameof(balanceTable));

            RegisterCompendium(characterId);

            if (!_owned.TryGetValue(characterId, out var entry))
            {
                entry = new OwnedExplorerEntry(characterId, grade);
                _owned[characterId] = entry;
                return ExplorerAcquireResult.NewCharacter(characterId, grade);
            }

            var fragments = ExplorerBalanceFormulas.GetDuplicateFragmentYield(balanceTable, grade);
            entry.FragmentCount += fragments;
            return ExplorerAcquireResult.DuplicateFragments(characterId, grade, fragments);
        }

        /// <summary>
        /// 조각을 소모해 한계돌파 단계를 1 올린다.
        /// </summary>
        public LimitBreakResult TryLimitBreak(string characterId, BalanceTable balanceTable)
        {
            if (string.IsNullOrEmpty(characterId))
                return LimitBreakResult.Failed(characterId, 0, INVALID_CHARACTER_ID);

            if (balanceTable == null)
                throw new ArgumentNullException(nameof(balanceTable));

            if (!_owned.TryGetValue(characterId, out var entry))
                return LimitBreakResult.Failed(characterId, 0, NOT_OWNED);

            var currentStage = entry.LimitBreakStage;
            if (currentStage >= balanceTable.MaxLimitBreakStage)
                return LimitBreakResult.Failed(characterId, currentStage, ALREADY_MAX_LIMIT_BREAK);

            var cost = ExplorerBalanceFormulas.GetLimitBreakFragmentCost(
                balanceTable,
                entry.Grade,
                currentStage);

            if (cost <= 0 || entry.FragmentCount < cost)
                return LimitBreakResult.Failed(characterId, currentStage, INSUFFICIENT_FRAGMENTS);

            entry.FragmentCount -= cost;
            entry.LimitBreakStage = currentStage + 1;

            return LimitBreakResult.Succeeded(
                characterId,
                currentStage,
                entry.LimitBreakStage,
                cost);
        }

        /// <summary>
        /// 세이브용 스냅샷을 생성한다.
        /// </summary>
        public ExplorerCatalogSaveData ToSaveData()
        {
            var owned = new OwnedExplorerEntry[_owned.Count];
            var ownedIndex = 0;

            foreach (var entry in _owned.Values)
            {
                owned[ownedIndex++] = new OwnedExplorerEntry
                {
                    CharacterId = entry.CharacterId,
                    Grade = entry.Grade,
                    LimitBreakStage = entry.LimitBreakStage,
                    FragmentCount = entry.FragmentCount,
                };
            }

            var compendium = new string[_compendium.Count];
            var compendiumIndex = 0;

            foreach (var id in _compendium)
                compendium[compendiumIndex++] = id;

            return new ExplorerCatalogSaveData
            {
                Owned = owned,
                CompendiumIds = compendium,
            };
        }

        /// <summary>
        /// 세이브 스냅샷에서 ExplorerCatalog 를 복원한다.
        /// </summary>
        public static ExplorerCatalog FromSaveData(ExplorerCatalogSaveData saveData)
        {
            var catalog = new ExplorerCatalog();

            if (saveData?.CompendiumIds != null)
            {
                foreach (var id in saveData.CompendiumIds)
                    catalog.RegisterCompendium(id);
            }

            if (saveData?.Owned == null)
                return catalog;

            foreach (var entry in saveData.Owned)
            {
                if (entry == null || string.IsNullOrEmpty(entry.CharacterId))
                    continue;

                catalog._owned[entry.CharacterId] = new OwnedExplorerEntry
                {
                    CharacterId = entry.CharacterId,
                    Grade = entry.Grade,
                    LimitBreakStage = entry.LimitBreakStage,
                    FragmentCount = entry.FragmentCount,
                };
                catalog.RegisterCompendium(entry.CharacterId);
            }

            return catalog;
        }

        private void RegisterCompendium(string characterId)
        {
            if (!string.IsNullOrEmpty(characterId))
                _compendium.Add(characterId);
        }
    }
}
