using System;
using System.Collections.Generic;
using UnityEngine;

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.RandomSystem
{
    [CreateAssetMenu(fileName = "RandomProvider", menuName = "SHUU/RandomProvider")]
    public class RandomProvider_Asset : ScriptableObject
    {
        #region Variables
        public string assetName;



        [SerializeField] private List<ProviderEntry> startProviders = new();
        private Dictionary<string, RandomProvider> provider_dict = new();



        private static bool debugLogEmission => SHUU_Preferences.instance.randomSystem_debugLogEmission;
        #endregion




        #region Main
        private void OnEnable() => CacheDictionary();


        private void OnValidate()
        {
            foreach (var entry in startProviders)
                entry.ValidateType();
        }
        #endregion



        #region Logic
        public void CacheDictionary()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif


            if (provider_dict == null) provider_dict = new();
            else if (provider_dict.Count > 0) provider_dict.Clear();


            foreach (var entry in startProviders)
            {
                RandomProvider provider = entry.Provider();


                if (provider_dict.ContainsKey(entry.GetName()) && debugLogEmission)
                    Debug.LogError($"Duplicate provider name detected in {assetName} asset. Name: {entry.GetName()}");

                provider_dict[entry.GetName()] = provider;
            }
        }


        public RandomProvider GetProvider(string name) => provider_dict[name];
        #endregion
    }




    #region Helper classes
    public enum SeedType
    {
        Generate,
        String,
        Int
    }


    [Serializable]
    public class ProviderEntry
    {
        public SeedType seedType = SeedType.Generate;

        [SerializeReference]
        public ProviderEntryVariables variables = new Generate_ProviderEntryVariables();



        public string GetName() => variables.name;

        public RandomProvider Provider() => variables.Provider();




        public void ValidateType()
        {
            switch (seedType)
            {
                case SeedType.Generate:
                    if (variables is not Generate_ProviderEntryVariables) variables = new Generate_ProviderEntryVariables();
                    break;

                case SeedType.String:
                    if (variables is not String_ProviderEntryVariables) variables = new String_ProviderEntryVariables();
                    break;

                case SeedType.Int:
                    if (variables is not Int_ProviderEntryVariables) variables = new Int_ProviderEntryVariables();
                    break;
            }
        }



        [Serializable]
        public abstract class ProviderEntryVariables
        {
            public string name = null;

            public abstract RandomProvider Provider();
        }

        [Serializable]
        public class Generate_ProviderEntryVariables : ProviderEntryVariables
        {
            public override RandomProvider Provider() => new RandomProvider(name);
        }
        [Serializable]
        public class String_ProviderEntryVariables : ProviderEntryVariables
        {
            [SerializeField] protected string seedString = null;

            public override RandomProvider Provider() => new RandomProvider(seedString, name);
        }
        [Serializable]
        public class Int_ProviderEntryVariables : ProviderEntryVariables
        {
            [SerializeField] protected int seedInt = 0;

            public override RandomProvider Provider() => new RandomProvider(seedInt, name);
        }
    }
    #endregion
}
