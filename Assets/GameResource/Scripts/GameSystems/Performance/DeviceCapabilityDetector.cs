namespace Backend.GameSystems.Performance
{
    /// <summary>
    /// 단말 RAM·코어 수 기반 품질 프리셋 자동 판정. Unity API 없이 테스트 가능하다.
    /// </summary>
    public static class DeviceCapabilityDetector
    {
        /// <summary>
        /// 시스템 사양으로 권장 품질 프리셋을 판정한다.
        /// </summary>
        public static QualityPreset DetectRecommendedPreset(
            int systemMemoryMb,
            int processorCount,
            PerformancePolicyTable policy)
        {
            if (policy == null)
                return QualityPreset.Recommended;

            if (systemMemoryMb > 0 && systemMemoryMb <= policy.LowRamThresholdMb)
                return QualityPreset.Low;

            if (processorCount > 0 && processorCount < policy.LowCoreThreshold)
                return QualityPreset.Low;

            return QualityPreset.Recommended;
        }

        /// <summary>
        /// 사용자 설정(Auto 포함)을 해석해 실제 적용 프리셋을 반환한다.
        /// </summary>
        public static QualityPreset ResolveEffectivePreset(
            QualityPreset userPreset,
            int systemMemoryMb,
            int processorCount,
            PerformancePolicyTable policy)
        {
            if (userPreset != QualityPreset.Auto)
                return userPreset;

            return DetectRecommendedPreset(systemMemoryMb, processorCount, policy);
        }
    }
}
