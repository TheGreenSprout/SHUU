using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.RandomSystem
{
    [Serializable]
    public class RandomProvider
    {
        #region Variables
        public string providerName {get; private set;}



        public int seed {get; private set;}

        private Pcg32 rng;



        private ulong state => rng.state;
        private ulong increment => rng.increment;
        #endregion




        #region Main
        public RandomProvider(int seed, string name = null)
        {
            if (string.IsNullOrEmpty(name)) providerName = seed.ToString();
            else providerName = name;


            this.seed = seed;

            rng = new Pcg32((ulong)this.seed);
        }

        public RandomProvider(string name = null) : this(GenerateSeed(), name) { }
        public RandomProvider(string seedString, string name = null) : this(GenerateSeed(seedString), name ?? seedString) { }
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
        public T PickWeighted<T>(params (T, int)[] items)
        {
            var weightedItems = new WeightedItem<T>[items.Length];

            for (int i = 0; i < items.Length; i++)
                weightedItems[i] = new WeightedItem<T>(items[i].Item1, items[i].Item2);

            return PickWeighted(weightedItems);
        }

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

        
        public RandomProvider Fork(int offset) => new RandomProvider((int)(ulong)(seed ^ (offset * 0x9E3779B9)));
        public RandomProvider Fork(string channel) => new RandomProvider(seed + GenerateSeed(channel));

        public RandomProvider CloneSeed() => new RandomProvider(seed);
        public RandomProvider CloneState()
        {
            var clone = new RandomProvider(seed);
            clone.RestoreState(GetState());
            return clone;
        }


        public RandomState GetState() => new RandomState { state = state, increment = increment };

        public void RestoreState(RandomState saved)
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
    }




    #region Helper classes
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


    public struct RandomState
    {
        public ulong state;
        public ulong increment;
    }



    public class WeightedItem<T>
    {
        public T item;
        public float weight;

        public WeightedItem(T item, float weight)
        {
            this.item = item;
            this.weight = weight;
        }
    }



    public class PerlinNoise2D
    {
        private float scale;

        private float offsetX;
        private float offsetY;



        public PerlinNoise2D(RandomProvider rng, float scale = 1f)
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
}
