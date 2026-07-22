#if UNITY_EDITOR
using Backend.Object.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Editor
{
    /// <summary>
    /// UIRegistry + 레이어 Canvas 루트를 가진 UIRoot Addressable 프리팹을 생성한다.
    /// </summary>
    public static class UIRootPrefabBuilder
    {
        private const string PrefabPath = "Assets/GameResource/Prefabs/UI/UIRoot.prefab";
        private const string Address = "UI/UIRoot.prefab";

        [MenuItem("Tools/Addressables/Build UI Root Prefab")]
        public static void BuildPrefab()
        {
            var root = new GameObject("UIRoot", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            StretchFull(rootRect);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0f;

            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<SafeArea>();

            var hudRoot = CreateLayerRoot(rootRect, "Layer_HUD");
            var panelRoot = CreateLayerRoot(rootRect, "Layer_Panel");
            var navigationRoot = CreateLayerRoot(rootRect, "Layer_Navigation");
            var popupRoot = CreateLayerRoot(rootRect, "Layer_Popup");

            var registry = root.AddComponent<UIRegistry>();
            WireRegistry(registry, hudRoot, panelRoot, navigationRoot, popupRoot);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UIRootPrefabBuilder] Saved {PrefabPath}");
        }

        [MenuItem("Tools/Addressables/Register UI Root")]
        public static void RegisterPrefab()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[UIRootPrefabBuilder] AddressableAssetSettings not found.");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[UIRootPrefabBuilder] Prefab not found: {PrefabPath}. Run Build UI Root Prefab first.");
                return;
            }

            var group = settings.FindGroup("UI");
            if (group == null)
            {
                Debug.LogError("[UIRootPrefabBuilder] UI addressable group not found.");
                return;
            }

            var guid = AssetDatabase.AssetPathToGUID(PrefabPath);
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry == null)
            {
                Debug.LogError("[UIRootPrefabBuilder] Failed to create addressable entry.");
                return;
            }

            entry.address = Address;
            settings.SetDirty(UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();

            Debug.Log($"[UIRootPrefabBuilder] Registered {PrefabPath} as '{Address}'.");
        }

        [MenuItem("Tools/Addressables/Build And Register UI Root")]
        public static void BuildAndRegister()
        {
            BuildPrefab();
            RegisterPrefab();
        }

        private static RectTransform CreateLayerRoot(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            StretchFull(rect);
            return rect;
        }

        private static void WireRegistry(
            UIRegistry registry,
            RectTransform hud,
            RectTransform panel,
            RectTransform navigation,
            RectTransform popup)
        {
            var so = new SerializedObject(registry);
            var layers = so.FindProperty("_layers");
            layers.arraySize = 4;
            SetLayerEntry(layers, 0, UILayer.HUD, hud);
            SetLayerEntry(layers, 1, UILayer.Panel, panel);
            SetLayerEntry(layers, 2, UILayer.Navigation, navigation);
            SetLayerEntry(layers, 3, UILayer.Popup, popup);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetLayerEntry(SerializedProperty array, int index, UILayer layer, RectTransform root)
        {
            var element = array.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("layer").enumValueIndex = (int)layer;
            element.FindPropertyRelative("root").objectReferenceValue = root;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
