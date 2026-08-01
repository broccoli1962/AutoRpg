using System;
using System.Collections.Generic;

namespace Backend.Chronicle
{
    [Serializable]
    internal sealed class PhraseBankJsonDto
    {
        public string eventType;
        public Dictionary<string, PhraseEntryJsonDto[]> slots;
    }

    [Serializable]
    internal sealed class PhraseEntryJsonDto
    {
        public string key;
        public string text;
        public int weight = 1;
        public string[] conditionTags;
    }
}
