using System;
using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Helpers;
using SHUU.InnerWorkings.Preferences;

namespace SHUU.Utils.SettingsSystem
{
    [CreateAssetMenu(fileName = "SettingsAtlas", menuName = "SHUU/SettingsAtlas")]
    public class SettingsAtlas : AutoSave_Build_ScriptableObject<SettingsAtlas>
    {
        #region Variables

        #region Static
        private static SettingsAtlas AllSettingsData_proxy
        {
            set
            {
                if (value == null || string.IsNullOrWhiteSpace(value.settingsName)) return;


                var keysToRemove = new List<string>();

                foreach (var item in AllSettingsData)
                    if (item.Value == null) keysToRemove.Add(item.Key);

                foreach (var key in keysToRemove)
                    AllSettingsData.Remove(key);


                if (AllSettingsData.ContainsKey(value.settingsName)) AllSettingsData[value.settingsName] = value;
                else AllSettingsData.Add(value.settingsName, value);
            }
        }


        public static SettingsAtlas DefaultAtlas => SHUUPreferences_SettingsSystem.DefaultAtlas;

        public static Dictionary<string, SettingsAtlas> AllSettingsData = new();
        public static SettingsAtlas GetSettingsAtlas(string name) => AllSettingsData.GetValueOrDefault(name);
        #endregion



        #region Overrides
        protected override SettingsAtlas obj => this;
        
        protected override string id => name;
        #endregion



        #region General
        public string settingsName;

        
        public List<SettingMap> maps = new();


        public string lastDefaultSetDateTime = "No Default Set";

        public SettingsAtlas_Data defaultData;


        public event Action<SettingsAtlas, string, string> onSettingsChanged;

        public void NotifyChanged(string mapName, string fieldKey) => onSettingsChanged?.Invoke(this, mapName, fieldKey);


        public bool generateStaticReferences = false;
        #endregion



        private static bool DebugLogEmission => SHUUPreferences_SettingsSystem.Instance != null && SHUUPreferences_SettingsSystem.Instance.debugLogEmission;

        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

            AllSettingsData_proxy = this;
            BindAllMaps();
        }
        #endregion



        #region Logic

        #region Map access
        public SettingMap GetMap(string mapName)
        {
            var map = maps.Find(x => x.mapName == mapName);
            if (map == null) return SettingMap.Null;

            BindMap(map);
            return map;
        }

        private void BindMap(SettingMap map)
        {
            if (map != null && map != SettingMap.Null) map.Bind(this);
        }

        private void BindAllMaps()
        {
            foreach (var map in maps)
                BindMap(map);
        }

        public bool TryGetMap(string mapName, out SettingMap map) => (map = GetMap(mapName)) != null && map != SettingMap.Null;
        #endregion



        #region Field access
        public bool GetBool (string mapName, string key) => GetMap(mapName).GetBool(key);
        
        public int GetInt (string mapName, string key) => GetMap(mapName).GetInt(key);
        
        public float GetFloat (string mapName, string key) => GetMap(mapName).GetFloat(key);
        
        public string GetString(string mapName, string key) => GetMap(mapName).GetString(key);
        
        public int GetEnumInt(string mapName, string key) => GetMap(mapName).GetEnumInt(key);
        public T GetEnum<T>(string mapName, string key) where T : Enum => GetMap(mapName).GetEnum<T>(key);


        public bool SetBool (string mapName, string key, bool value) => GetMap(mapName).SetBool(key, value);
        
        public bool SetInt (string mapName, string key, int value) => GetMap(mapName).SetInt(key, value);
        
        public bool SetFloat (string mapName, string key, float  value) => GetMap(mapName).SetFloat(key, value);
        
        public bool SetString (string mapName, string key, string value) => GetMap(mapName).SetString(key, value);
        
        public bool SetEnumInt (string mapName, string key, int value) => GetMap(mapName).SetEnumInt(key, value);
        public bool SetEnum<T> (string mapName, string key, T value) where T : Enum => GetMap(mapName).SetEnum(key, value);

        
        public SettingField GetField (string mapName, string key) => GetMap(mapName).GetSettingField(key);
        public Type GetFieldType(string mapName, string key) => GetMap(mapName).GetSettingFieldType(key);
        
        public bool SetField(string mapName, string key, object value) => GetMap(mapName).SetField(key, value);
        #endregion



        #region Defaults
        public void SaveAsDefaults()
        {
            foreach (var map in maps)
                map.SaveAsDefaults();

            defaultData = new SettingsAtlas_Data(this);
            lastDefaultSetDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void RestoreDefaults()
        {
            if (defaultData == null || !defaultData.hasValue)
            {
                if (DebugLogEmission)
                    Debug.LogWarning($"SettingsAtlas '{settingsName}' has no saved defaults.");
                return;
            }

            
            var restored = new List<SettingMap>();

            foreach (var snapshot in defaultData.maps)
            {
                var live = maps.Find(m => m.mapName == snapshot.mapName);
                if (live == null) live = new SettingMap { mapName = snapshot.mapName };

                live.ApplySnapshot(snapshot);
                restored.Add(live);
            }

            maps = restored;
            BindAllMaps();


            NotifyChanged(null, null);
        }
        #endregion

        #endregion
    }
}
