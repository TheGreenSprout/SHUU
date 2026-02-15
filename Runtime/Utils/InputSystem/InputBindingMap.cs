using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    [CreateAssetMenu(fileName = "New Input Binding Map", menuName = "SHUU/Input System/Input Binding Map")]
    public class InputBindingMap : AutoSave_Build_ScriptableObject<InputBindingMap>
    {
        private static InputBindingMap allInputBindingMaps_proxy
        {
            set
            {
                if (value == null || string.IsNullOrWhiteSpace(value.mapName)) return;

                foreach (var item in SHUU_Input.allInputBindingMaps)
                {
                    if (item.Value == null) SHUU_Input.allInputBindingMaps.Remove(item.Key);
                }

                if (SHUU_Input.allInputBindingMaps.ContainsKey(value.mapName)) SHUU_Input.allInputBindingMaps[value.mapName] = value;
                else SHUU_Input.allInputBindingMaps.Add(value.mapName, value);
            }
        }



        #region Variables
        [JsonIgnore] protected override InputBindingMap obj => this;

        [JsonIgnore] protected override string id => this.name;



        [JsonIgnore] public string mapName;

        [JsonIgnore] public bool enabled = true;


        public string lastDefaultSetDateTime = "No Default Set";
        [SerializeField] private InputBindingMap_Data defaultData = null;



        [Header("Single Input Sets")]
        public List<NAMED_InputSet> inputSets_list = new List<NAMED_InputSet>();
        [HideInInspector] [JsonIgnore] public Dictionary<string, InputSet> inputSets_dict = null;


        [Header("Composite Input Sets")]
        public List<NAMED_Composite_InputSet> compositeSets_list = new List<NAMED_Composite_InputSet>();
        [HideInInspector] [JsonIgnore] public Dictionary<string, Composite_InputSet> compositeSets_dict = null;
        #endregion




        #region Init
        protected override void OnEnable()
        {
            base.OnEnable();



            allInputBindingMaps_proxy = this;



            foreach (var set in inputSets_list)
            {
                set.overrideAction = null;
            }

            foreach (var set in compositeSets_list)
            {
                set.overrideAction = null;
            }


            //BuildDictionaries();
            ForceBuildDictionaries();
        }

        protected override void OnDisable()
        {
            foreach (var set in inputSets_list)
            {
                set.overrideAction = null;
            }

            foreach (var set in compositeSets_list)
            {
                set.overrideAction = null;
            }



            base.OnDisable();
        }
        #endregion



        #region Cache Dictionaries
        public void ForceBuildDictionaries()
        {
            if (inputSets_dict != null) inputSets_dict = null;
            if (compositeSets_dict != null) compositeSets_dict = null;

            BuildDictionaries();
        }


        public void BuildDictionaries()
        {
            if (inputSets_dict == null)
            {
                inputSets_dict = new Dictionary<string, InputSet>(StringComparer.OrdinalIgnoreCase);

                foreach (NAMED_InputSet namedSet in inputSets_list)
                {
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name)) continue;

                    if (!inputSets_dict.ContainsKey(namedSet.name.Trim())) inputSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    else Debug.LogError($"Duplicate InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " + "Only the first entry will be stored.");
                }
            }

            if (compositeSets_dict == null)
            {
                compositeSets_dict = new Dictionary<string, Composite_InputSet>(StringComparer.OrdinalIgnoreCase);

                foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
                {
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name)) continue;

                    if (!compositeSets_dict.ContainsKey(namedSet.name.Trim())) compositeSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    else Debug.LogError($"Duplicate Composite_InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " + "Only the first entry will be stored.");
                }
            }
        }
        #endregion



        #region Retrieval
        public bool TryGetSingleSet(string name, out InputSet set)
        {
            set = null;


            if (inputSets_dict == null || inputSets_dict.Count == 0) BuildDictionaries();
            if (inputSets_dict.Count == 0) return false;
            

            if (!enabled) return false;


            set = inputSets_dict.GetValueOrDefault(name, null);


            if (set == null || !set.enabled) return false;

            return true;
        }

        public bool TryGetCompositeSet(string name, out Composite_InputSet set)
        {
            set = null;


            if (compositeSets_dict == null || compositeSets_dict.Count == 0) BuildDictionaries();
            if (compositeSets_dict.Count == 0) return false;


            if (!enabled) return false;


            set = compositeSets_dict.GetValueOrDefault(name, null);


            if (set == null || !set.enabled) return false;

            return true;
        }


        public List<NAMED_InputSet> GetAllSingleSets() => inputSets_list;
        public List<NAMED_Composite_InputSet> GetAllCompositeSets() => compositeSets_list;

        public (List<NAMED_InputSet>, List<NAMED_Composite_InputSet>) GetAllSets() => (inputSets_list, compositeSets_list);

        public NAMED_InputSet GetSingleSet(string name)
        {
            foreach (NAMED_InputSet namedSet in inputSets_list)
            {
                if (namedSet.name.ToLower().Equals(name.ToLower())) return namedSet;
            }


            Debug.LogWarning($"No InputSet named '{name}' found in InputBindingMap '{mapName}'.");

            return null;
        }
        public NAMED_Composite_InputSet GetCompositeSet(string name)
        {
            foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
            {
                if (namedSet.name.ToLower().Equals(name.ToLower())) return namedSet;
            }


            Debug.LogWarning($"No Composite InputSet named '{name}' found in InputBindingMap '{mapName}'.");

            return null;
        }
        #endregion



        #region Defaults
        public void SaveDefaults()
        {
            lastDefaultSetDateTime = Stats.timestamp;

            defaultData = ToData();
        }


        public void RestoreDefaults()
        {
            if (defaultData == null || !defaultData.hasValue)
            {
                Debug.LogWarning($"InputBindingMap '{mapName}' has no default data to reset to.");

                return;
            }


            LoadFromData(defaultData);
        }
        #endregion



        #region Saving/Loading
        public InputBindingMap_Data ToData() => new InputBindingMap_Data(this);


        public void LoadFromData(InputBindingMap_Data data)
        {
            if (data == null || !data.hasValue)
            {
                Debug.LogWarning($"InputBindingMap '{mapName}' cannot load invalid data.");

                return;
            }


            List<NAMED_InputSet> single_copyList = new();
            for (int i = 0; i < data.inputSets_list.Count; i++)
            {
                NAMED_InputSet newSet = new NAMED_InputSet(data.inputSets_list[i]);
                this.inputSets_list[i].overrideAction?.Invoke(newSet);
                
                single_copyList.Add(newSet);
            }
            this.inputSets_list.CopyFrom_List_CopyContructors(single_copyList, x => new NAMED_InputSet(x));
            
            List<NAMED_Composite_InputSet> composite_copyList = new();
            for (int i = 0; i < data.compositeSets_list.Count; i++)
            {
                NAMED_Composite_InputSet newSet = new NAMED_Composite_InputSet(data.compositeSets_list[i]);
                this.compositeSets_list[i].overrideAction?.Invoke(newSet);
                
                composite_copyList.Add(newSet);
            }
            this.compositeSets_list.CopyFrom_List_CopyContructors(composite_copyList, x => new NAMED_Composite_InputSet(x));

            ForceBuildDictionaries();
        }
        #endregion
    }
}
