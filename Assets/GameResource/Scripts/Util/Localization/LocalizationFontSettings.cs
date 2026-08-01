using UnityEngine;
#if TMP_PRESENT
using TMPro;
#endif

namespace Backend.Util.Localization
{
    /// <summary>
    /// 현지화용 TMP 폰트·Fallback 체인 설정.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationFontSettings", menuName = "Abyss Chronicle/Localization Font Settings")]
    public sealed class LocalizationFontSettings : ScriptableObject
    {
        [Header("Primary")]
        [SerializeField] private UnityEngine.Object _koreanPrimaryFont;
        [SerializeField] private UnityEngine.Object _japanesePrimaryFont;
        [SerializeField] private UnityEngine.Object _latinPrimaryFont;

        [Header("Fallback Chain")]
        [SerializeField] private UnityEngine.Object[] _fallbackFonts = System.Array.Empty<UnityEngine.Object>();

#if TMP_PRESENT
        public TMP_FontAsset KoreanPrimaryFont => _koreanPrimaryFont as TMP_FontAsset;
        public TMP_FontAsset JapanesePrimaryFont => _japanesePrimaryFont as TMP_FontAsset;
        public TMP_FontAsset LatinPrimaryFont => _latinPrimaryFont as TMP_FontAsset;
        public TMP_FontAsset[] FallbackFonts
        {
            get
            {
                if (_fallbackFonts == null || _fallbackFonts.Length == 0)
                    return System.Array.Empty<TMP_FontAsset>();

                var result = new TMP_FontAsset[_fallbackFonts.Length];
                for (var i = 0; i < _fallbackFonts.Length; i++)
                    result[i] = _fallbackFonts[i] as TMP_FontAsset;

                return result;
            }
        }
#endif

        /// <summary>
        /// 현재 언어에 맞는 기본 TMP 폰트를 반환한다.
        /// </summary>
        public UnityEngine.Object ResolvePrimaryFont(GameLanguage language)
        {
            return language switch
            {
                GameLanguage.Korean => _koreanPrimaryFont != null ? _koreanPrimaryFont : _latinPrimaryFont,
                GameLanguage.Japanese => _japanesePrimaryFont != null ? _japanesePrimaryFont : _koreanPrimaryFont,
                _ => _latinPrimaryFont != null ? _latinPrimaryFont : _koreanPrimaryFont,
            };
        }
    }
}
