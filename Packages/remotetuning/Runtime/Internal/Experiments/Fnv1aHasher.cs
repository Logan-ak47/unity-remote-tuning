using System.Text;

namespace Ashutosh.RemoteTuning
{
    internal sealed class Fnv1aHasher : IStableHasher
    {
        // FNV-1a 32-bit constants
        private const uint OffsetBasis = 2166136261;
        private const uint Prime = 16777619;

        public uint Hash32(string input)
        {
            if (input == null) input = string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            uint hash = OffsetBasis;

            for (int i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= Prime;
            }

            return hash;
        }
    }
}