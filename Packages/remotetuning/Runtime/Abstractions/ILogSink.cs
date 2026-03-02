namespace Ashutosh.RemoteTuning
{
    public interface ILogSink
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }
}