using Backend.AddressableKey;
using Backend.Object.Management;
using TMPro;
using UnityEngine;

namespace Backend.Util
{
    /// <summary>
    /// HUD용 TMP 폰트. BMJUA SDF(Addressable) 우선.
    /// </summary>
    public static class RuntimeUiTmpFont
    {
        private const string HudFontKey = "BMJUA_SDF";
        private const string FallbackAssetPath = "Assets/Fonts/BMJUA_ttf SDF.asset";
        private static TMP_FontAsset _cached;

        public static TMP_FontAsset Get()
        {
            if (_cached != null)
                return _cached;

            var address = AddressableKeys.InGame.Get(HudFontKey);
            if (!string.IsNullOrEmpty(address))
                _cached = ResourceManager.LoadResource<TMP_FontAsset>(address);

            if (_cached == null)
                _cached = ResourceManager.LoadResource<TMP_FontAsset>(FallbackAssetPath);

#if UNITY_EDITOR
            if (_cached == null)
            {
                _cached = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackAssetPath);
            }
#endif

            return _cached;
        }
    }
}
