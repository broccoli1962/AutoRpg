using UnityEngine;
using UnityEngine.UI;

namespace Backend.Object.UI
{
    /// <summary>
    /// 모바일 세로 UI 기준 CanvasScaler 설정 (1080x1920, Match 0.5).
    /// </summary>
    public static class MobileCanvasScaler
    {
        public static readonly Vector2 ReferenceResolution = new(1080f, 1920f);
        public const float MatchWidthOrHeight = 0.5f;

        /// <summary>
        /// CanvasScaler 를 프로젝트 표준 모바일 설정으로 맞춘다.
        /// </summary>
        public static void Apply(CanvasScaler scaler)
        {
            if (scaler == null)
                return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchWidthOrHeight;
        }

        /// <summary>
        /// GameObject 에 CanvasScaler 가 없으면 추가하고 표준 설정을 적용한다.
        /// </summary>
        public static CanvasScaler Ensure(GameObject go)
        {
            if (!go.TryGetComponent(out CanvasScaler scaler))
                scaler = go.AddComponent<CanvasScaler>();

            Apply(scaler);
            return scaler;
        }
    }
}
