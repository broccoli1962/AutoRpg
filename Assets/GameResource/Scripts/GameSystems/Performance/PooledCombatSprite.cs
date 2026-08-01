using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 풀링된 전투 스프라이트(몬스터·드롭 아이콘) 공통 베이스.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class PooledCombatSprite : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;

        public Image Image => _image != null ? _image : (_image = GetComponent<Image>());
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());

        /// <summary>
        /// 풀 반환 전 상태를 초기화한다.
        /// </summary>
        public void ResetVisual()
        {
            Image.sprite = null;
            Image.color = Color.white;
            RectTransform.localScale = Vector3.one;
            RectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
