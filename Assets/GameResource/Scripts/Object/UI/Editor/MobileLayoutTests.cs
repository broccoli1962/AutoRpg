#if UNITY_EDITOR
using Backend.Object.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI.Editor.Tests
{
    public class MobileLayoutTests
    {
        private static readonly (float width, float height, string label)[] PortraitAspects =
        {
            (1080f, 2160f, "18:9"),
            (1080f, 2400f, "20:9"),
        };

        [Test]
        public void MobileCanvasScaler_UsesPortraitReference()
        {
            var go = new GameObject("Canvas");
            var scaler = go.AddComponent<CanvasScaler>();
            MobileCanvasScaler.Apply(scaler);

            Assert.AreEqual(MobileCanvasScaler.ReferenceResolution, scaler.referenceResolution);
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(MobileCanvasScaler.MatchWidthOrHeight, scaler.matchWidthOrHeight);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ExplorationHudBands_DoNotOverlap_OnCommonPortraitAspects()
        {
            foreach (var aspect in PortraitAspects)
            {
                var root = BuildTestHud(aspect.width, aspect.height);
                try
                {
                    AssertBandOrder(root, "TopBar", "StageArea", aspect.label);
                    AssertBandOrder(root, "StageArea", "LogStrip", aspect.label);
                    AssertBandOrder(root, "LogStrip", "ActionBar", aspect.label);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        [Test]
        public void TouchButtons_MeetMinimumHeight()
        {
            var go = new GameObject("Btn", typeof(RectTransform));
            go.AddComponent<Image>();
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 40f);
            go.AddComponent<TouchTargetSize>().Apply();

            Assert.GreaterOrEqual(rect.sizeDelta.y, TouchTargetSize.MinButtonHeight);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static GameObject BuildTestHud(float width, float height)
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(CanvasScaler));
            var canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(width, height);
            MobileCanvasScaler.Apply(canvasGo.GetComponent<CanvasScaler>());

            var root = new GameObject("ExplorationHudPanel", typeof(RectTransform));
            root.transform.SetParent(canvasGo.transform, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateBand(root.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -128f), Vector2.zero);
            CreateBand(root.transform, "StageArea", Vector2.zero, Vector2.one, new Vector2(0f, 320f), new Vector2(0f, -128f));
            CreateBand(root.transform, "LogStrip", Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 128f), new Vector2(0f, 192f));
            CreateBand(root.transform, "ActionBar", Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 128f));

            Canvas.ForceUpdateCanvases();
            return canvasGo;
        }

        private static GameObject CreateBand(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return go;
        }

        private static void AssertBandOrder(GameObject canvasRoot, string upperName, string lowerName, string aspectLabel)
        {
            var upper = canvasRoot.transform.Find($"ExplorationHudPanel/{upperName}") as RectTransform;
            var lower = canvasRoot.transform.Find($"ExplorationHudPanel/{lowerName}") as RectTransform;
            Assert.NotNull(upper, $"{aspectLabel}: missing {upperName}");
            Assert.NotNull(lower, $"{aspectLabel}: missing {lowerName}");

            var upperBottom = upper.offsetMin.y;
            var lowerTop = lower.offsetMax.y;
            Assert.GreaterOrEqual(upperBottom, lowerTop,
                $"{aspectLabel}: {upperName} overlaps {lowerName} (upperBottom={upperBottom}, lowerTop={lowerTop})");
        }
    }
}
#endif
