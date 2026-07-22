using UnityEngine;

namespace Backend.Object.UI
{
    /// <summary>
    /// Screen.safeArea 를 RectTransform 앵커에 반영한다. Canvas 루트 또는 풀스크린 UI 에 부착.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea ||
                _lastScreenSize.x != Screen.width ||
                _lastScreenSize.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            var screen = new Vector2(Screen.width, Screen.height);
            if (screen.x <= 0f || screen.y <= 0f)
                return;

            var anchorMin = _lastSafeArea.position;
            var anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x /= screen.x;
            anchorMin.y /= screen.y;
            anchorMax.x /= screen.x;
            anchorMax.y /= screen.y;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
