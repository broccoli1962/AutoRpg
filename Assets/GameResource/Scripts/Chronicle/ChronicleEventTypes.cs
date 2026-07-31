namespace Backend.Chronicle
{
    /// <summary>
    /// 연대기 문장 뱅크 이벤트 타입 식별자.
    /// </summary>
    public static class ChronicleEventTypes
    {
        public const string Move = "move";
        public const string CombatResult = "combat_result";
        public const string Discovery = "discovery";
        public const string Trap = "trap";
        public const string Rest = "rest";
        public const string Injury = "injury";
        public const string FloorClear = "floor_clear";
        public const string Milestone = "milestone";

        public static readonly string[] All =
        {
            Move,
            CombatResult,
            Discovery,
            Trap,
            Rest,
            Injury,
            FloorClear,
            Milestone
        };
    }
}
