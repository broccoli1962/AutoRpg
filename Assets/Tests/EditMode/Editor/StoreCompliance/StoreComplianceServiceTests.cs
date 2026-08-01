using NUnit.Framework;
using Backend.Meta.StoreCompliance;
using UnityEngine;

namespace Backend.Tests.EditMode.StoreCompliance
{
    public class StoreComplianceServiceTests
    {
        private StoreComplianceConfig _config;
        private StoreComplianceService _service;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<StoreComplianceConfig>();
            _config.ApplySpecDefaults();
            StoreComplianceConfigProvider.SetForTests(_config);
            _service = new StoreComplianceService(_config);
            AppBuildInfo.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            StoreComplianceConfigProvider.ResetCache();
            AppBuildInfo.ResetForTests();
        }

        [Test]
        public void StoreComplianceConfig_HasRequiredUrls()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_config.PrivacyPolicyUrl));
            Assert.IsFalse(string.IsNullOrEmpty(_config.TermsOfServiceUrl));
            Assert.IsFalse(string.IsNullOrEmpty(_config.AccountDeletionUrl));
            Assert.IsFalse(string.IsNullOrEmpty(_config.AccountDeletionEmail));
        }

        [Test]
        public void AppBuildInfo_DoesNotTargetChildren()
        {
            Assert.IsFalse(AppBuildInfo.TargetsChildren);
        }

        [Test]
        public void AppBuildInfo_DefaultEnvironmentIsDevelopment()
        {
            Assert.AreEqual(BuildEnvironmentKind.Development, AppBuildInfo.Environment);
        }

        [Test]
        public void SimulatedAdConsentService_ResolvesAsNotRequired()
        {
            var consent = new SimulatedAdConsentService();
            var status = consent.RequestConsentAsync().GetAwaiter().GetResult();

            Assert.IsTrue(consent.IsResolved);
            Assert.AreEqual(AdConsentStatus.NotRequired, status);
            Assert.IsTrue(consent.CanRequestPersonalizedAds);
        }

        [Test]
        public void StoreComplianceCopy_ProvidesFallbackStrings()
        {
            Assert.IsFalse(string.IsNullOrEmpty(StoreComplianceCopy.PanelTitle));
            Assert.IsFalse(string.IsNullOrEmpty(StoreComplianceCopy.TeenPaymentNotice));
            Assert.IsFalse(string.IsNullOrEmpty(StoreComplianceCopy.GachaRateInfo));
        }
    }
}
