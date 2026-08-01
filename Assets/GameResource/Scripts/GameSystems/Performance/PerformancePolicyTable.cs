using UnityEngine;

namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// spec 7.3 성능 목표 수치. Remote Config 로 덮어쓸 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "PerformancePolicyTable", menuName = "Abyss Chronicle/Performance Policy Table")]
    public sealed class PerformancePolicyTable : ScriptableObject
    {
        [SerializeField] private int _lowQualityTargetFps = 30;
        [SerializeField] private int _recommendedTargetFps = 60;
        [SerializeField] private int _powerSaveTargetFps = 15;
        [SerializeField] private float _idleTimeoutSeconds = 120f;
        [SerializeField] private float _lowQualityVfxDensity = 0.5f;
        [SerializeField] private float _recommendedVfxDensity = 1f;
        [SerializeField] private int _lowRamThresholdMb = 2048;
        [SerializeField] private int _lowCoreThreshold = 4;

        public int LowQualityTargetFps => _lowQualityTargetFps;
        public int RecommendedTargetFps => _recommendedTargetFps;
        public int PowerSaveTargetFps => _powerSaveTargetFps;
        public float IdleTimeoutSeconds => _idleTimeoutSeconds;
        public float LowQualityVfxDensity => _lowQualityVfxDensity;
        public float RecommendedVfxDensity => _recommendedVfxDensity;
        public int LowRamThresholdMb => _lowRamThresholdMb;
        public int LowCoreThreshold => _lowCoreThreshold;

        /// <summary>
        /// spec 7.3 기본값을 적용한다.
        /// </summary>
        public void ApplySpecDefaults()
        {
            _lowQualityTargetFps = 30;
            _recommendedTargetFps = 60;
            _powerSaveTargetFps = 15;
            _idleTimeoutSeconds = 120f;
            _lowQualityVfxDensity = 0.5f;
            _recommendedVfxDensity = 1f;
            _lowRamThresholdMb = 2048;
            _lowCoreThreshold = 4;
        }

        /// <summary>
        /// 프리셋에 해당하는 목표 FPS 를 반환한다.
        /// </summary>
        public int GetTargetFps(QualityPreset preset)
        {
            return preset == QualityPreset.Low ? _lowQualityTargetFps : _recommendedTargetFps;
        }

        /// <summary>
        /// 프리셋에 해당하는 VFX 밀도(0~1)를 반환한다.
        /// </summary>
        public float GetVfxDensity(QualityPreset preset)
        {
            return preset == QualityPreset.Low ? _lowQualityVfxDensity : _recommendedVfxDensity;
        }
    }
}
