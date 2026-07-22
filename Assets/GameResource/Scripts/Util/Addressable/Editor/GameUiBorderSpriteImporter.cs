#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Backend.Editor
{
    /// <summary>
    /// GameUI/Borders 스프라이트 import 설정을 Sliced UI에 맞게 통일한다.
    /// </summary>
    public static class GameUiBorderSpriteImporter
    {
        private const string BordersFolder = "Assets/GameResource/Images/GameUI/Borders";
        private const float PixelsPerUnit = 10f;

        [MenuItem("Tools/UI/Apply GameUI Border Sprite Settings (PPU 10, Sliced)")]
        public static void ApplyBorderSpriteSettings()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { BordersFolder });
            var updated = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path == null || !path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                var changed = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }

                if (!Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit))
                {
                    importer.spritePixelsPerUnit = PixelsPerUnit;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    updated++;
                    Debug.Log($"[GameUiBorderSpriteImporter] Updated {Path.GetFileName(path)} (PPU={PixelsPerUnit}).");
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[GameUiBorderSpriteImporter] Done. Updated {updated} texture(s).");
        }
    }
}
#endif
