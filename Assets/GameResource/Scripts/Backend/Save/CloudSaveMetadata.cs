using System;

namespace Backend.Services.Save
{
    /// <summary>
    /// 클라우드 세이브 메타데이터.
    /// </summary>
    public sealed class CloudSaveMetadata
    {
        public string UserId { get; }
        public long SavedAtUnixSeconds { get; }
        public int SchemaVersion { get; }
        public string Checksum { get; }

        public CloudSaveMetadata(string userId, long savedAtUnixSeconds, int schemaVersion, string checksum)
        {
            UserId = userId ?? string.Empty;
            SavedAtUnixSeconds = savedAtUnixSeconds;
            SchemaVersion = schemaVersion;
            Checksum = checksum ?? string.Empty;
        }

        /// <summary>
        /// UTC 기준 저장 시각.
        /// </summary>
        public DateTimeOffset SavedAtUtc =>
            DateTimeOffset.FromUnixTimeSeconds(SavedAtUnixSeconds);
    }
}
