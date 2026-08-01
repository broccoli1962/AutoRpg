using Backend.Meta.Gacha;
using Backend.Meta.Characters;
using NUnit.Framework;
using UnityEngine;

namespace Backend.Tests.EditMode.Gacha
{
    public class GachaRateDisclosureBuilderTests
    {
        private GachaRateTable _rateTable;
        private GachaBannerPool _bannerPool;

        [SetUp]
        public void SetUp()
        {
            _rateTable = ScriptableObject.CreateInstance<GachaRateTable>();
            _rateTable.ApplySpecDefaults();

            _bannerPool = ScriptableObject.CreateInstance<GachaBannerPool>();
        }

        [Test]
        public void Build_UsesRateTableGradeProbabilities()
        {
            var snapshot = GachaRateDisclosureBuilder.Build(_rateTable, _bannerPool, new GachaPityState());

            Assert.AreEqual(4, snapshot.GradeRates.Count);
            Assert.AreEqual(7_000, snapshot.GradeRates[0].RateBasisPoints);
            Assert.AreEqual(2_400, snapshot.GradeRates[1].RateBasisPoints);
            Assert.AreEqual(550, snapshot.GradeRates[2].RateBasisPoints);
            Assert.AreEqual(50, snapshot.GradeRates[3].RateBasisPoints);
        }

        [Test]
        public void Build_SplitsGradeRateEvenlyAcrossPoolMembers()
        {
            var snapshot = GachaRateDisclosureBuilder.Build(_rateTable, _bannerPool, new GachaPityState());

            var rItems = 0;
            foreach (var entry in snapshot.ItemRates)
            {
                if (entry.GradeLocalizeKey == "gacha.rate.grade.r")
                {
                    Assert.AreEqual(3_500, entry.RateBasisPoints);
                    rItems++;
                }
            }

            Assert.AreEqual(2, rItems);
        }

        [Test]
        public void Build_IncludesPityCountersAndThresholdsFromTable()
        {
            var pity = new GachaPityState { SsrCounter = 42, UrCounter = 17 };

            var snapshot = GachaRateDisclosureBuilder.Build(_rateTable, _bannerPool, pity);

            Assert.AreEqual(42, snapshot.SsrPityCounter);
            Assert.AreEqual(17, snapshot.UrPityCounter);
            Assert.AreEqual(_rateTable.SsrPityThreshold, snapshot.SsrPityThreshold);
            Assert.AreEqual(_rateTable.UrPityThreshold, snapshot.UrPityThreshold);
            Assert.AreEqual(10, snapshot.TenPullCount);
        }

        [Test]
        public void FormatPercent_ConvertsBasisPoints()
        {
            Assert.AreEqual("70", GachaRateDisclosureBuilder.FormatPercent(7_000));
            Assert.AreEqual("5.5", GachaRateDisclosureBuilder.FormatPercent(550));
            Assert.AreEqual("0.5", GachaRateDisclosureBuilder.FormatPercent(50));
        }
    }
}
