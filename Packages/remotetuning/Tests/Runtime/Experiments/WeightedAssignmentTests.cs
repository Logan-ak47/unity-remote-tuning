using System.Collections.Generic;
using NUnit.Framework;

namespace Ashutosh.RemoteTuning.Tests
{
    public sealed class WeightedAssignmentTests
    {
        [Test]
        public void WeightedAssignment_ApproximatelyMatches90_10Split()
        {
            var hasher = new Fnv1aHasher();
            var store = new AssignmentStore(new InMemoryKeyValueStore());
            var assigner = new ExperimentAssigner(hasher, store, new FakeLogSink());

            var spec = new ExperimentSpec(
                key: "reward_exp",
                rolloutPercent: 100,
                salt: "s1",
                variants: new List<VariantSpec>
                {
                    new VariantSpec("A", 90),
                    new VariantSpec("B", 10)
                });

            int countA = 0;
            int countB = 0;
            const int sampleSize = 10000;

            for (int i = 0; i < sampleSize; i++)
            {
                var userId = $"user_{i}";
                var assignment = assigner.GetOrAssign(userId, spec);

                Assert.IsTrue(assignment.InExperiment);
                Assert.IsTrue(assignment.VariantKey == "A" || assignment.VariantKey == "B");

                if (assignment.VariantKey == "A") countA++;
                if (assignment.VariantKey == "B") countB++;
            }

            var ratioB = (float)countB / sampleSize;

            Assert.That(ratioB, Is.InRange(0.08f, 0.12f));
            Assert.AreEqual(sampleSize, countA + countB);
        }

        [Test]
        public void RolloutZero_PutsEveryoneOut()
        {
            var hasher = new Fnv1aHasher();
            var store = new AssignmentStore(new InMemoryKeyValueStore());
            var assigner = new ExperimentAssigner(hasher, store, new FakeLogSink());

            var spec = new ExperimentSpec(
                key: "reward_exp",
                rolloutPercent: 0,
                salt: "s1",
                variants: new List<VariantSpec>
                {
                    new VariantSpec("A", 50),
                    new VariantSpec("B", 50)
                });

            var assignment = assigner.GetOrAssign("user_1", spec);

            Assert.IsFalse(assignment.InExperiment);
            Assert.IsNull(assignment.VariantKey);
        }
    }
}