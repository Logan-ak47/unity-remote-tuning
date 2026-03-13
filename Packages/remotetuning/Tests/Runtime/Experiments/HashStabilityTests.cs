using NUnit.Framework;

namespace Ashutosh.RemoteTuning.Tests
{
    public sealed class HashStabilityTests
    {
        [Test]
        public void Hash32_SameInput_ReturnsSameValue()
        {
            var hasher = new Fnv1aHasher();

            var a = hasher.Hash32("abc");
            var b = hasher.Hash32("abc");

            Assert.AreEqual(a, b);
        }

        [Test]
        public void Hash32_KnownValue_MatchesExpected()
        {
            var hasher = new Fnv1aHasher();

            var hash = hasher.Hash32("abc");

            Assert.AreEqual(440920331u, hash);
        }

        [Test]
        public void Hash32_DifferentInputs_UsuallyDiffer()
        {
            var hasher = new Fnv1aHasher();

            var a = hasher.Hash32("abc");
            var b = hasher.Hash32("abcd");

            Assert.AreNotEqual(a, b);
        }
    }
}