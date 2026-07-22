using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI
{
    /// <summary>
    /// UI 가 배치될 Canvas 레이어를 관리한다.
    /// 씬에 한 번 배치(예: UIRoot) 하고, Inspector 에서 UILayer 별 RectTransform 을 매핑한다.
    /// </summary>
    public class UIRegistry : MonoBehaviour
    {
        [Serializable]
        private struct LayerEntry
        {
            public UILayer layer;
            public RectTransform root;
        }

        [SerializeField] private List<LayerEntry> _layers = new();

        private Dictionary<UILayer, RectTransform> _lookup;

        private void Awake()
        {
            BuildLookup();
        }

        /// <summary>
        /// Addressable UIRoot 없이 Play 할 때 사용하는 표준 Canvas 레이어 루트.
        /// </summary>
        public static UIRegistry CreateStandardRoot()
        {
            var go = new GameObject("UIRoot");
            DontDestroyOnLoad(go);

            var rootRect = go.AddComponent<RectTransform>();
            StretchFull(rootRect);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0f;

            go.AddComponent<GraphicRaycaster>();

            var hudRoot = CreateLayerRoot(rootRect, "Layer_HUD");
            var panelRoot = CreateLayerRoot(rootRect, "Layer_Panel");
            var navigationRoot = CreateLayerRoot(rootRect, "Layer_Navigation");
            var popupRoot = CreateLayerRoot(rootRect, "Layer_Popup");

            var registry = go.AddComponent<UIRegistry>();
            registry.ConfigureLayers(new Dictionary<UILayer, RectTransform>
            {
                { UILayer.HUD, hudRoot },
                { UILayer.Panel, panelRoot },
                { UILayer.Navigation, navigationRoot },
                { UILayer.Popup, popupRoot },
            });

            return registry;
        }

        public void ConfigureLayers(Dictionary<UILayer, RectTransform> layerRoots)
        {
            _layers.Clear();
            foreach (var pair in layerRoots)
            {
                _layers.Add(new LayerEntry
                {
                    layer = pair.Key,
                    root = pair.Value
                });
            }

            BuildLookup();
        }

        private static RectTransform CreateLayerRoot(RectTransform parent, string name)
        {
            var layerGo = new GameObject(name, typeof(RectTransform));
            layerGo.transform.SetParent(parent, false);
            var rect = layerGo.GetComponent<RectTransform>();
            StretchFull(rect);
            return rect;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<UILayer, RectTransform>(_layers.Count);
            for (int i = 0; i < _layers.Count; i++)
            {
                var entry = _layers[i];
                if (entry.root == null)
                {
                    Debug.LogWarning($"[UIRegistry] Layer '{entry.layer}' has no RectTransform assigned.");
                    continue;
                }
                _lookup[entry.layer] = entry.root;
            }
        }

        public RectTransform GetRoot(UILayer layer)
        {
            if (_lookup == null) BuildLookup();
            return _lookup != null && _lookup.TryGetValue(layer, out var root) ? root : null;
        }
    }
}
