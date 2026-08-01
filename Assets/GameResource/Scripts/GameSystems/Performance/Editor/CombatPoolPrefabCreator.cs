#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Performance.Editor
{
    /// <summary>
    /// 전투 연출 풀 프리팹을 생성하고 Addressables CombatPool 그룹에 등록한다.
    /// </summary>
    public static class CombatPoolPrefabCreator
    {
        private const string PrefabFolder = "Assets/GameResource/Prefabs/CombatPool";
        private const string GroupName = "CombatPool";

        [MenuItem("Tools/Abyss Chronicle/Ensure Combat Pool Prefabs")]
        public static void EnsurePrefabs()
        {
            EnsureFolder("Assets/GameResource/Prefabs", "CombatPool");

            EnsureSpritePrefab("MonsterSprite", typeof(PooledCombatSprite));
            EnsureDamageTextPrefab();
            EnsureSpritePrefab("DropIcon", typeof(PooledCombatSprite));
            EnsureHitVfxPrefab();

            AssetDatabase.SaveAssets();
        }

        private static void EnsureSpritePrefab(string name, System.Type componentType)
        {
            var path = $"{PrefabFolder}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                EnsureAddressable(path);
                return;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), componentType);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(96f, 96f);

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            EnsureAddressable(path);
        }

        private static void EnsureDamageTextPrefab()
        {
            const string name = "DamageText";
            var path = $"{PrefabFolder}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                EnsureAddressable(path);
                return;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(PooledDamageText));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 48f);

            var label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = 28f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            EnsureAddressable(path);
        }

        private static void EnsureHitVfxPrefab()
        {
            const string name = "HitVfx";
            var path = $"{PrefabFolder}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                EnsureAddressable(path);
                return;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(PooledHitVfx));
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(64f, 64f);

            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            EnsureAddressable(path);
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
