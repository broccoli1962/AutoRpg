#if UNITY_EDITOR
using Backend.Services.RemoteConfig;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Backend.Services.Editor
{
    /// <summary>
    /// RemoteConfigDefaultsTable 에셋 생성 및 Addressables 등록.
    /// </summary>
    public static class BackendDataAssetCreator
    {
        private const string DefaultsPath = "Assets/GameResource/Data/Backend/RemoteConfigDefaultsTable.asset";
        private const string GroupName = "Backend";

        [MenuItem("Tools/Abyss Chronicle/Ensure Backend Data Assets")]
        public static void EnsureAssets()
        {
            EnsureFolder("Assets/GameResource/Data", "Backend");
            EnsureDefaultsTable();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureDefaultsTable()
        {
            var table = AssetDatabase.LoadAssetAtPath<RemoteConfigDefaultsTable>(DefaultsPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<RemoteConfigDefaultsTable>();
                table.ApplySpecDefaults();
                AssetDatabase.CreateAsset(table, DefaultsPath);
            }
            else
            {
                table.ApplySpecDefaults();
                EditorUtility.SetDirty(table);
            }

            EnsureAddressable(DefaultsPath);
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

            var group = settings.FindGroup(GroupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    GroupName,
                    false,
                    false,
                    true,
                    null,
                    typeof(BundledAssetGroupSchema));
            }

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
                settings.CreateOrMoveEntry(guid, group);
        }
    }
}
#endif
