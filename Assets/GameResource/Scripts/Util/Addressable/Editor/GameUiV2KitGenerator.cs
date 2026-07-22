#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Backend.Editor
{
    /// <summary>
    /// 12_UIUX v2 — GameUI/v2 절차적 9-slice·아이콘·일러스트 생성 (PPU 100).
    /// </summary>
    public static class GameUiV2KitGenerator
    {
        private const string OutputFolder = "Assets/GameResource/Images/GameUI/v2";
        private const float PixelsPerUnit = 100f;
        private static readonly Color PanelFill = new(0.07f, 0.09f, 0.14f, 0.94f);
        private static readonly Color PanelStroke = new(0.23f, 0.29f, 0.4f, 1f);
        private static readonly Color GoldAccent = new(0.96f, 0.84f, 0.45f, 1f);
        private static readonly Color CyanAccent = new(0.43f, 0.81f, 0.96f, 1f);

        [MenuItem("Tools/UI/Generate GameUI v2 Kit")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputFolder);

            SaveSliced("ui_panel_l", 128, 128, 24, 24, 24, 24, CreatePanelTexture(PanelFill, PanelStroke, GoldAccent * 0.45f));
            SaveSliced("ui_panel_s", 128, 128, 20, 20, 20, 20, CreatePanelTexture(
                new Color(0.08f, 0.11f, 0.16f, 0.92f),
                new Color(0.2f, 0.26f, 0.36f, 0.9f),
                CyanAccent * 0.35f));
            SaveSliced("ui_bar_top", 256, 64, 16, 12, 16, 12, CreateBarTexture(true));
            SaveSliced("ui_bar_bottom", 256, 72, 16, 12, 16, 16, CreateBarTexture(false));
            SaveSliced("ui_btn_primary", 192, 64, 28, 20, 28, 20, CreateButtonTexture(
                new Color(0.72f, 0.52f, 0.16f, 1f),
                new Color(0.96f, 0.78f, 0.28f, 1f)));
            SaveSliced("ui_btn_secondary", 192, 56, 24, 16, 24, 16, CreateButtonTexture(
                new Color(0.12f, 0.16f, 0.24f, 1f),
                new Color(0.24f, 0.32f, 0.46f, 1f)));
            SaveSliced("ui_tab_on", 160, 72, 20, 16, 20, 20, CreateTabTexture(true));
            SaveSliced("ui_tab_off", 160, 72, 20, 16, 20, 20, CreateTabTexture(false));
            SaveSliced("ui_progress_track", 256, 32, 12, 8, 12, 8, CreateProgressTrackTexture());
            SaveSliced("ui_progress_fill", 256, 32, 12, 8, 12, 8, CreateProgressFillTexture());

            SaveSimple("portrait_frame", 96, 96, 0, 0, 0, 0, CreatePortraitFrameTexture());
            SaveSimple("log_accent_strip", 4, 32, 0, 0, 0, 0, CreateAccentStripTexture());

            SaveSimple("icon_tab_explore", 56, 56, 0, 0, 0, 0, CreateCircleIcon(CyanAccent));
            SaveSimple("icon_tab_enhance", 56, 56, 0, 0, 0, 0, CreateCircleIcon(GoldAccent));
            SaveSimple("icon_tab_guild", 56, 56, 0, 0, 0, 0, CreateCircleIcon(new Color(0.43f, 0.88f, 0.54f)));
            SaveSimple("icon_tab_chronicle", 56, 56, 0, 0, 0, 0, CreateCircleIcon(new Color(0.72f, 0.55f, 0.95f)));
            SaveSimple("icon_tab_compendium", 56, 56, 0, 0, 0, 0, CreateCircleIcon(new Color(0.95f, 0.62f, 0.42f)));

            SaveSimple("icon_log_combat", 48, 48, 0, 0, 0, 0, CreateLogIcon(new Color(0.95f, 0.43f, 0.36f)));
            SaveSimple("icon_log_discovery", 48, 48, 0, 0, 0, 0, CreateLogIcon(new Color(0.43f, 0.88f, 0.54f)));
            SaveSimple("icon_log_event", 48, 48, 0, 0, 0, 0, CreateLogIcon(GoldAccent));
            SaveSimple("icon_log_narrative", 48, 48, 0, 0, 0, 0, CreateLogIcon(CyanAccent));

            GenerateIllustrations();
            RegisterAddressables();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[GameUiV2KitGenerator] Generated v2 UI kit in {OutputFolder}");
        }

        private static void RegisterAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[GameUiV2KitGenerator] AddressableAssetSettings not found; skip registration.");
                return;
            }

            var group = settings.FindGroup("InGame");
            if (group == null)
            {
                group = settings.CreateGroup(
                    "InGame",
                    false,
                    false,
                    false,
                    null,
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                    typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));
            }

            RegisterAsset(settings, group, "Assets/Fonts/BMJUA_ttf.ttf");
            foreach (var file in Directory.GetFiles(OutputFolder, "*.png"))
                RegisterAsset(settings, group, file.Replace('\\', '/'));

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        }

        private static void RegisterAsset(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (entry == null)
                return;

            entry.address = assetPath;
        }

        private static void GenerateIllustrations()
        {
            SaveIllustration("illust_guild_start", 560, 200, TryLoadModern("GuildStartHero"), CreateGradientIllustration(560, 200,
                new Color(0.1f, 0.16f, 0.24f), new Color(0.18f, 0.28f, 0.42f), GoldAccent));
            SaveIllustration("illust_zone_banner", 1184, 160, TryLoadModern("ZoneBannerMossyHollow"), CreateGradientIllustration(1184, 160,
                new Color(0.08f, 0.18f, 0.14f), new Color(0.14f, 0.28f, 0.22f), CyanAccent));
        }

        private static Texture2D TryLoadModern(string name)
        {
            var path = $"Assets/GameResource/Images/GameUI/Modern/{name}.png";
            if (!File.Exists(path))
                return null;

            var bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
                return null;

            return tex;
        }

        private static void SaveIllustration(string name, int width, int height, Texture2D source, Texture2D fallback)
        {
            var tex = source != null ? ResizeTexture(source, width, height) : fallback;
            SaveSimple(name, width, height, 0, 0, 0, 0, tex);
            if (source != null)
                global::UnityEngine.Object.DestroyImmediate(source);
        }

        private static Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        private static void SaveSliced(
            string name,
            int width,
            int height,
            int borderLeft,
            int borderTop,
            int borderRight,
            int borderBottom,
            Texture2D texture)
        {
            SaveTexture(name, texture);
            ConfigureImporter($"{OutputFolder}/{name}.png", borderLeft, borderTop, borderRight, borderBottom);
        }

        private static void SaveSimple(
            string name,
            int width,
            int height,
            int borderLeft,
            int borderTop,
            int borderRight,
            int borderBottom,
            Texture2D texture)
        {
            SaveTexture(name, texture);
            ConfigureImporter($"{OutputFolder}/{name}.png", borderLeft, borderTop, borderRight, borderBottom);
        }

        private static void SaveTexture(string name, Texture2D texture)
        {
            var path = $"{OutputFolder}/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            global::UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void ConfigureImporter(string path, int borderLeft, int borderTop, int borderRight, int borderBottom)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            if (borderLeft > 0 || borderTop > 0 || borderRight > 0 || borderBottom > 0)
                importer.spriteBorder = new Vector4(borderLeft, borderTop, borderRight, borderBottom);

            importer.SaveAndReimport();
        }

        private static Texture2D CreatePanelTexture(Color fill, Color border, Color accent)
        {
            const int size = 128;
            var tex = NewTexture(size, size);
            Fill(tex, fill);
            DrawBorderRing(tex, 24, border);
            DrawHorizontalGradientLine(tex, 24, 26, accent);
            DrawCornerGlow(tex, accent, 0.25f, 24);
            return tex;
        }

        private static Texture2D CreateBarTexture(bool topBar)
        {
            var width = 256;
            var height = topBar ? 64 : 72;
            var tex = NewTexture(width, height);
            for (var y = 0; y < height; y++)
            {
                var t = y / (float)(height - 1);
                var c = Color.Lerp(new Color(0.1f, 0.13f, 0.2f, 0.98f), new Color(0.07f, 0.09f, 0.14f, 0.94f), t);
                for (var x = 0; x < width; x++)
                    tex.SetPixel(x, y, c);
            }

            DrawHorizontalGradientLine(tex, 4, 6, GoldAccent * 0.55f);
            DrawBorderRing(tex, 12, PanelStroke * 0.85f);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateButtonTexture(Color bottom, Color top)
        {
            var tex = NewTexture(192, 64);
            for (var y = 0; y < 64; y++)
            {
                var t = y / 63f;
                var c = Color.Lerp(bottom, top, t);
                for (var x = 0; x < 192; x++)
                    tex.SetPixel(x, y, c);
            }

            DrawBorderRing(tex, 20, new Color(1f, 1f, 1f, 0.16f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateTabTexture(bool active)
        {
            var fill = active ? new Color(0.14f, 0.19f, 0.28f, 0.98f) : new Color(0.08f, 0.1f, 0.15f, 0.88f);
            var border = active ? GoldAccent : PanelStroke * 0.75f;
            var tex = NewTexture(160, 72);
            Fill(tex, fill);
            DrawBorderRing(tex, 16, border);
            if (active)
                DrawHorizontalGradientLine(tex, 8, 12, GoldAccent * 0.85f);
            return tex;
        }

        private static Texture2D CreateProgressTrackTexture()
        {
            var tex = NewTexture(256, 32);
            Fill(tex, new Color(0.1f, 0.13f, 0.19f, 1f));
            DrawBorderRing(tex, 8, new Color(0.16f, 0.2f, 0.28f, 1f));
            return tex;
        }

        private static Texture2D CreateProgressFillTexture()
        {
            var tex = NewTexture(256, 32);
            for (var y = 0; y < 32; y++)
            {
                var t = y / 31f;
                var c = Color.Lerp(new Color(0.18f, 0.62f, 0.42f, 1f), new Color(0.43f, 0.88f, 0.54f, 1f), t);
                for (var x = 0; x < 256; x++)
                    tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePortraitFrameTexture()
        {
            var tex = NewTexture(96, 96);
            Fill(tex, new Color(0.05f, 0.07f, 0.1f, 0.35f));
            DrawBorderRing(tex, 10, GoldAccent);
            DrawInnerInset(tex, 14, new Color(0.04f, 0.05f, 0.08f, 0.55f));
            return tex;
        }

        private static Texture2D CreateAccentStripTexture()
        {
            var tex = NewTexture(4, 32);
            for (var y = 0; y < 32; y++)
            {
                var t = y / 31f;
                var c = Color.Lerp(CyanAccent, GoldAccent, t);
                for (var x = 0; x < 4; x++)
                    tex.SetPixel(x, y, c);
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateGradientIllustration(int width, int height, Color bottom, Color top, Color accent)
        {
            var tex = NewTexture(width, height);
            for (var y = 0; y < height; y++)
            {
                var t = y / (float)(height - 1);
                var c = Color.Lerp(bottom, top, t);
                for (var x = 0; x < width; x++)
                    tex.SetPixel(x, y, c);
            }

            DrawHorizontalGradientLine(tex, height - 8, height - 4, accent * 0.65f);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleIcon(Color accent)
        {
            var tex = NewTexture(56, 56);
            FillRect(tex, 0, 0, 56, 56, Color.clear);
            DrawFilledCircle(tex, 28, 28, 22, accent * 0.25f);
            DrawFilledCircle(tex, 28, 28, 16, accent);
            DrawFilledCircle(tex, 28, 28, 10, new Color(0.08f, 0.1f, 0.14f, 0.85f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateLogIcon(Color accent)
        {
            var tex = NewTexture(48, 48);
            FillRect(tex, 0, 0, 48, 48, Color.clear);
            DrawFilledCircle(tex, 24, 24, 18, accent * 0.25f);
            DrawFilledCircle(tex, 24, 24, 11, accent);
            tex.Apply();
            return tex;
        }

        private static Texture2D NewTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            return tex;
        }

        private static void Fill(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
        }

        private static void FillRect(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
        {
            for (var y = y0; y < y1; y++)
            for (var x = x0; x < x1; x++)
                tex.SetPixel(x, y, color);
        }

        private static void DrawBorderRing(Texture2D tex, int thickness, Color color)
        {
            var width = tex.width;
            var height = tex.height;
            for (var i = 0; i < thickness; i++)
            {
                for (var x = 0; x < width; x++)
                {
                    tex.SetPixel(x, i, color);
                    tex.SetPixel(x, height - 1 - i, color);
                }

                for (var y = 0; y < height; y++)
                {
                    tex.SetPixel(i, y, color);
                    tex.SetPixel(width - 1 - i, y, color);
                }
            }

            tex.Apply();
        }

        private static void DrawInnerInset(Texture2D tex, int inset, Color color)
        {
            var size = tex.width;
            for (var y = inset; y < size - inset; y++)
            for (var x = inset; x < size - inset; x++)
                tex.SetPixel(x, y, Color.Lerp(tex.GetPixel(x, y), color, 0.65f));
            tex.Apply();
        }

        private static void DrawHorizontalGradientLine(Texture2D tex, int y0, int y1, Color color)
        {
            var width = tex.width;
            var border = Mathf.Min(16, width / 8);
            for (var y = y0; y <= y1 && y < tex.height; y++)
            for (var x = border; x < width - border; x++)
                tex.SetPixel(x, y, Color.Lerp(tex.GetPixel(x, y), color, color.a));
            tex.Apply();
        }

        private static void DrawCornerGlow(Texture2D tex, Color accent, float strength, int radius)
        {
            var width = tex.width;
            var height = tex.height;
            var corners = new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(width - 1, 0),
                new Vector2Int(0, height - 1),
                new Vector2Int(width - 1, height - 1)
            };

            foreach (var corner in corners)
            {
                for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var dx = x - corner.x;
                    var dy = y - corner.y;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius)
                        continue;

                    var t = 1f - dist / radius;
                    var existing = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, Color.Lerp(existing, accent, t * strength));
                }
            }

            tex.Apply();
        }

        private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            for (var y = cy - radius; y <= cy + radius; y++)
            for (var x = cx - radius; x <= cx + radius; x++)
            {
                if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
                    continue;

                var dx = x - cx;
                var dy = y - cy;
                if (dx * dx + dy * dy <= radius * radius)
                    tex.SetPixel(x, y, color);
            }
        }
    }
}
#endif
