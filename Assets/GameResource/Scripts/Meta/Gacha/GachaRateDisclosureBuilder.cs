using System;
using System.Collections.Generic;
using Backend.Meta.Characters;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// GachaRateTable·GachaBannerPool·천장 상태로 확률 공시 스냅샷을 생성한다.
    /// </summary>
    public static class GachaRateDisclosureBuilder
    {
        private const int TEN_PULL_COUNT = 10;

        private static readonly ExplorerGrade[] GradesInOrder =
        {
            ExplorerGrade.R,
            ExplorerGrade.SR,
            ExplorerGrade.SSR,
            ExplorerGrade.UR,
        };

        /// <summary>
        /// 확률 공시 화면용 스냅샷을 생성한다.
        /// </summary>
        public static GachaRateDisclosureSnapshot Build(
            GachaRateTable rateTable,
            GachaBannerPool bannerPool,
            GachaPityState pity)
        {
            if (rateTable == null)
                throw new ArgumentNullException(nameof(rateTable));
            if (bannerPool == null)
                throw new ArgumentNullException(nameof(bannerPool));

            rateTable.ValidateRates();

            var gradeRates = BuildGradeRates(rateTable);
            var itemRates = BuildItemRates(rateTable, bannerPool);

            return new GachaRateDisclosureSnapshot
            {
                GradeRates = gradeRates,
                ItemRates = itemRates,
                SsrPityCounter = pity?.GetSsrCounter() ?? 0,
                SsrPityThreshold = rateTable.SsrPityThreshold,
                UrPityCounter = pity?.GetUrCounter() ?? 0,
                UrPityThreshold = rateTable.UrPityThreshold,
                TenPullCount = TEN_PULL_COUNT,
            };
        }

        /// <summary>
        /// 만분율을 표시용 퍼센트 문자열로 변환한다.
        /// </summary>
        public static string FormatPercent(int basisPoints)
        {
            var percent = basisPoints / 100f;
            return percent.ToString("0.##");
        }

        private static List<GachaRateGradeEntry> BuildGradeRates(GachaRateTable rateTable)
        {
            var list = new List<GachaRateGradeEntry>(GradesInOrder.Length);

            foreach (var grade in GradesInOrder)
            {
                list.Add(new GachaRateGradeEntry
                {
                    GradeLocalizeKey = GetGradeLocalizeKey(grade),
                    RateBasisPoints = rateTable.GetRateBasisPoints(grade),
                });
            }

            return list;
        }

        private static List<GachaRateItemEntry> BuildItemRates(
            GachaRateTable rateTable,
            GachaBannerPool bannerPool)
        {
            var list = new List<GachaRateItemEntry>();

            foreach (var grade in GradesInOrder)
            {
                var members = bannerPool.GetGradePoolMembers(grade);
                if (members.Count == 0)
                    continue;

                var gradeRate = rateTable.GetRateBasisPoints(grade);
                var perItemRate = gradeRate / members.Count;
                var gradeKey = GetGradeLocalizeKey(grade);

                foreach (var characterId in members)
                {
                    list.Add(new GachaRateItemEntry
                    {
                        CharacterId = characterId,
                        CharacterNameLocalizeKey = GetCharacterLocalizeKey(characterId),
                        GradeLocalizeKey = gradeKey,
                        RateBasisPoints = perItemRate,
                    });
                }
            }

            return list;
        }

        private static string GetGradeLocalizeKey(ExplorerGrade grade)
        {
            return grade switch
            {
                ExplorerGrade.R => "gacha.rate.grade.r",
                ExplorerGrade.SR => "gacha.rate.grade.sr",
                ExplorerGrade.SSR => "gacha.rate.grade.ssr",
                ExplorerGrade.UR => "gacha.rate.grade.ur",
                _ => "gacha.rate.grade.r",
            };
        }

        private static string GetCharacterLocalizeKey(string characterId)
        {
            return $"gacha.character.{characterId}";
        }
    }
}
