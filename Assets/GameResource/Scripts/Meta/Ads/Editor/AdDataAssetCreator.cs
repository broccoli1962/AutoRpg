#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.Meta.Ads.Editor
{
    /// <summary>
    /// AdConfigTable 에셋이 없을 때 spec 기본값으로 생성하고 Addressables에 등록한다.
    /// </summary>
    public static class AdDataAssetCreator
    {
        private const string AdConfigTablePath = "Assets/GameResource/Data/Ads/AdConfigTable.asset";
        private const string GroupName = "Ads";

        [MenuItem("Tools/Abyss Chronicle/Ensure Ad Data Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Data", "Ads");
            EnsureAdConfigTable();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureAdConfigTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<AdConfigTable>(AdConfigTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<AdConfigTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, AdConfigTablePath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }

            EnsureAddressable(AdConfigTablePath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    var slashIndex = parent.LastIndexOf('/');
                    var grandParent = parent.Substring(0, slashIndex);
                    var parentName = parent.Substring(slashIndex + 1);
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
