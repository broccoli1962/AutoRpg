#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.GameSystems.Performance.Editor
{
    /// <summary>
    /// PerformancePolicyTable 에셋 생성 및 Addressables 등록.
    /// </summary>
    public static class PerformanceDataAssetCreator
    {
        private const string PolicyTablePath = "Assets/GameResource/Data/Performance/PerformancePolicyTable.asset";
        private const string GroupName = "Performance";

        [MenuItem("Tools/Abyss Chronicle/Ensure Performance Data Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Data", "Performance");

            var table = AssetDatabase.LoadAssetAtPath<PerformancePolicyTable>(PolicyTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<PerformancePolicyTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, PolicyTablePath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }

            EnsureAddressable(PolicyTablePath);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (AssetDatabase.IsValidFolder($"{parent}/{child}"))
                return;

            if (!AssetDatabase.IsValidFolder(parent))
            {
                var slashIndex = parent.LastIndexOf('/');
                AssetDatabase.CreateFolder(parent.Substring(0, slashIndex), parent.Substring(slashIndex + 1));
            }

            AssetDatabase.CreateFolder(parent, child);
        }

        private static void EnsureAddressable(string assetPath)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                return;

            var group = FindOrCreateGroup(settings);
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

            return settings.CreateGroup(GroupName, false, false, true, null, typeof(BundledAssetGroupSchema));
        }
    }
}
#endif
