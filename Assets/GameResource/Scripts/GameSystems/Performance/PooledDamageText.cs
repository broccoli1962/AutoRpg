using TMPro;
using UnityEngine;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 풀링된 데미지 플로팅 텍스트.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class PooledDamageText : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        private RectTransform _rectTransform;

        public TextMeshProUGUI Label => _label != null ? _label : (_label = GetComponent<TextMeshProUGUI>());
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        /// <summary>
        /// 풀 반환 전 상태를 초기화한다.
        /// </summary>
        public void ResetVisual()
        {
            Label.text = string.Empty;
            Label.color = Color.white;
            RectTransform.localScale = Vector3.one;
            RectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
