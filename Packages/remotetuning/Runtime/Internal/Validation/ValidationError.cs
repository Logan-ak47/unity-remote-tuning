namespace Ashutosh.RemoteTuning
{
    internal readonly struct ValidationError
    {
        public readonly string Path;
        public readonly string Message;

        public ValidationError(string path, string message)
        {
            Path = path;
            Message = message;
        }

        public override string ToString() => $"{Path}: {Message}";
    }
}