/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    public class InputTracker : MonoBehaviour
    {
        public static Dictionary<string, InputBindingMap> allInputBindingMaps;



        public List<InputBindingMap> mapsToSave = new List<InputBindingMap>();

        private static string FilePath => Path.Combine(Application.persistentDataPath, "input_bindings.json");

        [System.Serializable]
        private class MapsDataWrapper
        {
            public List<InputBindingMap_Data> maps = new List<InputBindingMap_Data>();
        }

        void Awake()
        {
            LoadAll();

            BuildDictionary();
        }

        void OnDestroy()
        {
            SaveAll();
        }


        private void BuildDictionary()
        {
            allInputBindingMaps = new Dictionary<string, InputBindingMap>(
                System.StringComparer.OrdinalIgnoreCase
            );

            foreach (var map in mapsToSave)
            {
                if (map == null)
                    continue;

                string key = map.mapName.Trim();

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogWarning($"InputBindingMap '{map}' has an empty mapName!");
                    continue;
                }

                if (allInputBindingMaps.ContainsKey(key))
                {
                    Debug.LogError($"Duplicate InputBindingMap name detected: '{key}'. " +
                                "Only the first entry will be stored.");
                    continue;
                }

                allInputBindingMaps.Add(key, map);
            }
        }



        // ---------------- SAVE ----------------
        public void SaveAll()
        {
            MapsDataWrapper wrapper = new MapsDataWrapper();

            foreach (var map in mapsToSave)
            {
                if (map != null)
                    wrapper.maps.Add(map.ToData());
            }

            string json = JsonUtility.ToJson(wrapper, true);

            try
            {
                File.WriteAllText(FilePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to save input bindings: " + ex);
            }
        }

        // ---------------- LOAD ----------------
        public void LoadAll()
        {
            if (!File.Exists(FilePath))
                return;

            try
            {
                string json = File.ReadAllText(FilePath);
                MapsDataWrapper wrapper = JsonUtility.FromJson<MapsDataWrapper>(json);

                foreach (var map in mapsToSave)
                {
                    if (map == null) continue;

                    // Match by mapName
                    var data = wrapper.maps.Find(m => 
                        m.mapName.Equals(map.mapName, System.StringComparison.OrdinalIgnoreCase));

                    if (data != null)
                    {
                        map.LoadFromData(data);
                    }
                    // If not found → keep defaults
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to load input bindings: " + ex);
            }
        }
    }
}
