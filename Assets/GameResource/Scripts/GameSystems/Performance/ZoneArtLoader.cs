using System.Collections.Generic;
using System.Threading;
using Backend.AddressableKey;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 구역별 온디맨드 아트 Addressables 그룹을 로드·해제한다.
    /// </summary>
    public static class ZoneArtLoader
    {
        private static readonly Dictionary<int, AsyncOperationHandle> LoadedZones = new();

        /// <summary>
        /// 해당 구역(1~8) 아트 번들을 온디맨드로 로드한다.
        /// </summary>
        public static async UniTask LoadZoneAsync(int zoneIndex, CancellationToken token = default)
        {
            if (zoneIndex < 1 || zoneIndex > 8)
                return;

            if (LoadedZones.ContainsKey(zoneIndex))
                return;

            var label = AddressableKeys.ZoneArt.GetZoneLabel(zoneIndex);
            var handle = Addressables.LoadAssetsAsync<UnityEngine.Object>(label, null);
            await handle.ToUniTask(cancellationToken: token);

            if (handle.Status == AsyncOperationStatus.Succeeded)
                LoadedZones[zoneIndex] = handle;
            else
                Addressables.Release(handle);
        }

        /// <summary>
        /// 로드된 구역 아트를 해제한다.
        /// </summary>
        public static void ReleaseZone(int zoneIndex)
        {
            if (!LoadedZones.TryGetValue(zoneIndex, out var handle))
                return;

            if (handle.IsValid())
                Addressables.Release(handle);

            LoadedZones.Remove(zoneIndex);
        }

        /// <summary>
        /// 모든 로드된 구역 아트를 해제한다.
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var pair in LoadedZones)
            {
                if (pair.Value.IsValid())
                    Addressables.Release(pair.Value);
            }

            LoadedZones.Clear();
        }
    }
}
