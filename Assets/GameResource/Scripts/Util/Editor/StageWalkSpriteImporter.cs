using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Backend.Util.Editor
{
    /// <summary>
    /// 전사 Walk 시트 임포트 설정 + InGame Addressable 등록.
    /// 프레임 분할은 런타임 <see cref="RuntimeStageSprites.GetFrames"/> 가 처리한다.
    /// </summary>
    public sealed class StageWalkSpriteImporter : AssetPostprocessor
    {
        private const string WalkAssetPath =
            "Assets/GameResource/Images/GameUI/stage/stage_party_warrior_walk.png";
        private const string InGameGroupName = "InGame";

        private void OnPreprocessTexture()
        {
            if (!IsWalkSheet(assetPath))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.npotScale = TextureImporterNPOTScale.None;
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (!IsWalkSheet(path))
                    continue;

                EnsureAddressable(path);
            }
        }

        private static void EnsureAddressable(string assetPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[StageWalkSpriteImporter] Addressable settings missing.");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            if (settings.FindAssetEntry(guid) != null)
                return;

            AddressableAssetGroup group = null;
            foreach (var candidate in settings.groups)
            {
                if (candidate != null && candidate.Name == InGameGroupName)
                {
                    group = candidate;
                    break;
                }
            }

            if (group == null)
            {
                Debug.LogWarning($"[StageWalkSpriteImporter] Addressable group '{InGameGroupName}' not found.");
                return;
            }

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = assetPath;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"[StageWalkSpriteImporter] Addressable registered: {assetPath}");
        }

        private static bool IsWalkSheet(string path) =>
            !string.IsNullOrEmpty(path) &&
            path.Replace('\\', '/') == WalkAssetPath;
    }
}
