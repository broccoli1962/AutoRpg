using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// EditMode·오프라인용 인메모리 클라우드 세이브 스토어.
    /// </summary>
    public sealed class SimulatedCloudSaveStore : ICloudSaveStore
    {
        private readonly Dictionary<string, GameSaveSnapshot> _store = new();

        /// <summary>
        /// 클라우드 세이브 메타데이터를 조회한다.
        /// </summary>
        public UniTask<CloudSaveMetadata> FetchMetadataAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !_store.TryGetValue(userId, out var snapshot))
                return UniTask.FromResult<CloudSaveMetadata>(null);

            return UniTask.FromResult(new CloudSaveMetadata(
                userId,
                snapshot.SavedAtUnixSeconds,
                snapshot.SchemaVersion,
                EncryptedLocalSaveStore.ComputeChecksum(snapshot)));
        }

        /// <summary>
        /// 클라우드 세이브를 다운로드한다.
        /// </summary>
        public UniTask<GameSaveSnapshot> DownloadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !_store.TryGetValue(userId, out var snapshot))
                return UniTask.FromResult<GameSaveSnapshot>(null);

            return UniTask.FromResult(CloneSnapshot(snapshot));
        }

        /// <summary>
        /// 클라우드에 세이브를 업로드한다.
        /// </summary>
        public UniTask<bool> UploadAsync(string userId, GameSaveSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(userId) || snapshot == null)
                return UniTask.FromResult(false);

            _store[userId] = CloneSnapshot(snapshot);
            return UniTask.FromResult(true);
        }

        /// <summary>
        /// 테스트용 저장소를 비운다.
        /// </summary>
        public void ClearForTests()
        {
            _store.Clear();
        }

        private static GameSaveSnapshot CloneSnapshot(GameSaveSnapshot source)
        {
            var json = UnityEngine.JsonUtility.ToJson(source);
            return UnityEngine.JsonUtility.FromJson<GameSaveSnapshot>(json);
        }
    }
}
