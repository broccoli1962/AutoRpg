using System;
using System.Collections.Generic;

namespace Backend.Chronicle
{
    /// <summary>
    /// 문장 뱅크의 개별 구(句) 항목.
    /// </summary>
    public sealed class PhraseEntry
    {
        /// <summary>
        /// 현지화 키 또는 원문 템플릿 ({character} 등 변수 포함 가능).
        /// </summary>
        public string LocalizationKey { get; }

        /// <summary>
        /// 가중 추첨 가중치. 1 미만이면 1로 취급한다.
        /// </summary>
        public int Weight { get; }

        /// <summary>
        /// 이 항목이 선택되려면 요청 컨텍스트에 모두 포함되어야 하는 조건 태그.
        /// 비어 있으면 무조건 후보가 된다.
        /// </summary>
        public IReadOnlyList<string> ConditionTags { get; }

        /// <summary>
        /// 문장 뱅크 항목을 생성한다.
        /// </summary>
        public PhraseEntry(string localizationKey, int weight, IReadOnlyList<string> conditionTags)
        {
            LocalizationKey = localizationKey ?? string.Empty;
            Weight = weight;
            ConditionTags = conditionTags ?? Array.Empty<string>();
        }
    }
}
