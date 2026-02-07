using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.RandomSystem
{
    [Serializable]
    public class RandomProvider
    {
        public string providerName {get; private set;}



        public int seed {get; private set;}

        private System.Random rng = null;


        private int calls = 0;




        #region Setup
        public RandomProvider(int seed, string name = null)
        {
            if (string.IsNullOrEmpty(name)) providerName = seed.ToString();
            else providerName = name;


            this.seed = seed;

            rng = new System.Random(this.seed);
        }


        public RandomProvider() : this(GenerateSeed()) { }

        public RandomProvider(string seedString) : this(GenerateSeed(seedString), seedString) { }
        #endregion



        #region Logic
        public int Range(int min, int max)
        {
            calls++;
            return rng.Next(min, max);
        }

        public float NextFloat() => (float)NextDouble();
        private double NextDouble()
        {
            calls++;
            return rng.NextDouble();
        }


        public bool Chance01(float probability) => NextFloat() < probability;
        public bool ChancePercent(float percent) => Chance01(percent/100f);

        public int Sign() => NextFloat() < 0.5f ? -1 : 1;


        public T Pick<T>(IList<T> list) => list[Range(0, list.Count)];

        public T PickWeighted<T>(IList<T> items, IList<float> weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                total += weights[i];
            }

            float roll = NextFloat() * total;

            for (int i = 0; i < items.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return items[i];
            }

            return items[^1];
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
        public List<T> Shuffle_NonMutable<T>(IEnumerable<T> source)
        {
            var list = new List<T>(source);
            Shuffle(list);
            
            return list;
        }


        public float Noise2D(int x, int y)
        {
            unchecked
            {
                int h = seed;
                h = h * 31 + x;
                h = h * 31 + y;
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        public float HashNoise(int x, int y, int z = 0)
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
            var tex = new Texture2D(width, height);
            tex.filterMode = FilterMode.Point;

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
        public RandomProvider Fork(int offset) => new RandomProvider(seed + offset);
        public RandomProvider Fork(string channel) => new RandomProvider(seed + GenerateSeed(channel));

        public RandomProvider Clone() => new RandomProvider(seed);


        public int GetState() => calls;

        public void RestoreState(int callCount)
        {
            rng = new System.Random(seed);

            calls = 0;
            for (int i = 0; i < callCount; i++)
            {
                rng.Next();
            }
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
}
