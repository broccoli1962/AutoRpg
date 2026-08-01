using Backend.Util.Localization;
using NUnit.Framework;

namespace Backend.Util.Localization.Tests
{
    public class LocalizationServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            LocalizationService.ResetWarningCacheForTests();
            LocalizationService.Initialize(GameLanguage.Korean);
        }

        [Test]
        public void Get_ReturnsLocalizedValue_ForKnownKey()
        {
            var text = LocalizationService.Get("ui.common.confirm");

            Assert.That(text, Is.EqualTo("확인"));
        }

        [Test]
        public void Get_ReturnsKeyName_WhenMissing()
        {
            const string missingKey = "missing.key.test";

            var text = LocalizationService.Get(missingKey);

            Assert.That(text, Is.EqualTo(missingKey));
            Assert.That(string.IsNullOrEmpty(text), Is.False);
        }

        [Test]
        public void ChangeLanguage_SwitchesLookupTable()
        {
            LocalizationService.ChangeLanguage(GameLanguage.English);

            var text = LocalizationService.Get("ui.common.confirm");

            Assert.That(text, Is.EqualTo("Confirm"));
        }

        [Test]
        public void ChangeLanguage_SupportsJapanese()
        {
            LocalizationService.ChangeLanguage(GameLanguage.Japanese);

            var text = LocalizationService.Get("ui.common.confirm");

            Assert.That(text, Is.EqualTo("確認"));
        }
    }

    public class LargeNumberFormatterTests
    {
        [TestCase(999, "999")]
        [TestCase(1000, "1K")]
        [TestCase(1500, "1.5K")]
        [TestCase(1_000_000, "1M")]
        [TestCase(1_000_000_000, "1B")]
        [TestCase(1_000_000_000_000, "1T")]
        [TestCase(1_000_000_000_000_000, "1Qa")]
        public void Format_UsesExpectedSuffix(long value, string expected)
        {
            Assert.That(LargeNumberFormatter.Format(value), Is.EqualTo(expected));
        }

        [Test]
        public void Format_HandlesNegativeValues()
        {
            Assert.That(LargeNumberFormatter.Format(-2500), Is.EqualTo("-2.5K"));
        }
    }
}
