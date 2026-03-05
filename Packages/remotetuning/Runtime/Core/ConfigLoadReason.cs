namespace Ashutosh.RemoteTuning
{
    public enum ConfigLoadReason
    {
        Default_NoCache,
        Default_ExpiredCache,
        Cache_Fresh,
        Cache_Stale_Served,
        Remote_Success,
        Remote_NotModified,
        Remote_Failed_UsingCache,
        Remote_Failed_UsingDefault,
        Remote_Invalid_UsingCache,
        Remote_Invalid_UsingDefault
    }
}