using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TableData
{
    [CreateAssetMenu(fileName = "TableLinker", menuName = "Tables/TableLinker")]
    public class TableLinker : ScriptableObject
    {
		 public ZoneTable ZoneTable;
		 public MonsterTable MonsterTable;
		 public DiscoveryTable DiscoveryTable;
		 public DynamicEventTable DynamicEventTable;
		 public CharacterTable CharacterTable;

    }
}