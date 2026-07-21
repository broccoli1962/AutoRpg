using System;
using System.Collections.Generic;
using System.Linq;
using Backend.GameSystems.DynamicEvent.Data;
using Newtonsoft.Json;
using UnityEngine;

namespace Backend.GameSystems.DynamicEvent.LLM
{
    /// <summary>
    /// 동적 이벤트 LLM 출력(JSON)을 파싱하고 뼈대 선택지 id와 대조해 검증한다.
    /// </summary>
    public static class DynamicEventLlmParser
    {
        [Serializable]
        private sealed class DynamicEventSceneJsonDto
        {
            public string narration;
            public List<DynamicEventChoiceJsonDto> choices;
        }

        [Serializable]
        private sealed class DynamicEventChoiceJsonDto
        {
            public string id;
            public string text;
        }

        public static bool TryParseSceneJson(
            string raw,
            DynamicEventTemplate template,
            out DynamicEventLlmNarration narration)
        {
            narration = null;
            if (string.IsNullOrWhiteSpace(raw) || template == null)
                return false;

            var json = ExtractJsonObject(raw);
            if (string.IsNullOrEmpty(json))
                return false;

            DynamicEventSceneJsonDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DynamicEventSceneJsonDto>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DynamicEventLlmParser] JSON parse failed: {e.Message}");
                return false;
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.narration) || dto.choices == null)
                return false;

            var expectedIds = template.Choices.Select(c => c.Id).ToHashSet();
            var parsedChoices = new List<DynamicEventChoiceText>();
            foreach (var choice in dto.choices)
            {
                if (choice == null || string.IsNullOrWhiteSpace(choice.id) || string.IsNullOrWhiteSpace(choice.text))
                    continue;

                if (!expectedIds.Contains(choice.id))
                    continue;

                parsedChoices.Add(new DynamicEventChoiceText { Id = choice.id, Text = choice.text.Trim() });
            }

            if (parsedChoices.Count != expectedIds.Count)
                return false;

            narration = new DynamicEventLlmNarration
            {
                Narration = dto.narration.Trim(),
                Choices = parsedChoices
            };
            return true;
        }

        private static string ExtractJsonObject(string raw)
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start < 0 || end <= start)
                return null;

            return raw.Substring(start, end - start + 1);
        }
    }
}
