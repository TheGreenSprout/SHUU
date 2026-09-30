using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SHUU.Utils.RandomSystem
{
    [Serializable]
    public class SHUU_RandomProvider
    {
        #region Variables
        public int seed {get; private set;}

        private Pcg32 rng;


        private ulong state => rng.state;
        private ulong increment => rng.increment;
        #endregion




        #region Main
        public SHUU_RandomProvider(int seed)
        {
            this.seed = seed;
            rng = new Pcg32((ulong)this.seed);
        }

        public SHUU_RandomProvider() : this(GenerateSeed()) { }
        public SHUU_RandomProvider(string seedString) : this(GenerateSeed(seedString)) { }
        #endregion



        #region Logic

        #region Random Gen
        public uint NextUInt() => rng.NextUInt();
        public float NextFloat() => rng.NextFloat();

        public int Range(int min, int max) => rng.Range(min, max);


        public bool Chance01(float probability) => NextFloat() < probability;
        public bool ChancePercent(float percent) => Chance01(percent/100f);

        public int Sign() => NextFloat() < 0.5f ? -1 : 1;


        public T Pick<T>(IList<T> list) => list[Range(0, list.Count)];
        public T Pick<T>(params T[] items) => Pick((IList<T>)items);

        public T PickWeighted<T>(IList<T> items, IList<float> weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
                total += weights[i];

            float roll = NextFloat() * total;

            for (int i = 0; i < items.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return items[i];
            }

            return items[^1];
        }
        public T PickWeighted<T>(IList<WeightedItem<T>> items)
        {
            float total = 0f;

            for (int i = 0; i < items.Count; i++)
                total += items[i].weight;

            float roll = NextFloat() * total;

            for (int i = 0; i < items.Count; i++)
            {
                roll -= items[i].weight;

                if (roll <= 0f)
                    return items[i].item;
            }

            return items[^1].item;
        }
        public T PickWeighted<T>(params WeightedItem<T>[] items) => PickWeighted(items);

        public List<T> PickUniques<T>(IList<T> source, int count)
        {
            var copy = new List<T>(source);
            Shuffle(copy);
            return copy.GetRange(0, Math.Min(count, copy.Count));
        }

        public void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        public List<T> ToShuffled<T>(IEnumerable<T> source)
        {
            var list = new List<T>(source);
            Shuffle(list);
            
            return list;
        }


        public float CoordinateHash2D(int x, int y)
        {
            unchecked
            {
                int h = seed;
                h = h * 31 + x;
                h = h * 31 + y;
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        public float CoordinateHash3D(int x, int y, int z = 0)
        {
            unchecked
            {
                int h = seed;
                h = h * 31 + x;
                h = h * 31 + y;
                h = h * 31 + z;
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }


        public static Texture2D Noise2D_Texture(int width, int height, Func<int, int, float> sampler)
        {
            var tex = new Texture2D(width, height) { filterMode = FilterMode.Point };

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float v = sampler(x, y);
                    tex.SetPixel(x, y, new Color(v, v, v));
                }

            tex.Apply();
            return tex;
        }
        #endregion



        #region Misc
        public int GetSeed() => seed;


        public void Reset() => rng = new Pcg32((ulong)GenerateSeed());
        public void Reset(int seed) => rng = new Pcg32((ulong)seed);
        public void Reset(string seedString) => rng = new Pcg32((ulong)GenerateSeed(seedString));

        
        public SHUU_RandomProvider Fork(int offset) => new SHUU_RandomProvider((int)(ulong)(seed ^ (offset * 0x9E3779B9)));
        public SHUU_RandomProvider Fork(string channel) => new SHUU_RandomProvider(seed + GenerateSeed(channel));

        public SHUU_RandomProvider CloneSeed() => new SHUU_RandomProvider(seed);
        public SHUU_RandomProvider CloneState()
        {
            var clone = new SHUU_RandomProvider(seed);
            clone.RestoreState(GetState());
            return clone;
        }


        public State GetState() => new State { state = state, increment = increment };

        public void RestoreState(State saved)
        {
            rng.state = saved.state;
            rng.increment = saved.increment;
        }
        #endregion



        #region Statics
        public static int GenerateSeed_Guid() => Guid.NewGuid().GetHashCode();
        public static int GenerateSeed()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Environment.TickCount;
                hash = hash * 31 + DateTime.Now.Millisecond;

                return hash;
            }
        }

        public static int GenerateSeed(string value)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in value)
                    hash = hash * 31 + c;

                return hash;
            }
        }
        #endregion

        #endregion




        #region Helper classes
        public struct State
        {
            public ulong state;
            public ulong increment;
        }



        public class Pcg32
        {
            internal ulong state;
            internal ulong increment;


            public Pcg32(ulong seed, ulong sequence = 1)
            {
                increment = (sequence << 1) | 1UL;

                state = 0;
                NextUInt();

                state += seed;
                NextUInt();
            }

            public uint NextUInt()
            {
                ulong oldState = state;

                state = oldState * 6364136223846793005UL + increment;

                uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
                int rot = (int)(oldState >> 59);

                return (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            }
            public int Range(int min, int max)
            {
                if (max <= min) throw new ArgumentException("max must be greater than min");
                
                return (int)(NextUInt() % (uint)(max - min)) + min;
            }
            public float NextFloat() => (NextUInt() >> 8) * (1f / (1 << 24));
        }
        #endregion
    }





    #region Data classes

    #region Weighted Item
    public class WeightedItem<T>
    {
        public T item;
        public float weight;



        public WeightedItem(T item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
        public WeightedItem(T item, int weight)
        {
            this.item = item;
            this.weight = weight;
        }

        public static implicit operator WeightedItem<T>((T item, float weight) value) => new WeightedItem<T>(value.item, value.weight);
        public static implicit operator WeightedItem<T>((T item, int weight) value) => new WeightedItem<T>(value.item, value.weight);
    }
    #endregion




    #region Perlin Noise
    public class PerlinNoise2D
    {
        private float scale;

        private float offsetX;
        private float offsetY;



        public PerlinNoise2D(SHUU_RandomProvider rng, float scale = 1f)
        {
            this.scale = scale;

            offsetX = rng.Range(-100_000, 100_000);
            offsetY = rng.Range(-100_000, 100_000);
        }


        public float Sample(float x, float y) => Mathf.PerlinNoise((x + offsetX) * scale, (y + offsetY) * scale);

        public float FractalNoise(float x, float y, int octaves, float lacunarity = 2f, float persistence = 0.5f)
        {
            float value = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float max = 0f;

            for (int i = 0; i < octaves; i++)
            {
                value += Sample(x * frequency, y * frequency) * amplitude;
                max += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return value / max;
        }

        public float[,] Noise2D_Map(int width, int height)
        {
            var map = new float[width, height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                map[x, y] = Sample(x, y);

            return map;
        }
    }
    #endregion




    #region Marble Bag
    public class MarbleBag<T>
    {
        #region Variables
        private readonly SHUU_RandomProvider rng;
        
        private readonly List<T> bag = new();
        private readonly List<T> pool;


        public int RemainingInBag => bag.Count;
        public int PoolSize => pool.Count;
        #endregion



        #region Main
        #region XML doc
        /// <summary>
        /// One marble per item; put the same item in more than once to make it more likely to be drawn.
        /// </summary>
        #endregion
        public MarbleBag(SHUU_RandomProvider rng, IEnumerable<T> items) : this(rng, items.ToArray()) { }
        #region XML doc
        /// <summary>
        /// One marble per item; put the same item in more than once to make it more likely to be drawn.
        /// </summary>
        #endregion
        public MarbleBag(SHUU_RandomProvider rng, params T[] items)
        {
            this.rng = rng;
            pool = new List<T>(items);

            Refill();
        }

        #region XML doc
        /// <summary>
        /// Builds a bag from weighted items: a weight of 3 puts 3 marbles of that item in the bag, so it comes up 3 times as often as a
        /// weight-1 item. Weights are rounded to the nearest whole marble, so anything under 0.5 never makes it in.
        /// </summary>
        #endregion
        public static MarbleBag<T> FromWeighted(SHUU_RandomProvider rng, IList<WeightedItem<T>> items)
        {
            List<T> expanded = new();

            foreach (var entry in items)
            {
                int count = Mathf.RoundToInt(entry.weight);

                for (int i = 0; i < count; i++) expanded.Add(entry.item);
            }

            return new MarbleBag<T>(rng, expanded);
        }
        #region XML doc
        /// <summary>
        /// Builds a bag from weighted items: a weight of 3 puts 3 marbles of that item in the bag, so it comes up 3 times as often as a
        /// weight-1 item. Weights are rounded to the nearest whole marble, so anything under 0.5 never makes it in.
        /// </summary>
        #endregion
        public static MarbleBag<T> FromWeighted(SHUU_RandomProvider rng, params WeightedItem<T>[] items) => FromWeighted(rng, items.ToList());
        #endregion


        #region Logic
        #region XML doc
        /// <summary>
        /// Takes one marble out of the bag, refilling and reshuffling first if it's empty.
        /// </summary>
        #endregion
        public T Draw()
        {
            if (bag.Count == 0) Refill();
            if (bag.Count == 0) throw new InvalidOperationException("The marble bag is empty (it has no marbles to refill with).");

            int last = bag.Count - 1;
            T picked = bag[last];
            bag.RemoveAt(last);

            return picked;
        }

        #region XML doc
        /// <summary>
        /// Puts every marble back in the bag and reshuffles, whether or not it was empty. What a refill draws from (the pool) is unaffected.
        /// </summary>
        #endregion
        public void Refill()
        {
            bag.Clear();
            bag.AddRange(pool);

            rng.Shuffle(bag);
        }

        #region XML doc
        /// <summary>
        /// Adds a marble to the pool (so future refills include it too) and to the bag currently being drawn from, at a random position.
        /// </summary>
        #endregion
        public void Add(T item)
        {
            pool.Add(item);

            bag.Insert(rng.Range(0, bag.Count + 1), item);
        }
        #region XML doc
        /// <summary>
        /// Adds several marbles at once, each one placed independently (see Add(T)).
        /// </summary>
        #endregion
        public void Add(params T[] items)
        {
            foreach (T item in items)
                Add(item);
        }

        #region XML doc
        /// <summary>
        /// Adds a weighted marble: a weight of 3 adds 3 copies of the item (see FromWeighted), each placed independently. Weights are rounded
        /// to the nearest whole marble, so anything under 0.5 adds none.
        /// </summary>
        #endregion
        public void Add(WeightedItem<T> entry)
        {
            int count = Mathf.RoundToInt(entry.weight);

            for (int i = 0; i < count; i++)
                Add(entry.item);
        }
        #region XML doc
        /// <summary>
        /// Adds several weighted marbles at once (see Add(WeightedItem&lt;T&gt;)).
        /// </summary>
        #endregion
        public void Add(params WeightedItem<T>[] entries)
        {
            foreach (WeightedItem<T> entry in entries)
                Add(entry);
        }
        #endregion
    }
    #endregion

    #endregion
}
