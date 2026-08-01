using System;

namespace Backend.Meta.Characters
{
    /// <summary>
    /// 탐험가 도감·보유 목록 세이브 스냅샷.
    /// </summary>
    [Serializable]
    public sealed class ExplorerCatalogSaveData
    {
        public OwnedExplorerEntry[] Owned = Array.Empty<OwnedExplorerEntry>();
        public string[] CompendiumIds = Array.Empty<string>();
    }
}
