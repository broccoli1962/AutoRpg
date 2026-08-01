using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Backend.Util.Localization
{
    /// <summary>
    /// GSSL Localize JSON 기반 키 조회·언어 전환 API.
    /// </summary>
    public static class LocalizationService
    {
        private static readonly HashSet<string> WarnedMissingKeys = new(StringComparer.Ordinal);

        /// <summary>
        /// 현재 활성 언어.
        /// </summary>
        public static GameLanguage CurrentLanguage { get; private set; } = GameLanguage.Korean;

        /// <summary>
        /// 현재 활성 CultureInfo.
        /// </summary>
        public static CultureInfo CurrentCulture { get; private set; } = CreateCulture(GameLanguage.Korean);

        /// <summary>
        /// 부트스트랩 시 호출해 시스템 언어 또는 기본값으로 초기화한다.
        /// </summary>
        public static void Initialize(GameLanguage? overrideLanguage = null)
        {
            var language = overrideLanguage ?? ResolveSystemLanguage();
            ChangeLanguage(language);
        }

        /// <summary>
        /// 표시 언어를 변경하고 UI 갱신 이벤트를 발행한다.
        /// </summary>
        public static void ChangeLanguage(GameLanguage language)
        {
            CurrentLanguage = language;
            CurrentCulture = CreateCulture(language);
            LocalizeTable.ChangeLanguage(ToSystemLanguage(language));
        }

        /// <summary>
        /// 현지화 키로 문자열을 조회한다. 누락 시 키 이름과 경고를 반환한다.
        /// </summary>
        public static string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            var resolved = LocalizeTable.GetLocalizeText(key, args);
            if (!IsMissingKey(key, resolved))
                return resolved;

            WarnMissingKey(key);
            return key;
        }

        /// <summary>
        /// Chronicle 등 내부 키 해석용. 누락 시 키 문자열을 그대로 반환한다.
        /// </summary>
        public static string ResolveKey(string key)
        {
            return Get(key);
        }

        /// <summary>
        /// 테스트·도메인 리로드용 경고 캐시를 비운다.
        /// </summary>
        public static void ResetWarningCacheForTests()
        {
            WarnedMissingKeys.Clear();
        }

        private static bool IsMissingKey(string key, string resolved)
        {
            if (string.IsNullOrEmpty(resolved))
                return true;

            if (resolved == key)
                return true;

            return resolved.Length > 1 && resolved[0] == '!' && resolved.Substring(1) == key;
        }

        private static void WarnMissingKey(string key)
        {
            WarnedMissingKeys.Add(key);
        }

        private static GameLanguage ResolveSystemLanguage()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.Korean => GameLanguage.Korean,
                SystemLanguage.Japanese => GameLanguage.Japanese,
                _ => GameLanguage.English,
            };
        }

        private static SystemLanguage ToSystemLanguage(GameLanguage language)
        {
            return language switch
            {
                GameLanguage.Korean => SystemLanguage.Korean,
                GameLanguage.Japanese => SystemLanguage.Japanese,
                _ => SystemLanguage.English,
            };
        }

        private static CultureInfo CreateCulture(GameLanguage language)
        {
            var name = language switch
            {
                GameLanguage.Korean => "ko-KR",
                GameLanguage.Japanese => "ja-JP",
                _ => "en-US",
            };

            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                return CultureInfo.InvariantCulture;
            }
        }
    }
}
