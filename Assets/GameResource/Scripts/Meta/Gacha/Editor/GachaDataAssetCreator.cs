#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.Meta.Gacha.Editor
{
    /// <summary>
    /// GachaRateTable·GachaBannerPool 에셋이 없을 때 spec 기본값으로 생성하고 Addressables에 등록한다.
    /// </summary>
    public static class GachaDataAssetCreator
    {
        private const string RateTablePath = "Assets/GameResource/Data/Gacha/GachaRateTable.asset";
        private const string BannerPoolPath = "Assets/GameResource/Data/Gacha/GachaBannerPool.asset";
        private const string GroupName = "Gacha";

        [MenuItem("Tools/Abyss Chronicle/Ensure Gacha Data Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Data", "Gacha");
            EnsureRateTable();
            EnsureBannerPool();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureRateTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<GachaRateTable>(RateTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<GachaRateTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, RateTablePath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }

            EnsureAddressable(RateTablePath);
        }

        private static void EnsureBannerPool()
        {
            var pool = AssetDatabase.LoadAssetAtPath<GachaBannerPool>(BannerPoolPath);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<GachaBannerPool>();
                AssetDatabase.CreateAsset(pool, BannerPoolPath);
            }

            EnsureAddressable(BannerPoolPath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    var grandParent = parent.Substring(0, parent.LastIndexOf('/'));
                    var parentName = parent.Substring(parent.LastIndexOf('/') + 1);
                    AssetDatabase.CreateFolder(grandParent, parentName);
                }

                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void EnsureAddressable(string assetPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var existingEntry = settings.FindAssetEntry(guid);
            if (existingEntry != null && existingEntry.address == assetPath)
                return;

            var group = FindOrCreateGroup(settings);
            if (group == null)
                return;

            var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
            entry.address = assetPath;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }

        private static AddressableAssetGroup FindOrCreateGroup(AddressableAssetSettings settings)
        {
            foreach (var candidate in settings.groups)
            {
                if (candidate != null && candidate.Name == GroupName)
                    return candidate;
            }

            return settings.CreateGroup(
                GroupName,
                false,
                false,
                true,
                null,
                typeof(BundledAssetGroupSchema));
        }
    }
}
#endif
