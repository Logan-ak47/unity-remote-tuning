using System.Collections.Generic;
using NUnit.Framework;

namespace Ashutosh.RemoteTuning.Tests
{
    public sealed class PersistenceTests
    {
        [Test]
        public void AssignmentStore_SaveAndLoad_Works()
        {
            var store = new AssignmentStore(new InMemoryKeyValueStore());

            store.Set("user_1", "reward_exp", "B");

            var found = store.TryGet("user_1", "reward_exp", out var variant);

            Assert.IsTrue(found);
            Assert.AreEqual("B", variant);
        }

        [Test]
        public void PersistedAssignment_DoesNotFlip_WhenWeightsChange()
        {
            var backingStore = new InMemoryKeyValueStore();
            var store = new AssignmentStore(backingStore);
            var assigner = new ExperimentAssigner(new Fnv1aHasher(), store, new FakeLogSink());

            var firstSpec = new ExperimentSpec(
                key: "reward_exp",
                rolloutPercent: 100,
                salt: "s1",
                variants: new List<VariantSpec>
                {
                    new VariantSpec("A", 50),
                    new VariantSpec("B", 50)
                });

            var initial = assigner.GetOrAssign("user_42", firstSpec);
            Assert.IsTrue(initial.InExperiment);

            var changedSpec = new ExperimentSpec(
                key: "reward_exp",
                rolloutPercent: 100,
                salt: "s1",
                variants: new List<VariantSpec>
                {
                    new VariantSpec("A", 100)
                });

            var afterChange = assigner.GetOrAssign("user_42", changedSpec);

            Assert.IsTrue(afterChange.InExperiment);
            Assert.AreEqual(initial.VariantKey, afterChange.VariantKey);
        }
    }
}