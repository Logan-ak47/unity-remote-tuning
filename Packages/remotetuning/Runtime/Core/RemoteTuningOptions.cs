namespace Ashutosh.RemoteTuning
{
    public sealed class RemoteTuningOptions
    {
        public string EndpointUrl;
        public int TimeoutSeconds = 10;

        // Cache policy placeholders (Day 3)
        public int TtlSeconds = 300;
        public int StaleSeconds = 1800;

        public RemoteTuningOptions(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
    }
}