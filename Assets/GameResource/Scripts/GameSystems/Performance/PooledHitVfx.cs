using UnityEngine;
using UnityEngine.UI;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 풀링된 히트 VFX 스프라이트.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class PooledHitVfx : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private float _remainingLifetime;

        public Image Image => _image != null ? _image : (_image = GetComponent<Image>());
        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : (_rectTransform = GetComponent<RectTransform>());
        public float RemainingLifetime => _remainingLifetime;

        /// <summary>
        /// VFX 수명을 설정한다.
        /// </summary>
        public void Begin(float lifetimeSeconds)
        {
            _remainingLifetime = lifetimeSeconds;
        }

        private void Update()
        {
            if (_remainingLifetime <= 0f)
                return;

            _remainingLifetime -= Time.deltaTime;
        }

        /// <summary>
        /// 수명이 만료되었는지 확인한다.
        /// </summary>
        public bool IsExpired => _remainingLifetime <= 0f;

        /// <summary>
        /// 풀 반환 전 상태를 초기화한다.
        /// </summary>
        public void ResetVisual()
        {
            _remainingLifetime = 0f;
            Image.sprite = null;
            Image.color = Color.white;
            RectTransform.localScale = Vector3.one;
            RectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
