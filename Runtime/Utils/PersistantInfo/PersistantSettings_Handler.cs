using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

using SHUU.Utils.SettingsSystem;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.PersistantInfo
{
    public class PersistantSettings_Handler : MonoBehaviour
    {
        #region Variables
        [SerializeField] private List<SettingsUpdate> settingsUpdate = new();
        private Dictionary<SettingsAtlas, Dictionary<string, UnityEvent<SettingsAtlas, string, string>>> settingsUpdate_dict = new();
        #endregion




        #region Main
        private void Awake()
        {
            BuildDictionary();

            if (settingsUpdate_dict == null) return;

            foreach (var settings in settingsUpdate_dict.Keys)
                if (settings != null) settings.onSettingsChanged += SettingsUpdate;
        }

        private void OnDestroy()
        {
            if (settingsUpdate_dict == null) return;

            foreach (var settings in settingsUpdate_dict.Keys)
                if (settings != null) settings.onSettingsChanged -= SettingsUpdate;
        }


        private void SettingsUpdate(SettingsAtlas data, string mapName, string fieldKey)
        {
            if (settingsUpdate_dict == null) BuildDictionary();
            if (data == null || mapName == null || fieldKey == null) return;

            string key = CompositeKey(mapName, fieldKey);
            if (!settingsUpdate_dict.ContainsKey(data) || !settingsUpdate_dict[data].ContainsKey(key)) return;

            settingsUpdate_dict[data][key].Invoke(data, mapName, fieldKey);
        }
        #endregion




        #region Logic
        private void BuildDictionary()
        {
            settingsUpdate_dict = new();

            if (settingsUpdate == null) return;
            settingsUpdate.Clean();

            foreach (var update in settingsUpdate)
            {
                if (update.settingsData == null || update.maps == null) continue;

                if (!settingsUpdate_dict.ContainsKey(update.settingsData))
                    settingsUpdate_dict[update.settingsData] = new();

                var dict = settingsUpdate_dict[update.settingsData];

                foreach (var map in update.maps)
                {
                    if (map.items == null) continue;

                    foreach (var item in map.items)
                        dict[CompositeKey(map.mapName, item.field)] = item.updateEvent;
                }
            }
        }

        private static string CompositeKey(string mapName, string fieldKey) => $"{mapName}\0{fieldKey}";
        #endregion
    }




    #region Helper classes
    [Serializable]
    public class SettingsUpdate
    {
        public SettingsAtlas settingsData;

        public List<SettingsUpdate_Map> maps;
    }


    [Serializable]
    public class SettingsUpdate_Map
    {
        public string mapName;

        public List<SettingsUpdate_Item> items;
    }


    [Serializable]
    public class SettingsUpdate_Item
    {
        public string field;

        public UnityEvent<SettingsAtlas, string, string> updateEvent;
    }
    #endregion
}
