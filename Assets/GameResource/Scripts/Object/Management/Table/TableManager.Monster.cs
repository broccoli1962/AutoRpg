using TableData;
using UnityEngine;

namespace Backend.Object.Management
{
    public partial class TableManager
    {
        private System.Collections.Generic.Dictionary<string, MonsterData> _monsterById;

        /// <summary>몬스터 ID 로 GSSL 테이블 행을 조회한다.</summary>
        public static MonsterData GetMonsterData(string monsterId) =>
            TryGetMonsterData(monsterId, out var data) ? data : null;

        /// <summary>몬스터 ID 조회. 없으면 false.</summary>
        public static bool TryGetMonsterData(string monsterId, out MonsterData data)
        {
            data = null;
            if (!IsInitialized || string.IsNullOrEmpty(monsterId))
                return false;

            Instance.EnsureMonsterDict();
            return Instance._monsterById.TryGetValue(monsterId, out data);
        }

        /// <summary>구역·ID 로 몬스터 행을 조회한다.</summary>
        public static bool TryGetMonster(string zoneId, string monsterId, out MonsterData data)
        {
            data = null;
            if (!TryGetMonsterData(monsterId, out data))
                return false;

            if (!string.IsNullOrEmpty(zoneId) && data.zoneId != zoneId)
            {
                data = null;
                return false;
            }

            return true;
        }

        private void EnsureMonsterDict()
        {
            if (_monsterById != null)
                return;

            _monsterById = new System.Collections.Generic.Dictionary<string, MonsterData>();
            var table = MonsterTable;
            if (table?.dataList == null)
                return;

            foreach (var row in table.dataList)
            {
                if (string.IsNullOrEmpty(row.id))
                    continue;

                _monsterById[row.id] = row;
            }
        }
    }
}
