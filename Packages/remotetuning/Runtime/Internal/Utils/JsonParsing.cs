using System;
using Newtonsoft.Json.Linq;

namespace Ashutosh.RemoteTuning
{
    internal static class JsonParsing
    {
        public static bool TryParseObject(string json, out JObject obj, out string error)
        {
            obj = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "JSON is empty.";
                return false;
            }

            try
            {
                var token = JToken.Parse(json);
                obj = token as JObject;
                if (obj == null)
                {
                    error = "Root JSON token is not an object.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}