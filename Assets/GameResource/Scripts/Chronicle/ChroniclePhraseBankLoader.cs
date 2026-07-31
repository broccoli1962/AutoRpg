using System;
using System.Collections.Generic;
using Backend.AddressableKey;
using Backend.Object.Management;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Backend.Chronicle
{
    /// <summary>
    /// Addressable JSON 문장 뱅크를 <see cref="PhraseBank"/>으로 적재한다.
    /// </summary>
    public static class ChroniclePhraseBankLoader
    {
        /// <summary>
        /// 등록된 이벤트 타입별 문장 뱅크 JSON을 비동기 로드한다.
        /// </summary>
        public static async UniTask<PhraseBank> LoadAsync()
        {
            var pools =
                new Dictionary<string, IReadOnlyDictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>>();

            foreach (var eventType in ChronicleEventTypes.All)
            {
                var jsonText = await LoadJsonTextAsync(eventType);
                if (string.IsNullOrWhiteSpace(jsonText))
                    continue;

                MergeDto(pools, ParseDto(jsonText));
            }

            return new PhraseBank(pools);
        }

        private static async UniTask<string> LoadJsonTextAsync(string eventType)
        {
            var address = AddressableKeys.Chronicle.Get(eventType);
            if (!string.IsNullOrEmpty(address))
            {
                var asset = await ResourceManager.LoadResourceAsync<TextAsset>(address);
                if (asset != null && !string.IsNullOrWhiteSpace(asset.text))
                    return asset.text;
            }

#if UNITY_EDITOR
            var editorPath = $"Assets/GameResource/Data/Chronicle/{eventType}.json";
            var editorAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(editorPath);
            if (editorAsset != null && !string.IsNullOrWhiteSpace(editorAsset.text))
                return editorAsset.text;
#endif

            return null;
        }

        private static PhraseBankJsonDto ParseDto(string jsonText)
        {
            return JsonConvert.DeserializeObject<PhraseBankJsonDto>(jsonText);
        }

        private static void MergeDto(
            Dictionary<string, IReadOnlyDictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>> pools,
            PhraseBankJsonDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.eventType) || dto.slots == null)
                return;

            var slotMap = new Dictionary<PhraseSlot, IReadOnlyList<PhraseEntry>>();
            foreach (var pair in dto.slots)
            {
                if (!Enum.TryParse(pair.Key, ignoreCase: false, out PhraseSlot slot))
                    continue;

                slotMap[slot] = ConvertEntries(pair.Value);
            }

            pools[dto.eventType] = slotMap;
        }

        private static IReadOnlyList<PhraseEntry> ConvertEntries(PhraseEntryJsonDto[] entries)
        {
            if (entries == null || entries.Length == 0)
                return Array.Empty<PhraseEntry>();

            var result = new List<PhraseEntry>(entries.Length);
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.text))
                    continue;

                result.Add(new PhraseEntry(
                    entry.text.Trim(),
                    entry.weight,
                    entry.conditionTags ?? Array.Empty<string>()));
            }

            return result;
        }
    }
}
