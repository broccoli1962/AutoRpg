#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.GameSystems.Performance.Editor
{
    /// <summary>
    /// Addressables 를 재구성해 초기 다운로드를 줄이고 구역별 아트를 온디맨드 그룹으로 분리한다.
    /// </summary>
    public static class AddressablePerformanceSetup
    {
        private const string InGameGroupName = "InGame";
        private const string CoreGroupName = "Core";
        private const string ImagesRoot = "Assets/GameResource/Images";

        [MenuItem("Tools/Abyss Chronicle/Setup Performance Addressables")]
        public static void SetupAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            RemoveBroadImageEntry(settings);
            EnsureCoreGroup(settings);
            EnsureZoneArtGroups(settings);
            MoveLegacyArtToRemote(settings);
            RemoveEntryByAddress(settings, ImagesRoot);

            AssetDatabase.SaveAssets();
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, null, true);
        }

        private static void RemoveBroadImageEntry(AddressableAssetSettings settings)
        {
            RemoveEntryByAddress(settings, ImagesRoot);
        }

        private static void RemoveEntryByAddress(AddressableAssetSettings settings, string address)
        {
            foreach (var group in settings.groups)
            {
                if (group == null)
                    continue;

                AddressableAssetEntry found = null;
                foreach (var entry in group.entries)
                {
                    if (entry.address == address)
                    {
                        found = entry;
                        break;
                    }
                }

                if (found != null)
                    settings.RemoveAssetEntry(found.guid);
            }
        }

        private static void EnsureCoreGroup(AddressableAssetSettings settings)
        {
            var coreGroup = FindOrCreateLocalGroup(settings, CoreGroupName);
            var essentialPaths = new[]
            {
                "Assets/GameResource/Images/GameUI/v2",
                "Assets/GameResource/Images/GameUI/stage",
                "Assets/Fonts/BMJUA_ttf.ttf",
                "Assets/Fonts/BMJUA_ttf SDF.asset",
                "Assets/GameResource/Prefabs/AudioSource.prefab",
            };

            foreach (var folderOrAsset in essentialPaths)
                MovePathToGroup(settings, coreGroup, folderOrAsset);
        }

        private static void EnsureZoneArtGroups(AddressableAssetSettings settings)
        {
            for (var zone = 1; zone <= 8; zone++)
            {
                var groupName = $"ZoneArt_{zone:D2}";
                var group = FindOrCreateRemoteGroup(settings, groupName);
                var zoneFolder = $"Assets/GameResource/Images/GameUI/Zones/Zone{zone:D2}";
                EnsureFolderOnDisk(zoneFolder);

                var bannerPath = $"Assets/GameResource/Images/GameUI/v2/illust_zone_banner.png";
                if (zone == 1 && File.Exists(bannerPath))
                    MovePathToGroup(settings, group, bannerPath, $"ZoneArt_{zone:D2}/illust_zone_banner");

                MovePathToGroup(settings, group, zoneFolder);
                ApplyZoneLabel(group, groupName);
            }
        }

        private static void MoveLegacyArtToRemote(AddressableAssetSettings settings)
        {
            var remoteGroup = FindOrCreateRemoteGroup(settings, "LegacyArt_Remote");
            var legacyPaths = new[]
            {
                "Assets/GameResource/Images/GameUI/Modern",
                "Assets/GameResource/Images/GameUI/Borders",
            };

            foreach (var path in legacyPaths)
            {
                if (AssetDatabase.IsValidFolder(path) || File.Exists(path))
                    MovePathToGroup(settings, remoteGroup, path);
            }
        }

        private static void MovePathToGroup(
            AddressableAssetSettings settings,
            AddressableAssetGroup group,
            string assetPath,
            string addressOverride = null)
        {
            if (!AssetDatabase.IsValidFolder(assetPath) && !File.Exists(assetPath))
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = addressOverride ?? assetPath;
        }

        private static void ApplyZoneLabel(AddressableAssetGroup group, string label)
        {
            foreach (var entry in group.entries)
                entry.SetLabel(label, true, true);
        }

        private static AddressableAssetGroup FindOrCreateLocalGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = FindGroup(settings, groupName) ?? settings.CreateGroup(
                groupName, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            SetIncludeInBuild(group, true);
            return group;
        }

        private static AddressableAssetGroup FindOrCreateRemoteGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = FindGroup(settings, groupName) ?? settings.CreateGroup(
                groupName, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            SetIncludeInBuild(group, false);
            return group;
        }

        private static void SetIncludeInBuild(AddressableAssetGroup group, bool include)
        {
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema != null)
                schema.IncludeInBuild = include;
        }

        private static AddressableAssetGroup FindGroup(AddressableAssetSettings settings, string groupName)
        {
            foreach (var candidate in settings.groups)
            {
                if (candidate != null && candidate.Name == groupName)
                    return candidate;
            }

            return null;
        }

        private static void EnsureFolderOnDisk(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolderOnDisk(parent);

            if (!string.IsNullOrEmpty(parent))
                AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
