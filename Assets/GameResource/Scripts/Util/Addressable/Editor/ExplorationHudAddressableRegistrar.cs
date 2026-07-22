#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Backend.Editor
{
    /// <summary>
    /// ExplorationHudPanel 프리팹을 Addressables UI 그룹에 등록한다.
    /// </summary>
    public static class ExplorationHudAddressableRegistrar
    {
        private const string PrefabPath = "Assets/GameResource/Prefabs/UI/ExplorationHudPanel.prefab";
        private const string Address = "UI/ExplorationHudPanel.prefab";
        private const string GroupName = "UI";

        [MenuItem("Tools/Addressables/Register Exploration HUD Panel")]
        public static void RegisterExplorationHudPanel()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[ExplorationHudAddressableRegistrar] AddressableAssetSettings not found.");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[ExplorationHudAddressableRegistrar] Prefab not found: {PrefabPath}");
                return;
            }

            var group = settings.FindGroup(GroupName);
            if (group == null)
            {
                group = settings.CreateGroup(
                    GroupName,
                    false,
                    false,
                    false,
                    null,
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));
            }

            var guid = AssetDatabase.AssetPathToGUID(PrefabPath);
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry == null)
            {
                Debug.LogError("[ExplorationHudAddressableRegistrar] Failed to create addressable entry.");
                return;
            }

            entry.address = Address;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();

            Debug.Log($"[ExplorationHudAddressableRegistrar] Registered {PrefabPath} as '{Address}' in group '{GroupName}'.");
        }
    }
}
#endif
