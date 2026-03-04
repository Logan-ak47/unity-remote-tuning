namespace Ashutosh.RemoteTuning
{
    internal sealed class NullLogSink : ILogSink
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { }
    }
}