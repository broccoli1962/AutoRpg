using UnityEngine;

namespace Backend.Object.UI
{
    public enum TouchTargetKind
    {
        Button = 0,
        Icon = 1,
    }

    /// <summary>
    /// 터치 타깃 최소 크기를 보장한다. 버튼 88px+, 아이콘 96x96+.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class TouchTargetSize : MonoBehaviour
    {
        public const float MinButtonHeight = 88f;
        public const float MinIconSize = 96f;

        [SerializeField] private TouchTargetKind _kind = TouchTargetKind.Button;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_rect == null)
                TryGetComponent(out _rect);

            Apply();
        }
#endif

        /// <summary>
        /// 직렬화된 종류에 맞춰 RectTransform 최소 크기를 적용한다.
        /// </summary>
        public void Apply()
        {
            if (_rect == null)
                return;

            var size = _rect.sizeDelta;
            if (_kind == TouchTargetKind.Icon)
            {
                if (size.x < MinIconSize)
                    size.x = MinIconSize;
                if (size.y < MinIconSize)
                    size.y = MinIconSize;
            }
            else
            {
                if (size.y < MinButtonHeight)
                    size.y = MinButtonHeight;
            }

            _rect.sizeDelta = size;
        }
    }
}
