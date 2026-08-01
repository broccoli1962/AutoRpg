using Backend.GameSystems.Performance;
using NUnit.Framework;
using UnityEngine;

namespace Backend.GameSystems.Performance.Tests
{
    public class DeviceCapabilityDetectorTests
    {
        private PerformancePolicyTable _policy;

        [SetUp]
        public void SetUp()
        {
            _policy = ScriptableObject.CreateInstance<PerformancePolicyTable>();
            _policy.ApplySpecDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (_policy != null)
                UnityEngine.Object.DestroyImmediate(_policy);
        }

        [Test]
        public void DetectRecommendedPreset_2GbRam_ReturnsLow()
        {
            var preset = DeviceCapabilityDetector.DetectRecommendedPreset(2048, 8, _policy);
            Assert.AreEqual(QualityPreset.Low, preset);
        }

        [Test]
        public void DetectRecommendedPreset_4GbRam_ReturnsRecommended()
        {
            var preset = DeviceCapabilityDetector.DetectRecommendedPreset(4096, 8, _policy);
            Assert.AreEqual(QualityPreset.Recommended, preset);
        }

        [Test]
        public void DetectRecommendedPreset_LowCoreCount_ReturnsLow()
        {
            var preset = DeviceCapabilityDetector.DetectRecommendedPreset(6144, 2, _policy);
            Assert.AreEqual(QualityPreset.Low, preset);
        }

        [Test]
        public void ResolveEffectivePreset_Auto_UsesDeviceDetection()
        {
            var preset = DeviceCapabilityDetector.ResolveEffectivePreset(
                QualityPreset.Auto, 2048, 8, _policy);
            Assert.AreEqual(QualityPreset.Low, preset);
        }

        [Test]
        public void ResolveEffectivePreset_ManualLow_IgnoresDevice()
        {
            var preset = DeviceCapabilityDetector.ResolveEffectivePreset(
                QualityPreset.Low, 8192, 8, _policy);
            Assert.AreEqual(QualityPreset.Low, preset);
        }
    }

    public class PerformancePolicyTableTests
    {
        private PerformancePolicyTable _policy;

        [SetUp]
        public void SetUp()
        {
            _policy = ScriptableObject.CreateInstance<PerformancePolicyTable>();
            _policy.ApplySpecDefaults();
        }

        [TearDown]
        public void TearDown()
        {
            if (_policy != null)
                UnityEngine.Object.DestroyImmediate(_policy);
        }

        [Test]
        public void ApplySpecDefaults_MatchesSpec73Targets()
        {
            Assert.AreEqual(30, _policy.LowQualityTargetFps);
            Assert.AreEqual(60, _policy.RecommendedTargetFps);
            Assert.AreEqual(15, _policy.PowerSaveTargetFps);
            Assert.AreEqual(120f, _policy.IdleTimeoutSeconds, 0.001f);
            Assert.AreEqual(0.5f, _policy.LowQualityVfxDensity, 0.001f);
            Assert.AreEqual(2048, _policy.LowRamThresholdMb);
        }

        [Test]
        public void GetTargetFps_Low_Returns30()
        {
            Assert.AreEqual(30, _policy.GetTargetFps(QualityPreset.Low));
        }

        [Test]
        public void GetTargetFps_Recommended_Returns60()
        {
            Assert.AreEqual(60, _policy.GetTargetFps(QualityPreset.Recommended));
        }

        [Test]
        public void GetVfxDensity_Low_IsHalf()
        {
            Assert.AreEqual(0.5f, _policy.GetVfxDensity(QualityPreset.Low), 0.001f);
        }
    }

    public class PerformanceSettingsStoreTests
    {
        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey("abyss_quality_preset");
            PlayerPrefs.Save();
        }

        [Test]
        public void LoadPreset_DefaultsToAuto()
        {
            PlayerPrefs.DeleteKey("abyss_quality_preset");
            Assert.AreEqual(QualityPreset.Auto, PerformanceSettingsStore.LoadPreset());
        }

        [Test]
        public void SavePreset_PersistsValue()
        {
            PerformanceSettingsStore.SavePreset(QualityPreset.Low);
            Assert.AreEqual(QualityPreset.Low, PerformanceSettingsStore.LoadPreset());
        }
    }

    public class ZoneArtAddressableKeysTests
    {
        [Test]
        public void GetZoneLabel_ValidZone_ReturnsLabel()
        {
            Assert.AreEqual("ZoneArt_03", Backend.AddressableKey.AddressableKeys.ZoneArt.GetZoneLabel(3));
        }

        [Test]
        public void GetZoneLabel_OutOfRange_ReturnsNull()
        {
            Assert.IsNull(Backend.AddressableKey.AddressableKeys.ZoneArt.GetZoneLabel(0));
            Assert.IsNull(Backend.AddressableKey.AddressableKeys.ZoneArt.GetZoneLabel(9));
        }
    }
}
