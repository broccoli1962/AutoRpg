using System.Collections.Generic;
using Backend.GameSystems.DynamicEvent.Data;
using Backend.GameSystems.Exploration.Data;
using Backend.Object.Management;
using Backend.Util;

namespace Backend.GameSystems.Exploration
{
    /// <summary>
    /// GSSL 테이블 데이터 조회. TableManager 초기화 후 스프레드시트 데이터를 우선 사용한다.
    /// </summary>
    public static class GameTableCatalog
    {
        /// <summary>GSSL 테이블 사용 가능 여부.</summary>
        public static bool IsReady =>
            GameStateUtil.CanAccessManagers() &&
            TableManager.IsInitialized &&
            TableManager.ZoneTable != null;

        /// <summary>구역 표시 이름을 반환한다.</summary>
        public static string GetZoneDisplayName(string zoneId)
        {
            if (!TryGetZone(zoneId, out var zone))
                return zoneId;

            return zone.displayName;
        }

        /// <summary>구역 최소 층을 반환한다.</summary>
        public static int GetMinFloor(string zoneId)
        {
            return TryGetZone(zoneId, out var zone) ? zone.minFloor : 1;
        }

        /// <summary>구역 최대 층을 반환한다.</summary>
        public static int GetMaxFloor(string zoneId)
        {
            return TryGetZone(zoneId, out var zone) ? zone.maxFloor : ZoneDefinitions.MossyHollowMaxFloor;
        }

        /// <summary>구역 보상 배율을 반환한다.</summary>
        public static float GetRewardMultiplier(string zoneId, int floor = 0)
        {
            var multiplier = TryGetZone(zoneId, out var zone) ? zone.rewardMultiplier : 1f;
            if (floor > 0 && ZoneDefinitions.IsEndlessZone(zoneId))
            {
                var segment = ZoneDefinitions.GetEndlessSegmentIndex(floor);
                multiplier *= ZoneDefinitions.GetEndlessScaleMultiplier(
                    segment,
                    ZoneDefinitions.EndlessSegmentRewardBoost);
            }

            return multiplier;
        }

        /// <summary>구역 위험 배율을 반환한다.</summary>
        public static float GetRiskMultiplier(string zoneId, int floor = 0)
        {
            var multiplier = TryGetZone(zoneId, out var zone) ? zone.riskMultiplier : 1f;
            if (floor > 0 && ZoneDefinitions.IsEndlessZone(zoneId))
            {
                var segment = ZoneDefinitions.GetEndlessSegmentIndex(floor);
                multiplier *= ZoneDefinitions.GetEndlessScaleMultiplier(
                    segment,
                    ZoneDefinitions.EndlessSegmentRiskBoost);
            }

            return multiplier;
        }

        /// <summary>구역 몬스터 목록을 반환한다.</summary>
        public static IReadOnlyList<ZoneDefinitions.MonsterDefinition> GetMonsters(string zoneId)
        {
            if (!IsReady || TableManager.MonsterTable?.dataList == null)
                return ZoneDefinitions.GetMonstersFallback(zoneId);

            var list = new List<ZoneDefinitions.MonsterDefinition>();
            foreach (var row in TableManager.MonsterTable.dataList)
            {
                if (row.zoneId != zoneId)
                    continue;

                list.Add(new ZoneDefinitions.MonsterDefinition(
                    row.id,
                    row.displayName,
                    ParseMonsterRarity(row.rarity),
                    row.hp,
                    row.attack,
                    row.defense,
                    row.goldReward));
            }

            return list.Count > 0 ? list : ZoneDefinitions.GetMonstersFallback(zoneId);
        }

        /// <summary>몬스터 ID 로 GSSL 행을 조회한다. 없으면 null.</summary>
        public static MonsterData GetMonsterRow(string monsterId)
        {
            if (!IsReady || string.IsNullOrEmpty(monsterId))
                return null;

            return TableManager.TryGetMonsterData(monsterId, out var row) ? row : null;
        }

        /// <summary>구역 발견 아이템 목록을 반환한다.</summary>
        public static IReadOnlyList<ZoneDefinitions.DiscoveryDefinition> GetDiscoveries(string zoneId)
        {
            if (!IsReady || TableManager.DiscoveryTable?.dataList == null)
                return ZoneDefinitions.GetDiscoveriesFallback(zoneId);

            var list = new List<ZoneDefinitions.DiscoveryDefinition>();
            foreach (var row in TableManager.DiscoveryTable.dataList)
            {
                if (row.zoneId != zoneId)
                    continue;

                list.Add(new ZoneDefinitions.DiscoveryDefinition(
                    row.itemId,
                    row.displayName,
                    row.quantity,
                    row.goldValue));
            }

            return list.Count > 0 ? list : ZoneDefinitions.GetDiscoveriesFallback(zoneId);
        }

        /// <summary>GSSL Character 테이블 기반 기본 파티를 생성한다.</summary>
        public static PartyState CreateDefaultParty()
        {
            if (!IsReady || TableManager.CharacterTable?.dataList == null)
                return ZoneDefinitions.CreateDefaultPartyFallback();

            var party = new PartyState();
            foreach (var row in TableManager.CharacterTable.dataList)
            {
                if (string.IsNullOrEmpty(row.id))
                    continue;

                party.Members.Add(CreateCharacterFromRow(row));
            }

            return party.Members.Count > 0 ? party : ZoneDefinitions.CreateDefaultPartyFallback();
        }

        /// <summary>등록된 모든 구역 ID 목록.</summary>
        public static List<string> CreateAllZoneIdList()
        {
            if (!IsReady || TableManager.ZoneTable?.dataList == null)
                return ZoneDefinitions.CreateAllZoneIdListFallback();

            var list = new List<string>(TableManager.ZoneTable.dataList.Count);
            foreach (var row in TableManager.ZoneTable.dataList)
            {
                if (!string.IsNullOrEmpty(row.id))
                    list.Add(row.id);
            }

            return list.Count > 0 ? list : ZoneDefinitions.CreateAllZoneIdListFallback();
        }

        /// <summary>GSSL DynamicEvent 테이블과 코드 정의를 병합한 템플릿 목록.</summary>
        public static IReadOnlyList<DynamicEventTemplate> GetDynamicEventTemplates()
        {
            if (!IsReady || TableManager.DynamicEventTable?.dataList == null)
                return DynamicEventDefinitions.All;

            return DynamicEventDefinitions.MergeTableRows(
                TableManager.DynamicEventTable.dataList,
                CreateAllZoneIdList());
        }

        private static bool TryGetZone(string zoneId, out ZoneData zone)
        {
            zone = null;
            if (!IsReady || TableManager.ZoneTable?.dataList == null)
                return false;

            foreach (var row in TableManager.ZoneTable.dataList)
            {
                if (row.id != zoneId)
                    continue;

                zone = row;
                return true;
            }

            return false;
        }

        private static CharacterState CreateCharacterFromRow(CharacterData row)
        {
            var maxHp = 80 + row.vit * 6;
            var character = new CharacterState
            {
                CharacterId = row.id,
                DisplayName = row.displayName,
                Role = ParseEnum(row.role, CharacterRole.Warrior),
                Level = 1,
                Str = row.str,
                Agi = row.agi,
                Int = row.intel,
                Vit = row.vit,
                Luk = row.luk,
                MaxHp = maxHp,
                CurrentHp = maxHp
            };

            var personality = ParseEnum(row.personality, PersonalityTag.Cautious);
            character.PersonalityTags.Add(personality);
            return character;
        }

        private static MonsterRarity ParseMonsterRarity(string value)
        {
            return System.Enum.TryParse(value, out MonsterRarity rarity) ? rarity : MonsterRarity.Common;
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback)
            where TEnum : struct
        {
            return System.Enum.TryParse(value, out TEnum parsed) ? parsed : fallback;
        }
    }
}
