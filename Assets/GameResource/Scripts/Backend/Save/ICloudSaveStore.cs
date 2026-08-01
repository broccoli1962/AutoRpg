using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// Firestore 클라우드 세이브 읽기·쓰기.
    /// </summary>
    public interface ICloudSaveStore
    {
        /// <summary>
        /// 클라우드 세이브 메타데이터를 조회한다.
        /// </summary>
        UniTask<CloudSaveMetadata> FetchMetadataAsync(string userId);

        /// <summary>
        /// 클라우드 세이브를 다운로드한다.
        /// </summary>
        UniTask<GameSaveSnapshot> DownloadAsync(string userId);

        /// <summary>
        /// 클라우드에 세이브를 업로드한다.
        /// </summary>
        UniTask<bool> UploadAsync(string userId, GameSaveSnapshot snapshot);
    }
}
