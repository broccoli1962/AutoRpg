using System;
using Backend.Services.Auth;
using Cysharp.Threading.Tasks;

namespace Backend.Services.Save
{
    /// <summary>
    /// 로컬 진실 공급원 + Firestore 주기 백업·복원·충돌 해결.
    /// </summary>
    public sealed class SaveBackupService
    {
        private const long CONFLICT_THRESHOLD_SECONDS = 60;

        private readonly IAuthService _authService;
        private readonly ILocalSaveStore _localStore;
        private readonly ICloudSaveStore _cloudStore;
        private readonly IGameSaveAggregator _aggregator;
        private readonly ISaveConflictPresenter _conflictPresenter;
        private readonly Func<DateTimeOffset> _utcNow;

        private DateTimeOffset _lastCloudBackupUtc;
        private TimeSpan _backupInterval = TimeSpan.FromMinutes(5);

        public SaveBackupService(
            IAuthService authService,
            ILocalSaveStore localStore,
            ICloudSaveStore cloudStore,
            IGameSaveAggregator aggregator,
            ISaveConflictPresenter conflictPresenter = null,
            Func<DateTimeOffset> utcNow = null)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _localStore = localStore ?? throw new ArgumentNullException(nameof(localStore));
            _cloudStore = cloudStore ?? throw new ArgumentNullException(nameof(cloudStore));
            _aggregator = aggregator ?? throw new ArgumentNullException(nameof(aggregator));
            _conflictPresenter = conflictPresenter ?? new AutoLocalSaveConflictPresenter();
            _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// 마지막 클라우드 백업 시각.
        /// </summary>
        public DateTimeOffset LastCloudBackupUtc => _lastCloudBackupUtc;

        /// <summary>
        /// 클라우드 백업 주기.
        /// </summary>
        public TimeSpan BackupInterval
        {
            get => _backupInterval;
            set => _backupInterval = value.TotalSeconds > 0 ? value : TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// 부트 시 로컬·클라우드 세이브를 동기화한다.
        /// </summary>
        public async UniTask<bool> SynchronizeOnBootAsync()
        {
            var userId = _authService.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
                return false;

            var localExists = _localStore.Exists();
            var localSnapshot = localExists ? await _localStore.LoadAsync() : null;
            var localMetadata = localExists
                ? _localStore.GetLocalMetadata(userId)
                : null;

            var cloudMetadata = await _cloudStore.FetchMetadataAsync(userId);
            var cloudSnapshot = cloudMetadata != null
                ? await _cloudStore.DownloadAsync(userId)
                : null;

            if (localSnapshot == null && cloudSnapshot != null)
            {
                await ApplyCloudSnapshotAsync(userId, cloudSnapshot);
                return true;
            }

            if (localSnapshot != null && cloudSnapshot == null)
            {
                await _localStore.SaveAsync(localSnapshot);
                await _cloudStore.UploadAsync(userId, localSnapshot);
                _lastCloudBackupUtc = _utcNow();
                return true;
            }

            if (localSnapshot != null && cloudSnapshot != null && HasConflict(localMetadata, cloudMetadata))
            {
                var choice = await _conflictPresenter.PresentConflictAsync(localMetadata, cloudMetadata);
                switch (choice)
                {
                    case SaveConflictChoice.UseCloud:
                        await ApplyCloudSnapshotAsync(userId, cloudSnapshot);
                        break;
                    case SaveConflictChoice.UseLocal:
                        await _localStore.SaveAsync(localSnapshot);
                        await _cloudStore.UploadAsync(userId, localSnapshot);
                        _lastCloudBackupUtc = _utcNow();
                        break;
                }

                return true;
            }

            if (localSnapshot != null)
                _aggregator.ImportSnapshot(localSnapshot);

            return true;
        }

        /// <summary>
        /// 현재 상태를 로컬에 저장하고 필요 시 클라우드에 백업한다.
        /// </summary>
        public async UniTask<bool> SaveLocalAsync(bool forceCloudBackup = false)
        {
            var snapshot = _aggregator.ExportSnapshot();
            var saved = await _localStore.SaveAsync(snapshot);
            if (!saved)
                return false;

            if (forceCloudBackup || ShouldBackupToCloud())
                return await BackupToCloudAsync(snapshot);

            return true;
        }

        /// <summary>
        /// 클라우드에서 세이브를 복원한다.
        /// </summary>
        public async UniTask<bool> RestoreFromCloudAsync()
        {
            var userId = _authService.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
                return false;

            var cloudSnapshot = await _cloudStore.DownloadAsync(userId);
            if (cloudSnapshot == null)
                return false;

            await ApplyCloudSnapshotAsync(userId, cloudSnapshot);
            return true;
        }

        private async UniTask ApplyCloudSnapshotAsync(string userId, GameSaveSnapshot cloudSnapshot)
        {
            _aggregator.ImportSnapshot(cloudSnapshot);
            await _localStore.SaveAsync(cloudSnapshot);
            await _cloudStore.UploadAsync(userId, cloudSnapshot);
            _lastCloudBackupUtc = _utcNow();
        }

        private async UniTask<bool> BackupToCloudAsync(GameSaveSnapshot snapshot)
        {
            var userId = _authService.CurrentUser?.UserId;
            if (string.IsNullOrEmpty(userId))
                return false;

            var uploaded = await _cloudStore.UploadAsync(userId, snapshot);
            if (uploaded)
                _lastCloudBackupUtc = _utcNow();

            return uploaded;
        }

        private bool ShouldBackupToCloud()
        {
            if (_lastCloudBackupUtc == default)
                return true;

            return _utcNow() - _lastCloudBackupUtc >= _backupInterval;
        }

        private static bool HasConflict(CloudSaveMetadata local, CloudSaveMetadata cloud)
        {
            if (local == null || cloud == null)
                return false;

            if (local.Checksum == cloud.Checksum)
                return false;

            var delta = Math.Abs(local.SavedAtUnixSeconds - cloud.SavedAtUnixSeconds);
            return delta >= CONFLICT_THRESHOLD_SECONDS;
        }
    }
}
