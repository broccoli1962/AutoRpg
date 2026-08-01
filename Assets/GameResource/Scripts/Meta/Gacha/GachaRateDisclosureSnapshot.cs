using System.Collections.Generic;

namespace Backend.Meta.Gacha
{
    /// <summary>
    /// 확률 공시 화면에 표시할 등급별·항목별 확률·천장 스냅샷.
    /// </summary>
    public sealed class GachaRateDisclosureSnapshot
    {
        public IReadOnlyList<GachaRateGradeEntry> GradeRates { get; set; }
        public IReadOnlyList<GachaRateItemEntry> ItemRates { get; set; }
        public int SsrPityCounter { get; set; }
        public int SsrPityThreshold { get; set; }
        public int UrPityCounter { get; set; }
        public int UrPityThreshold { get; set; }
        public int TenPullCount { get; set; }
    }

    /// <summary>
    /// 등급별 공급 확률 항목.
    /// </summary>
    public sealed class GachaRateGradeEntry
    {
        public string GradeLocalizeKey { get; set; }
        public int RateBasisPoints { get; set; }
    }

    /// <summary>
    /// 등급 내 개별 캐릭터 확률 항목.
    /// </summary>
    public sealed class GachaRateItemEntry
    {
        public string CharacterId { get; set; }
        public string CharacterNameLocalizeKey { get; set; }
        public string GradeLocalizeKey { get; set; }
        public int RateBasisPoints { get; set; }
    }
}
