using NUnit.Framework;

namespace Ashutosh.RemoteTuning.Tests
{
    public sealed class ValidationTests
    {
        [Test]
        public void DefaultConfig_IsValid()
        {
            var validator = new DefaultConfigValidator();

            var result = validator.Validate(ConfigDefaults.RawJson, out var document);

            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(document);
            Assert.AreEqual("default-1", document.ConfigVersion);
        }

        [Test]
        public void Validator_RejectsMissingRequiredKeys()
        {
            var validator = new DefaultConfigValidator();

            var badJson = @"{
              ""configVersion"": ""v1"",
              ""features"": { ""iapVisible"": true },
              ""experiments"": {}
            }";

            var result = validator.Validate(badJson, out var document);

            Assert.IsFalse(result.IsValid);
            Assert.IsNull(document);
            Assert.IsNotEmpty(result.Errors);
        }

        [Test]
        public void Validator_RejectsOutOfRangeValues()
        {
            var validator = new DefaultConfigValidator();

            var badJson = @"{
              ""configVersion"": ""v2"",
              ""tuning"": {
                ""difficultyScalar"": 999,
                ""adCooldownSeconds"": -5,
                ""rewardMultiplier"": 1.0
              },
              ""features"": {
                ""iapVisible"": true
              },
              ""experiments"": {}
            }";

            var result = validator.Validate(badJson, out var document);

            Assert.IsFalse(result.IsValid);
            Assert.IsNull(document);
            Assert.IsTrue(result.Errors.Count >= 1);
        }
    }
}