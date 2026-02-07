/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    public class InputTracker : MonoBehaviour
    {
        public static Dictionary<string, InputBindingMap> allInputBindingMaps;



        public List<InputBindingMap> mapsToSave = new List<InputBindingMap>();

        private static string filePath => Path.Combine(Application.persistentDataPath, "input_bindings.json");

        [System.Serializable]
        private class MapsDataWrapper
        {
            public List<InputBindingMap_Data> maps = new List<InputBindingMap_Data>();
        }

        private void Awake()
        {
            LoadAll();

            BuildDictionary();
        }

        private void OnDestroy()
        {
            foreach (var map in mapsToSave)
            {
                map.OnDisable();
            }


            SaveAll();
        }


        private void Update()
        {
            SHUU_Input.Update();


            foreach (var map in allInputBindingMaps.Values)
            {
                foreach (var set in map.inputSets_list)
                {
                    foreach (AxisSource source in set.set.validSources.Where(x => x is AxisSource))
                    {
                        source.Tick();
                    }
                }
            }
        }


        private void BuildDictionary()
        {
            allInputBindingMaps = new Dictionary<string, InputBindingMap>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var map in mapsToSave)
            {
                if (map == null) continue;

                string key = map.mapName?.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"InputBindingMap '{map}' has an empty mapName!");

                    continue;
                }

                if (allInputBindingMaps.ContainsKey(key))
                {
                    Debug.LogError($"Duplicate InputBindingMap name detected: '{key}'. " + "Only the first entry will be stored.");

                    continue;
                }

                allInputBindingMaps.Add(key, map);
            }
        }



        // ---------------- SAVE ----------------
        public void SaveAll()
        {
            #if !UNITY_EDITOR
            MapsDataWrapper wrapper = new MapsDataWrapper();

            foreach (var map in mapsToSave)
            {
                if (map != null) wrapper.maps.Add(map.ToData());
            }

            try
            {
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to save input bindings: " + ex);
            }
            #endif
        }

        // ---------------- LOAD ----------------
        public void LoadAll()
        {
            #if !UNITY_EDITOR
            if (!File.Exists(filePath)) return;

            try
            {
                string json = File.ReadAllText(filePath);
                MapsDataWrapper wrapper = JsonUtility.FromJson<MapsDataWrapper>(json);

                if (wrapper == null || wrapper.maps == null) return;

                foreach (var map in mapsToSave)
                {
                    if (map == null) continue;

                    // Match by mapName
                    var data = wrapper.maps.Find(!string.IsNullOrEmpty(m.mapName) && m => m.mapName.Equals(map.mapName, System.StringComparison.OrdinalIgnoreCase));

                    if (data != null) map.LoadFromData(data);
                    else Debug.Log($"InputTracker: No saved data for map '{map.mapName}', using defaults.");
                    // If not found → keep defaults
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load input bindings: " + ex);
            }
            #endif
        }
    }
}
