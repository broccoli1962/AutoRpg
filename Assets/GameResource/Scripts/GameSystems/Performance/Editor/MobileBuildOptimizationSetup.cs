#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Backend.GameSystems.Performance.Editor
{
    /// <summary>
    /// 모바일 빌드 최적화: ASTC 텍스처·오디오 압축·코드 스트리핑을 적용한다.
    /// </summary>
    public static class MobileBuildOptimizationSetup
    {
        private const string ImagesRoot = "Assets/GameResource/Images";
        private const string SoundsRoot = "Assets/GameResource/Sounds";

        [MenuItem("Tools/Abyss Chronicle/Apply Mobile Build Optimizations")]
        public static void ApplyOptimizations()
        {
            ApplyAstcCompression();
            ApplyAudioCompression();
            ApplyCodeStripping();
            AssetDatabase.SaveAssets();
        }

        private static void ApplyAstcCompression()
        {
            if (!AssetDatabase.IsValidFolder(ImagesRoot))
                return;

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ImagesRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                ApplyAstcPlatform(importer, "Android");
                ApplyAstcPlatform(importer, "iPhone");
                importer.SaveAndReimport();
            }
        }

        private static void ApplyAstcPlatform(TextureImporter importer, string platform)
        {
            var settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true;
            settings.maxTextureSize = settings.maxTextureSize > 0 ? settings.maxTextureSize : 2048;
            settings.format = TextureImporterFormat.ASTC_6x6;
            settings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(settings);
        }

        private static void ApplyAudioCompression()
        {
            if (!AssetDatabase.IsValidFolder(SoundsRoot))
                return;

            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { SoundsRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                    continue;

                var isShort = Path.GetFileNameWithoutExtension(path).StartsWith("sfx_", System.StringComparison.OrdinalIgnoreCase)
                              || Path.GetFileNameWithoutExtension(path).StartsWith("pop", System.StringComparison.OrdinalIgnoreCase);

                var sampleSettings = importer.defaultSampleSettings;
                sampleSettings.loadType = isShort ? AudioClipLoadType.DecompressOnLoad : AudioClipLoadType.Streaming;
                sampleSettings.compressionFormat = isShort ? AudioCompressionFormat.ADPCM : AudioCompressionFormat.Vorbis;
                sampleSettings.quality = isShort ? 0.8f : 0.6f;
                importer.defaultSampleSettings = sampleSettings;

                ApplyAudioPlatform(importer, "Android", sampleSettings);
                ApplyAudioPlatform(importer, "iPhone", sampleSettings);
                importer.SaveAndReimport();
            }
        }

        private static void ApplyAudioPlatform(AudioImporter importer, string platform, AudioImporterSampleSettings sampleSettings)
        {
            importer.SetOverrideSampleSettings(platform, sampleSettings);
        }

        private static void ApplyCodeStripping()
        {
            PlayerSettings.stripEngineCode = true;

            var namedBuildTarget = NamedBuildTarget.Android;
            PlayerSettings.SetManagedStrippingLevel(namedBuildTarget, ManagedStrippingLevel.Medium);

            namedBuildTarget = NamedBuildTarget.iOS;
            PlayerSettings.SetManagedStrippingLevel(namedBuildTarget, ManagedStrippingLevel.Medium);
        }
    }
}
#endif
