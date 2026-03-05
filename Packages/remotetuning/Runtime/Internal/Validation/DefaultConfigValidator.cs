using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Ashutosh.RemoteTuning
{
    internal sealed class DefaultConfigValidator : IConfigValidator
    {
        public ValidationResult Validate(string rawJson, out ConfigDocument document)
        {
            document = null;
            var errors = new List<ValidationError>();

            if (!JsonParsing.TryParseObject(rawJson, out var root, out var parseError))
            {
                errors.Add(new ValidationError("$", $"Parse failed: {parseError}"));
                return ValidationResult.Fail(errors);
            }

            // Required: configVersion (string)
            var configVersion = RequireString(root, "configVersion", "$.configVersion", errors);

            // Required: tuning object
            var tuning = RequireObject(root, "tuning", "$.tuning", errors);

            // Required tuning keys + ranges
            if (tuning != null)
            {
                RequireNumberInRange(tuning, "difficultyScalar", "$.tuning.difficultyScalar", 0.1, 5.0, errors);
                RequireIntInRange(tuning, "adCooldownSeconds", "$.tuning.adCooldownSeconds", 0, 600, errors);
                RequireNumberInRange(tuning, "rewardMultiplier", "$.tuning.rewardMultiplier", 0.0, 10.0, errors);
            }

            // Required: features object
            var features = RequireObject(root, "features", "$.features", errors);

            if (features != null)
            {
                RequireBool(features, "iapVisible", "$.features.iapVisible", errors);
            }

            // Optional: experiments object (validate lightly, but structurally correct)
            var experimentsToken = root["experiments"];
            if (experimentsToken != null && experimentsToken.Type != JTokenType.Object)
            {
                errors.Add(new ValidationError("$.experiments", "Must be an object if present."));
            }
            else if (experimentsToken is JObject experimentsObj)
            {
                ValidateExperiments(experimentsObj, errors);
            }

            if (errors.Count > 0)
                return ValidationResult.Fail(errors);

            document = new ConfigDocument(rawJson, configVersion, root);
            return ValidationResult.Ok();
        }

        private static void ValidateExperiments(JObject experiments, List<ValidationError> errors)
        {
            // Expected shape:
            // "experiments": {
            //   "exp_key": { "rollout": 0..100, "salt": "x", "variants":[{"key":"A","weight":50},{"key":"B","weight":50}] }
            // }
            foreach (var prop in experiments.Properties())
            {
                var expPath = $"$.experiments.{prop.Name}";
                if (prop.Value.Type != JTokenType.Object)
                {
                    errors.Add(new ValidationError(expPath, "Experiment entry must be an object."));
                    continue;
                }

                var expObj = (JObject)prop.Value;

                // rollout optional but if present must be 0..100
                if (expObj.TryGetValue("rollout", out var rolloutTok))
                {
                    if (!TryGetInt(rolloutTok, out var rollout) || rollout < 0 || rollout > 100)
                        errors.Add(new ValidationError($"{expPath}.rollout", "Must be int 0..100."));
                }

                // variants optional for now (we’ll harden later), but if present must be array with valid weights
                if (expObj.TryGetValue("variants", out var variantsTok))
                {
                    if (variantsTok.Type != JTokenType.Array)
                    {
                        errors.Add(new ValidationError($"{expPath}.variants", "Must be an array."));
                        continue;
                    }

                    var arr = (JArray)variantsTok;
                    if (arr.Count == 0)
                    {
                        errors.Add(new ValidationError($"{expPath}.variants", "Must not be empty."));
                        continue;
                    }

                    var totalWeight = 0;
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var vPath = $"{expPath}.variants[{i}]";
                        if (arr[i].Type != JTokenType.Object)
                        {
                            errors.Add(new ValidationError(vPath, "Variant must be an object."));
                            continue;
                        }

                        var vObj = (JObject)arr[i];
                        var keyTok = vObj["key"];
                        if (keyTok == null || keyTok.Type != JTokenType.String || string.IsNullOrWhiteSpace(keyTok.Value<string>()))
                            errors.Add(new ValidationError($"{vPath}.key", "Required non-empty string."));

                        var weightTok = vObj["weight"];
                        if (weightTok == null || !TryGetInt(weightTok, out var w) || w <= 0)
                            errors.Add(new ValidationError($"{vPath}.weight", "Required int > 0."));
                        else
                            totalWeight += w;
                    }

                    if (totalWeight <= 0)
                        errors.Add(new ValidationError($"{expPath}.variants", "Total weight must be > 0."));
                }
            }
        }

        // ------- Helpers -------

        private static JObject RequireObject(JObject parent, string key, string path, List<ValidationError> errors)
        {
            var tok = parent[key];
            if (tok == null)
            {
                errors.Add(new ValidationError(path, "Missing required object."));
                return null;
            }

            if (tok.Type != JTokenType.Object)
            {
                errors.Add(new ValidationError(path, "Must be an object."));
                return null;
            }

            return (JObject)tok;
        }

        private static string RequireString(JObject parent, string key, string path, List<ValidationError> errors)
        {
            var tok = parent[key];
            if (tok == null)
            {
                errors.Add(new ValidationError(path, "Missing required string."));
                return null;
            }

            if (tok.Type != JTokenType.String)
            {
                errors.Add(new ValidationError(path, "Must be a string."));
                return null;
            }

            var val = tok.Value<string>();
            if (string.IsNullOrWhiteSpace(val))
            {
                errors.Add(new ValidationError(path, "Must not be empty."));
                return null;
            }

            return val;
        }

        private static void RequireBool(JObject parent, string key, string path, List<ValidationError> errors)
        {
            var tok = parent[key];
            if (tok == null)
            {
                errors.Add(new ValidationError(path, "Missing required bool."));
                return;
            }

            if (tok.Type != JTokenType.Boolean)
                errors.Add(new ValidationError(path, "Must be a boolean."));
        }

        private static void RequireIntInRange(JObject parent, string key, string path, int min, int max, List<ValidationError> errors)
        {
            var tok = parent[key];
            if (tok == null)
            {
                errors.Add(new ValidationError(path, $"Missing required int ({min}..{max})."));
                return;
            }

            if (!TryGetInt(tok, out var val))
            {
                errors.Add(new ValidationError(path, "Must be an integer."));
                return;
            }

            if (val < min || val > max)
                errors.Add(new ValidationError(path, $"Out of range ({min}..{max})."));
        }

        private static void RequireNumberInRange(JObject parent, string key, string path, double min, double max, List<ValidationError> errors)
        {
            var tok = parent[key];
            if (tok == null)
            {
                errors.Add(new ValidationError(path, $"Missing required number ({min}..{max})."));
                return;
            }

            if (!TryGetDouble(tok, out var val))
            {
                errors.Add(new ValidationError(path, "Must be a number."));
                return;
            }

            if (val < min || val > max)
                errors.Add(new ValidationError(path, $"Out of range ({min}..{max})."));
        }

        private static bool TryGetInt(JToken tok, out int value)
        {
            value = default;
            if (tok.Type == JTokenType.Integer)
            {
                value = tok.Value<int>();
                return true;
            }

            // sometimes numbers come as floats
            if (tok.Type == JTokenType.Float)
            {
                var d = tok.Value<double>();
                if (Math.Abs(d % 1) < 0.000001)
                {
                    value = (int)d;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetDouble(JToken tok, out double value)
        {
            value = default;
            if (tok.Type == JTokenType.Float || tok.Type == JTokenType.Integer)
            {
                value = tok.Value<double>();
                return true;
            }

            return false;
        }
    }
}