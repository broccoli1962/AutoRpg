using System;
using System.Collections.Generic;

namespace Backend.GameSystems.DynamicEvent.Data
{
    [Serializable]
    public sealed class DynamicEventTrigger
    {
        public DynamicEventTriggerType Type = DynamicEventTriggerType.FloorEnter;
        public List<string> ZoneIds = new();
        public float Probability = 0.12f;
        public int MinFloor;
        public int MaxFloor;
    }

    [Serializable]
    public sealed class DynamicEventChoice
    {
        public string Id;
        public Dictionary<DynamicEventOutcomeEffect, float> EffectPool = new();
    }

    [Serializable]
    public sealed class DynamicEventTemplate
    {
        public string EventId;
        public DynamicEventCategory Category;
        public DynamicEventTrigger Trigger = new();
        public DynamicEventIntensity Intensity = DynamicEventIntensity.Standard;
        public List<DynamicEventChoice> Choices = new();
    }

    [Serializable]
    public sealed class DynamicEventChoiceText
    {
        public string Id;
        public string Text;
    }

    [Serializable]
    public sealed class DynamicEventLlmNarration
    {
        public string Narration;
        public List<DynamicEventChoiceText> Choices = new();
    }

    [Serializable]
    public sealed class DynamicEventInstance
    {
        public string InstanceId;
        public string TemplateId;
        public string ZoneId;
        public int Floor;
        public string LeaderName;
        public DynamicEventLlmNarration LlmNarration = new();
        public string PlayerChoiceId;
        public DynamicEventOutcomeEffect ResolvedOutcome;
        public string LlmResultNarration;
        public bool IsSceneReady;
        public bool IsResolved;
        public bool RequiresManualChoice;
    }
}
