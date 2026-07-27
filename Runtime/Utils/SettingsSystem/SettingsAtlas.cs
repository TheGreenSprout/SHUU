using System;
using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Helpers;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.SettingsSystem
{
    [CreateAssetMenu(fileName = "SettingsAtlas", menuName = "SHUU/SettingsAtlas")]
    public class SettingsAtlas : AutoSave_Build_ScriptableObject<SettingsAtlas>
    {
        #region Variables

        #region Static
        private static SettingsAtlas allSettingsData_proxy
        {
            set
            {
                if (value == null || string.IsNullOrWhiteSpace(value.settingsName)) return;


                var keysToRemove = new List<string>();

                foreach (var item in allSettingsData)
                    if (item.Value == null) keysToRemove.Add(item.Key);

                foreach (var key in keysToRemove)
                    allSettingsData.Remove(key);


                if (allSettingsData.ContainsKey(value.settingsName)) allSettingsData[value.settingsName] = value;
                else allSettingsData.Add(value.settingsName, value);
            }
        }


        public static SettingsAtlas defaultAtlas => SHUU_Preferences_SettingsSystem.defaultAtlas;

        public static Dictionary<string, SettingsAtlas> allSettingsData = new();
        public static SettingsAtlas GetSettingsAtlas(string name) => allSettingsData.GetValueOrDefault(name);
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



        private static bool debugLogEmission => SHUU_Preferences.instance.settingsSystem_debugLogEmission;

        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

            allSettingsData_proxy = this;
        }
        #endregion



        #region Logic

        #region Map access
        public SettingMap GetMap(string mapName) => maps.Find(x => x.mapName == mapName) ?? SettingMap.Null;

        public bool TryGetMap(string mapName, out SettingMap map) => (map = GetMap(mapName)) != null && map != SettingMap.Null;
        #endregion



        #region Field access
        public bool GetBool (string mapName, string key) => GetMap(mapName).GetBool(key);
        
        public int GetInt (string mapName, string key) => GetMap(mapName).GetInt(key);
        
        public float GetFloat (string mapName, string key) => GetMap(mapName).GetFloat(key);
        
        public string GetString(string mapName, string key) => GetMap(mapName).GetString(key);
        
        public int GetEnumInt(string mapName, string key) => GetMap(mapName).GetEnumInt(key);
        public T GetEnum<T>(string mapName, string key) where T : Enum => GetMap(mapName).GetEnum<T>(key);


        public bool SetBool (string mapName, string key, bool value)
        {
            bool r = GetMap(mapName).SetBool(key, value);

            if (r) NotifyChanged(mapName, key);

            return r;
        }
        
        public bool SetInt (string mapName, string key, int value)
        {
            bool r = GetMap(mapName).SetInt(key, value);

            if (r) NotifyChanged(mapName, key);

            return r;
        }
        
        public bool SetFloat (string mapName, string key, float  value)
        {
            bool r = GetMap(mapName).SetFloat(key, value);

            if (r) NotifyChanged(mapName, key);

            return r;
        }
        
        public bool SetString (string mapName, string key, string value)
        {
            bool r = GetMap(mapName).SetString(key, value);

            if (r) NotifyChanged(mapName, key);

            return r;
        }
        
        public bool SetEnumInt (string mapName, string key, int value)
        {
            bool r = GetMap(mapName).SetEnumInt(key, value); 

            if (r) NotifyChanged(mapName, key);

            return r;
        }
        public bool SetEnum<T> (string mapName, string key, T value) where T : Enum
        {
            bool r = GetMap(mapName).SetEnum(key, value);

            if (r) NotifyChanged(mapName, key);

            return r;
        }

        
        public SettingField GetField (string mapName, string key) => GetMap(mapName).GetSettingField(key);
        public Type GetFieldType(string mapName, string key) => GetMap(mapName).GetSettingFieldType(key);
        
        public bool SetField(string mapName, string key, object value)
        {
            bool result = GetMap(mapName).SetField(key, value);
            if (result) NotifyChanged(mapName, key);
            return result;
        }
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
                if (debugLogEmission)
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


            NotifyChanged(null, null);
        }
        #endregion

        #endregion
    }
}