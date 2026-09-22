using System;
using System.Collections.Generic;

namespace SHUU.Samples.Blobatar.InnerWorkings
{
    /// <summary>
    /// Reads named "traits" (independent [0,1) values) derived from a seed,
    /// with helpers to map them into ranges, integers, picks, booleans, and
    /// symmetric jitter. Optionally, specific keys can be pinned to a fixed
    /// value or restricted to a small fixed set of choices via <paramref name="overrides"/>.
    /// Ported from traits.ts.
    /// </summary>
    public class Traits
    {
        readonly uint state;
        readonly Dictionary<string, float[]> overrides;

        /// <param name="seed">Any string. The same seed always yields the same traits.</param>
        /// <param name="normalize">NFC-normalize, trim, and lowercase the seed first (recommended).</param>
        /// <param name="overrides">
        /// Optional per-key overrides. A single-element array pins the key to that value.
        /// A multi-element array restricts the key to one of those values (still chosen
        /// deterministically from the seed). Omit a key to use the seed's natural value.
        /// </param>
        public Traits(string seed, bool normalize = true, Dictionary<string, float[]> overrides = null)
        {
            state = BlobatarHash.SeedState(seed, normalize);
            this.overrides = overrides;
        }

        /// <summary>Equivalent to calling the trait as a function in the original library: t("key").</summary>
        public float this[string key] => Get(key);

        public float Get(string key)
        {
            float? o = null;
            if (overrides != null && overrides.TryGetValue(key, out var arr))
            {
                if (arr == null || arr.Length == 0)
                {
                    o = null;
                }
                else if (arr.Length == 1)
                {
                    o = arr[0];
                }
                else
                {
                    int idx = (int)Math.Floor(BlobatarHash.Stream(state, key) * arr.Length);
                    if (idx >= arr.Length) idx = arr.Length - 1;
                    if (idx < 0) idx = 0;
                    o = arr[idx];
                }
            }

            if (o == null) return BlobatarHash.Stream(state, key);

            float v = o.Value;
            if (v > 0f) return v < 1f ? v : 0.999999f;
            return 0f;
        }

        public float Num(string key, float min, float max) => min + Get(key) * (max - min);

        public int Int(string key, int min, int max) => min + (int)Math.Floor(Get(key) * (max - min + 1));

        public T Pick<T>(string key, IReadOnlyList<T> options)
        {
            int idx = (int)Math.Floor(Get(key) * options.Count);
            if (idx >= options.Count) idx = options.Count - 1;
            return options[idx];
        }

        public bool Bool(string key, float p = 0.5f) => Get(key) < p;

        /// <summary>Returns a value in [-amount, amount].</summary>
        public float Jitter(string key, float amount) => (Get(key) * 2f - 1f) * amount;
    }
}
