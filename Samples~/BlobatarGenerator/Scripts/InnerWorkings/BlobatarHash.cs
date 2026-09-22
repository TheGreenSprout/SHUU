using System.Text;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>
    /// Deterministic seed -&gt; float[0,1) hashing, ported 1:1 from the original
    /// blobatar TypeScript library's hash.ts (a small FNV/murmur-style mixer).
    /// The same seed string always produces the same stream of values, on any
    /// platform, which is what lets a blobatar be regenerated from just a name.
    /// </summary>
    public static class BlobatarHash
    {
        public static string NormalizeSeed(string seed)
        {
            return seed.Normalize(System.Text.NormalizationForm.FormC).Trim().ToLowerInvariant();
        }

        static uint Feed(uint h, byte[] bytes)
        {
            unchecked
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    h = (h ^ bytes[i]) * 3432918353u;
                    h = (h << 13) | (h >> 19);
                }
            }
            return h;
        }

        static uint Finalize(uint h)
        {
            unchecked
            {
                h ^= h >> 16;
                h *= 2246822507u;
                h ^= h >> 13;
                h *= 3266489909u;
                h ^= h >> 16;
            }
            return h;
        }

        /// <summary>Derives the root hash state for a seed string.</summary>
        public static uint SeedState(string seed, bool normalize = true)
        {
            string s = normalize ? NormalizeSeed(seed) : seed;
            uint init = unchecked(1779033703u ^ (uint)s.Length);
            return Feed(init, Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Derives a value in [0,1) for a named "trait key" from the root state.
        /// Different keys for the same seed give independent-looking values.
        /// </summary>
        public static float Stream(uint state, string key)
        {
            uint h = Feed(state, new byte[] { 0xFF });
            h = Feed(h, Encoding.UTF8.GetBytes(key));
            h = Finalize(h);
            return h / 4294967296f;
        }
    }
}
