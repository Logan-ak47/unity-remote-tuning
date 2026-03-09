namespace Ashutosh.RemoteTuning
{
  internal static class ConfigDefaults
  {
    // Must match validator schema. Keep this minimal, predictable, and safe.
    public const string RawJson = @"{
  ""configVersion"": ""default-1"",
  ""tuning"": {
    ""difficultyScalar"": 1.0,
    ""adCooldownSeconds"": 60,
    ""rewardMultiplier"": 1.0
  },
  ""features"": {
    ""iapVisible"": true
  },
  ""experiments"": {
   ""reward_exp"": {
    ""rollout"": 100,
    ""salt"": ""s1"",
    ""variants"": [
      { ""key"": ""A"", ""weight"": 50 },
      { ""key"": ""B"", ""weight"": 50 }
    ]
  }
   }
}";
  }
}