using TableData;
using UnityEngine;

namespace Backend.Object.Management
{
    public partial class TableManager
    {
        private TableLinker _tableLinker;

        /// <summary>GSSL Zone 테이블.</summary>
        public static ZoneTable ZoneTable => Instance._tableLinker?.ZoneTable;

        /// <summary>GSSL Monster 테이블.</summary>
        public static MonsterTable MonsterTable => Instance._tableLinker?.MonsterTable;

        /// <summary>GSSL Discovery 테이블.</summary>
        public static DiscoveryTable DiscoveryTable => Instance._tableLinker?.DiscoveryTable;

        /// <summary>GSSL Character 테이블.</summary>
        public static CharacterTable CharacterTable => Instance._tableLinker?.CharacterTable;

        /// <summary>GSSL DynamicEvent 테이블.</summary>
        public static DynamicEventTable DynamicEventTable => Instance._tableLinker?.DynamicEventTable;
    }
}
