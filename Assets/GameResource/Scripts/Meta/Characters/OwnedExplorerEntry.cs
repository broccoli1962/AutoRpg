using System;

namespace Backend.Meta.Characters
{
    /// <summary>
    /// 보유 탐험가 1명의 런타임 상태.
    /// </summary>
    [Serializable]
    public sealed class OwnedExplorerEntry
    {
        public string CharacterId;
        public ExplorerGrade Grade;
        public int LimitBreakStage;
        public int FragmentCount;

        public OwnedExplorerEntry()
        {
        }

        public OwnedExplorerEntry(string characterId, ExplorerGrade grade)
        {
            CharacterId = characterId;
            Grade = grade;
            LimitBreakStage = 0;
            FragmentCount = 0;
        }
    }
}
