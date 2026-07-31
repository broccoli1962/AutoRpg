#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.Simulation.Editor
{
    /// <summary>
    /// BalanceTable 에셋이 없을 때 spec 기본값으로 생성하고 Addressables에 등록한다.
    /// </summary>
    public static class BalanceTableAssetCreator
    {
        private const string ASSET_PATH = "Assets/GameResource/Data/Balance/BalanceTable.asset";
        private const string GroupName = "Balance";

        [MenuItem("Tools/Abyss Chronicle/Ensure Balance Table Asset")]
        public static void EnsureAsset()
        {
            var table = AssetDatabase.LoadAssetAtPath<BalanceTable>(ASSET_PATH);
            if (table == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/GameResource/Data/Balance"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/GameResource/Data"))
                        AssetDatabase.CreateFolder("Assets/GameResource", "Data");

                    AssetDatabase.CreateFolder("Assets/GameResource/Data", "Balance");
                }

                table = ScriptableObject.CreateInstance<BalanceTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, ASSET_PATH);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }

            EnsureAddressable(ASSET_PATH);
            AssetDatabase.SaveAssets();
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
