#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Backend.Util.Localization.Editor
{
    /// <summary>
    /// 한글·일본어 TMP SDF 및 Fallback 체인을 생성·연결한다.
    /// </summary>
    public static class LocalizationFontSetup
    {
        private const string MENU = "Tools/Abyss Chronicle/Setup Localization Fonts";

        [MenuItem(MENU)]
        public static void SetupFonts()
        {
            var koreanPath = "Assets/Fonts/BMJUA_ttf SDF.asset";
            var japanesePath = "Assets/Fonts/NotoSansJP-Regular SDF.asset";
            var latinPath = "Assets/Fonts/NotoSans-Regular SDF.asset";

            var koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(koreanPath);
            var japaneseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(japanesePath);
            var latinFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(latinPath);

            if (koreanFont == null)
            {
                EditorUtility.DisplayDialog(
                    "Localization Fonts",
                    $"Missing Korean font asset:\n{koreanPath}",
                    "OK");
                return;
            }

            ApplyFallbackChain(koreanFont, japaneseFont, latinFont);
            CreateOrUpdateSettingsAsset(koreanFont, japaneseFont, latinFont);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Abyss Chronicle/Import Localization Font Sources")]
        public static void ImportFontSources()
        {
            EnsureFontDirectory();
            ImportSourceFont(
                "Assets/Fonts/NotoSans-Regular.ttf",
                "https://github.com/googlefonts/noto-fonts/raw/main/hinted/ttf/NotoSans/NotoSans-Regular.ttf");

            AssetDatabase.Refresh();

            var japaneseSource = ResolveJapaneseSourceFontPath();
            CreateSdfAsset(japaneseSource, "Assets/Fonts/NotoSansJP-Regular SDF.asset");
            CreateSdfAsset("Assets/Fonts/NotoSans-Regular.ttf", "Assets/Fonts/NotoSans-Regular SDF.asset");
            SetupFonts();
        }

        private static string ResolveJapaneseSourceFontPath()
        {
            const string bundledTtc = "Assets/Fonts/YuGothM.ttc";
            if (System.IO.File.Exists(bundledTtc))
                return bundledTtc;

            const string bundledTtf = "Assets/Fonts/NotoSansJP-Regular.ttf";
            return bundledTtf;
        }

        private static void EnsureFontDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Fonts"))
                AssetDatabase.CreateFolder("Assets", "Fonts");
        }

        private static void ImportSourceFont(string assetPath, string url)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null)
                return;

            var request = UnityEngine.Networking.UnityWebRequest.Get(url);
            request.SendWebRequest();
            while (!request.isDone)
            {
            }

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog(
                    "Localization Fonts",
                    $"Failed to download font:\n{url}\n\n{request.error}",
                    "OK");
                return;
            }

            System.IO.File.WriteAllBytes(assetPath, request.downloadHandler.data);
            AssetDatabase.ImportAsset(assetPath);
        }

        private static void CreateSdfAsset(string sourceFontPath, string sdfAssetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfAssetPath) != null)
                return;

            EnsureFontImportSettings(sourceFontPath);

            var source = LoadSourceFont(sourceFontPath);
            if (source == null)
            {
                EditorUtility.DisplayDialog(
                    "Localization Fonts",
                    $"Source font not found:\n{sourceFontPath}",
                    "OK");
                return;
            }

            var sdf = TMP_FontAsset.CreateFontAsset(source);
            if (sdf == null)
            {
                EditorUtility.DisplayDialog(
                    "Localization Fonts",
                    $"Failed to create TMP font asset:\n{sourceFontPath}",
                    "OK");
                return;
            }

            AssetDatabase.CreateAsset(sdf, sdfAssetPath);
        }

        private static Font LoadSourceFont(string sourceFontPath)
        {
            var direct = AssetDatabase.LoadAssetAtPath<Font>(sourceFontPath);
            if (direct != null)
                return direct;

            var assets = AssetDatabase.LoadAllAssetsAtPath(sourceFontPath);
            if (assets == null)
                return null;

            foreach (var asset in assets)
            {
                if (asset is Font font)
                    return font;
            }

            return null;
        }

        private static void EnsureFontImportSettings(string sourceFontPath)
        {
            var importer = AssetImporter.GetAtPath(sourceFontPath);
            if (importer is TrueTypeFontImporter ttfImporter)
            {
                ttfImporter.includeFontData = true;
                ttfImporter.fontTextureCase = FontTextureCase.Dynamic;
                ttfImporter.SaveAndReimport();
                return;
            }

            if (importer != null)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ApplyFallbackChain(
            TMP_FontAsset koreanFont,
            TMP_FontAsset japaneseFont,
            TMP_FontAsset latinFont)
        {
            if (koreanFont == null)
                return;

            var fallbacks = new System.Collections.Generic.List<TMP_FontAsset>();
            if (japaneseFont != null && japaneseFont != koreanFont)
                fallbacks.Add(japaneseFont);
            if (latinFont != null && latinFont != koreanFont && latinFont != japaneseFont)
                fallbacks.Add(latinFont);

            koreanFont.fallbackFontAssetTable = fallbacks;

            if (japaneseFont != null)
            {
                var jpFallbacks = new System.Collections.Generic.List<TMP_FontAsset> { koreanFont };
                if (latinFont != null && latinFont != japaneseFont)
                    jpFallbacks.Add(latinFont);
                japaneseFont.fallbackFontAssetTable = jpFallbacks;
            }

            EditorUtility.SetDirty(koreanFont);
            if (japaneseFont != null)
                EditorUtility.SetDirty(japaneseFont);
        }

        private static void CreateOrUpdateSettingsAsset(
            TMP_FontAsset koreanFont,
            TMP_FontAsset japaneseFont,
            TMP_FontAsset latinFont)
        {
            const string settingsPath = "Assets/GameResource/Data/Localization/LocalizationFontSettings.asset";
            EnsureLocalizationDataFolder();

            var settings = AssetDatabase.LoadAssetAtPath<LocalizationFontSettings>(settingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationFontSettings>();
                AssetDatabase.CreateAsset(settings, settingsPath);
            }

            var serialized = new SerializedObject(settings);
            serialized.FindProperty("_koreanPrimaryFont").objectReferenceValue = koreanFont;
            serialized.FindProperty("_japanesePrimaryFont").objectReferenceValue = japaneseFont;
            serialized.FindProperty("_latinPrimaryFont").objectReferenceValue = latinFont;

            var fallbackProperty = serialized.FindProperty("_fallbackFonts");
            fallbackProperty.arraySize = 0;
            if (japaneseFont != null)
            {
                fallbackProperty.arraySize++;
                fallbackProperty.GetArrayElementAtIndex(0).objectReferenceValue = japaneseFont;
            }

            if (latinFont != null)
            {
                fallbackProperty.arraySize++;
                fallbackProperty.GetArrayElementAtIndex(fallbackProperty.arraySize - 1).objectReferenceValue = latinFont;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static void EnsureLocalizationDataFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/GameResource/Data"))
                return;

            if (!AssetDatabase.IsValidFolder("Assets/GameResource/Data/Localization"))
                AssetDatabase.CreateFolder("Assets/GameResource/Data", "Localization");
        }
    }
}
#endif
