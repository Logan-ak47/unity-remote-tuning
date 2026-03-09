using System;
using Newtonsoft.Json.Linq;

namespace Ashutosh.RemoteTuning
{
    internal sealed class ConfigAccessor
    {
        private string _lastRawJson;
        private JObject _root;
        private bool _isValid;

        private readonly ILogSink _log;

        public bool IsValid => _isValid;

        public ConfigAccessor(ILogSink log)
        {
            _log = log ?? new NullLogSink();
        }

        public void Bind(string rawJson)
        {
            if (ReferenceEquals(rawJson, _lastRawJson))
                return;

            _lastRawJson = rawJson;
            _isValid = JsonParsing.TryParseObject(rawJson, out _root, out var err);

            if (!_isValid)
            {
                _root = null;
                _log.Warn($"[RemoteTuning] ConfigAccessor parse failed: {err}");
            }
        }

        public bool TryGetToken(string dottedPath, out JToken token)
        {
            token = null;
            if (!_isValid || _root == null)
                return false;

            var parts = ConfigPath.Split(dottedPath);
            if (parts.Length == 0)
                return false;

            JToken cur = _root;
            for (int i = 0; i < parts.Length; i++)
            {
                if (cur is JObject obj && obj.TryGetValue(parts[i], out var next))
                {
                    cur = next;
                    continue;
                }

                return false;
            }

            token = cur;
            return true;
        }

        public bool TryGetBool(string path, out bool value)
        {
            value = default;
            if (!TryGetToken(path, out var t)) return false;
            if (t.Type == JTokenType.Boolean) { value = t.Value<bool>(); return true; }
            return false;
        }

        public bool TryGetInt(string path, out int value)
        {
            value = default;
            if (!TryGetToken(path, out var t)) return false;

            if (t.Type == JTokenType.Integer) { value = t.Value<int>(); return true; }
            if (t.Type == JTokenType.Float)
            {
                var d = t.Value<double>();
                if (Math.Abs(d % 1) < 0.000001) { value = (int)d; return true; }
            }
            return false;
        }

        public bool TryGetFloat(string path, out float value)
        {
            value = default;
            if (!TryGetToken(path, out var t)) return false;

            if (t.Type == JTokenType.Float || t.Type == JTokenType.Integer)
            {
                value = (float)t.Value<double>();
                return true;
            }
            return false;
        }

        public bool TryGetString(string path, out string value)
        {
            value = null;
            if (!TryGetToken(path, out var t)) return false;
            if (t.Type == JTokenType.String) { value = t.Value<string>(); return true; }
            return false;
        }
    }
}