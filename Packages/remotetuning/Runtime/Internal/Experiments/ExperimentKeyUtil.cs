namespace Ashutosh.RemoteTuning
{
    internal static class ExperimentKeyUtil
    {
        // bump v1 if you change encoding format
        private const string Prefix = "Ashutosh.RemoteTuning.Assignments.v1.";

        public static string BuildAssignmentKey(string userId, string experimentKey)
            => $"{Prefix}{userId}::{experimentKey}";
    }
}