using UnityEngine;

namespace Ashutosh.RemoteTuning
{
    internal sealed class UnityLogSink : ILogSink
    {
        public void Info(string message) => Debug.Log(message);
        public void Warn(string message) => Debug.LogWarning(message);
        public void Error(string message) => Debug.LogError(message);
    }
}