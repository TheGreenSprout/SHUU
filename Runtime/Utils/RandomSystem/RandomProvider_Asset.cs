using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.RandomSystem
{
    [CreateAssetMenu(fileName = "RandomProvider", menuName = "SHUU/RandomProvider")]
    public class RandomProvider_Asset : ScriptableObject
    {
        public string assetName;



        [SerializeField] private List<ProviderEntry> startProviders = new();
        private Dictionary<string, RandomProvider> provider_dict = new();




        private void OnEnable() => CacheDictionary();



        public void CacheDictionary()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif


            if (provider_dict == null) provider_dict = new();
            else if (provider_dict.Count > 0) provider_dict.Clear();


            foreach (var entry in startProviders)
            {
                if (string.IsNullOrEmpty(entry.name) && entry.seed == 0)
                {
                    Debug.LogWarning($"Invalid entry detected on RandomProvider_Asset '{assetName}'.");

                    continue;
                }
                else if (string.IsNullOrEmpty(entry.name)) entry.name = entry.seed.ToString();

                if (provider_dict.ContainsKey(entry.name))
                {
                    Debug.LogWarning($"Repeated key entry detected on RandomProvider_Asset '{assetName}'.");

                    continue;
                }


                RandomProvider provider = null;

                switch (entry.seedType)
                {
                    case SeedType.Generate:
                        provider = new RandomProvider();
                        break;
                }

                provider_dict.Add(entry.name, provider);
            }
        }


        public RandomProvider GetProvider(string name) => provider_dict.TryGetValue(name, out var result) ? result : null;
        public RandomProvider GetProvider(int seed) => GetProvider(seed.ToString());
    }



    public enum SeedType
    {
        Generate,
        Name,
        Int
    }
    [Serializable]
    public class ProviderEntry
    {
        public string name = null;

        public int seed;


        public SeedType seedType;
    }
}
