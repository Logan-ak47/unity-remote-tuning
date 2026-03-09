using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    internal readonly struct VariantSpec
    {
        public readonly string Key;
        public readonly int Weight;

        public VariantSpec(string key, int weight)
        {
            Key = key;
            Weight = weight;
        }
    }

    internal readonly struct ExperimentSpec
    {
        public readonly string Key;
        public readonly int RolloutPercent;     // 0..100
        public readonly string Salt;            // optional
        public readonly IReadOnlyList<VariantSpec> Variants;

        public ExperimentSpec(string key, int rolloutPercent, string salt, IReadOnlyList<VariantSpec> variants)
        {
            Key = key;
            RolloutPercent = rolloutPercent;
            Salt = salt;
            Variants = variants;
        }
    }

    internal readonly struct Assignment
    {
        public readonly bool InExperiment;
        public readonly string VariantKey;

        public Assignment(bool inExperiment, string variantKey)
        {
            InExperiment = inExperiment;
            VariantKey = variantKey;
        }

        public static Assignment Out() => new Assignment(false, null);
    }
}