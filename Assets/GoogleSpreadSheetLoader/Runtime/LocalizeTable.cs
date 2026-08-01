using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
// using Util;

[System.Serializable]
public class LocalizeKeyValue
{
    public string Key;
    public string Value;
}

public static class LocalizeTable
{
    private static Dictionary<string, string> dicLocalize = new();
    public static UnityAction OnChangedLanguage;

    public static void Initialize(SystemLanguage language)
    {
        var suffix = language switch
        {
            SystemLanguage.Korean => "ko",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.English => "en",
            _ => "en",
        };

        var assetName = $"Localize_{suffix}";
        var obj = Resources.Load<TextAsset>(assetName);

        dicLocalize.Clear();
        if (obj == null || string.IsNullOrEmpty(obj.text))
            return;

        var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.text);
        if (dictionary == null)
            return;

        foreach (var pair in dictionary)
        {
            if (!string.IsNullOrEmpty(pair.Key))
                dicLocalize[pair.Key] = pair.Value ?? string.Empty;
        }
    }

    public static void ChangeLanguage(SystemLanguage language)
    {
        Initialize(language);

        // LanguageUtil.SetLanguageCode(language);

        OnChangedLanguage?.Invoke();
    }

    public static string GetLocalizeText(this string key, params object[] param)
    {
        if (dicLocalize.Count == 0)
            Initialize(Application.systemLanguage);

        if (dicLocalize.TryGetValue(key, out var result) && !string.IsNullOrEmpty(result))
        {
            return param != null && param.Length > 0
                ? string.Format(result, param)
                : result;
        }

        return key;
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void InitializeOnLoadMethod()
    {
        OnChangedLanguage = null;
    }
#endif
}