namespace Ashutosh.RemoteTuning.Tests
{
    internal sealed class FakeLogSink : ILogSink
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }
}