using System;
using NUnit.Framework;

namespace Ashutosh.RemoteTuning.Tests
{
    public sealed class CacheTtlRulesTests
    {
        [Test]
        public void CacheEvaluation_TransitionsFreshToStaleToExpired()
        {
            var start = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var clock = new FakeClock(start);
            var store = new InMemoryKeyValueStore();
            var repo = new ConfigCacheRepository(store, clock, new FakeLogSink());

            repo.Save(
                rawJson: @"{ ""configVersion"": ""v1"", ""tuning"": { ""difficultyScalar"": 1.0, ""adCooldownSeconds"": 60, ""rewardMultiplier"": 1.0 }, ""features"": { ""iapVisible"": true }, ""experiments"": {} }",
                eTag: "etag-1",
                configVersion: "v1");

            var policy = CachePolicy.FromSeconds(60, 120);

            var evalFresh = repo.Evaluate(policy);
            Assert.AreEqual(CacheStatus.Fresh, evalFresh.Status);
            Assert.IsTrue(evalFresh.CanServe);
            Assert.IsFalse(evalFresh.ShouldRevalidate);

            clock.Advance(TimeSpan.FromSeconds(61));
            var evalStale = repo.Evaluate(policy);
            Assert.AreEqual(CacheStatus.Stale, evalStale.Status);
            Assert.IsTrue(evalStale.CanServe);
            Assert.IsTrue(evalStale.ShouldRevalidate);

            clock.Advance(TimeSpan.FromSeconds(120));
            var evalExpired = repo.Evaluate(policy);
            Assert.AreEqual(CacheStatus.Expired, evalExpired.Status);
            Assert.IsFalse(evalExpired.CanServe);
            Assert.IsTrue(evalExpired.ShouldRevalidate);
        }

        [Test]
        public void CacheEvaluation_Miss_WhenNoEntryExists()
        {
            var clock = new FakeClock(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
            var store = new InMemoryKeyValueStore();
            var repo = new ConfigCacheRepository(store, clock, new FakeLogSink());

            var policy = CachePolicy.FromSeconds(60, 120);
            var eval = repo.Evaluate(policy);

            Assert.AreEqual(CacheStatus.Miss, eval.Status);
            Assert.IsFalse(eval.CanServe);
            Assert.IsTrue(eval.ShouldRevalidate);
        }
    }
}