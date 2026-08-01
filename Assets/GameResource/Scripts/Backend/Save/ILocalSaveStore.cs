using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// 로컬 암호화 세이브 읽기·쓰기.
    /// </summary>
    public interface ILocalSaveStore
    {
        /// <summary>
        /// 로컬 세이브가 존재하는지 확인한다.
        /// </summary>
        bool Exists();

        /// <summary>
        /// 로컬 세이브를 읽는다. 없으면 null.
        /// </summary>
        UniTask<GameSaveSnapshot> LoadAsync();

        /// <summary>
        /// 로컬 세이브를 저장한다.
        /// </summary>
        UniTask<bool> SaveAsync(GameSaveSnapshot snapshot);

        /// <summary>
        /// 로컬 세이브 메타데이터를 반환한다.
        /// </summary>
        CloudSaveMetadata GetLocalMetadata(string userId);
    }
}
