using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Ashutosh.RemoteTuning
{
    internal static class ExperimentSpecParser
    {
        public static bool TryParse(ConfigAccessor accessor, string experimentKey, out ExperimentSpec spec)
        {
            spec = default;

            if (accessor == null || string.IsNullOrWhiteSpace(experimentKey))
                return false;

            if (!accessor.TryGetToken($"experiments.{experimentKey}", out var tok))
                return false;

            if (tok is not JObject obj)
                return false;

            // rollout optional (default 100)
            var rollout = 100;
            if (obj.TryGetValue("rollout", out var rolloutTok))
            {
                if (!TryGetInt(rolloutTok, out rollout))
                    rollout = 100;
            }

            // salt optional
            string salt = null;
            if (obj.TryGetValue("salt", out var saltTok) && saltTok.Type == JTokenType.String)
                salt = saltTok.Value<string>();

            // variants required
            if (!obj.TryGetValue("variants", out var variantsTok) || variantsTok.Type != JTokenType.Array)
                return false;

            var arr = (JArray)variantsTok;
            if (arr.Count == 0)
                return false;

            var variants = new List<VariantSpec>(arr.Count);
            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i] is not JObject vObj) continue;

                var keyTok = vObj["key"];
                var weightTok = vObj["weight"];

                if (keyTok?.Type != JTokenType.String) continue;
                var vKey = keyTok.Value<string>();
                if (string.IsNullOrWhiteSpace(vKey)) continue;

                if (!TryGetInt(weightTok, out var w) || w <= 0) continue;

                variants.Add(new VariantSpec(vKey, w));
            }

            if (variants.Count == 0)
                return false;

            spec = new ExperimentSpec(experimentKey, rollout, salt, variants);
            return true;
        }

        private static bool TryGetInt(JToken tok, out int value)
        {
            value = default;
            if (tok == null) return false;

            if (tok.Type == JTokenType.Integer)
            {
                value = tok.Value<int>();
                return true;
            }

            if (tok.Type == JTokenType.Float)
            {
                var d = tok.Value<double>();
                if (System.Math.Abs(d % 1) < 0.000001)
                {
                    value = (int)d;
                    return true;
                }
            }

            return false;
        }
    }
}