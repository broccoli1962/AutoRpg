using Backend.GameSystems.Exploration.Data;

namespace Backend.GameSystems.Exploration.Stage
{
    /// <summary>
    /// 층 진행도 → 스테이지 세그먼트 라벨 (예: 1-3, 보스 구간).
    /// </summary>
    public static class StageSegmentUtil
    {
        public static string BuildStageLabel(ExplorationState state)
        {
            if (state == null)
                return string.Empty;

            var zoneLabel = StageZoneTheme.Resolve(state.ZoneId).ZoneLabel;
            var segment = ResolveSegmentIndex(state.FloorProgress);
            return $"{zoneLabel} {state.CurrentFloor}-{segment}";
        }

        public static string BuildSegmentHint(ExplorationState state)
        {
            if (state == null)
                return string.Empty;

            var progress = state.FloorProgress;
            if (progress >= 95f)
                return "출구";
            if (progress >= 70f)
                return "심층";
            if (progress >= 35f)
                return "전투";
            return "탐색";
        }

        private static int ResolveSegmentIndex(float floorProgress)
        {
            if (floorProgress < 25f)
                return 1;
            if (floorProgress < 50f)
                return 2;
            if (floorProgress < 75f)
                return 3;
            return 4;
        }
    }
}
