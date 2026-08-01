using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Backend.Services.Save
{
    /// <summary>
    /// Firestore 클라우드 세이브 구현체. ABYSS_HAS_FIREBASE 정의 시 실 SDK를 사용한다.
    /// </summary>
    public sealed class FirebaseCloudSaveStore : ICloudSaveStore
    {
        private const string COLLECTION = "player_saves";
        private const string FIELD_PAYLOAD = "payload";
        private const string FIELD_SAVED_AT = "savedAtUnixSeconds";
        private const string FIELD_SCHEMA = "schemaVersion";
        private const string FIELD_CHECKSUM = "checksum";

        /// <summary>
        /// 클라우드 세이브 메타데이터를 조회한다.
        /// </summary>
        public async UniTask<CloudSaveMetadata> FetchMetadataAsync(string userId)
        {
#if ABYSS_HAS_FIREBASE
            if (string.IsNullOrEmpty(userId))
                return null;

            try
            {
                var doc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                    .Collection(COLLECTION)
                    .Document(userId)
                    .GetSnapshotAsync();

                if (!doc.Exists)
                    return null;

                return new CloudSaveMetadata(
                    userId,
                    doc.GetValue<long>(FIELD_SAVED_AT),
                    doc.GetValue<int>(FIELD_SCHEMA),
                    doc.GetValue<string>(FIELD_CHECKSUM));
            }
            catch (Exception)
            {
                return null;
            }
#else
            await UniTask.CompletedTask;
            return null;
#endif
        }

        /// <summary>
        /// 클라우드 세이브를 다운로드한다.
        /// </summary>
        public async UniTask<GameSaveSnapshot> DownloadAsync(string userId)
        {
#if ABYSS_HAS_FIREBASE
            if (string.IsNullOrEmpty(userId))
                return null;

            try
            {
                var doc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                    .Collection(COLLECTION)
                    .Document(userId)
                    .GetSnapshotAsync();

                if (!doc.Exists)
                    return null;

                var json = doc.GetValue<string>(FIELD_PAYLOAD);
                return JsonUtility.FromJson<GameSaveSnapshot>(json);
            }
            catch (Exception)
            {
                return null;
            }
#else
            await UniTask.CompletedTask;
            return null;
#endif
        }

        /// <summary>
        /// 클라우드에 세이브를 업로드한다.
        /// </summary>
        public async UniTask<bool> UploadAsync(string userId, GameSaveSnapshot snapshot)
        {
#if ABYSS_HAS_FIREBASE
            if (string.IsNullOrEmpty(userId) || snapshot == null)
                return false;

            try
            {
                snapshot.SavedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var json = JsonUtility.ToJson(snapshot);
                var checksum = EncryptedLocalSaveStore.ComputeChecksum(snapshot);

                var data = new System.Collections.Generic.Dictionary<string, object>
                {
                    { FIELD_PAYLOAD, json },
                    { FIELD_SAVED_AT, snapshot.SavedAtUnixSeconds },
                    { FIELD_SCHEMA, snapshot.SchemaVersion },
                    { FIELD_CHECKSUM, checksum },
                };

                await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                    .Collection(COLLECTION)
                    .Document(userId)
                    .SetAsync(data);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
#else
            await UniTask.CompletedTask;
            return false;
#endif
        }
    }
}
