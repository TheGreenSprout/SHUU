using System;
using System.Collections.Generic;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.SettingsSytem
{
    [CreateAssetMenu(fileName = "SettingsData", menuName = "SHUU/SettingsData")]
    public class SettingsData : ScriptableObject
    {
        public string settingsName;


        public List<SettingField> fields = new();


        public string lastDefaultSetDateTime = "No Default Set";
        public List<SettingField> defaultFields = new();


        public event Action<string> OnSettingsChanged;
        public void NotifyChanged(string field) => OnSettingsChanged?.Invoke(field);




        #region Defaults
        public void RestoreDefaults()
        {
            fields.CopyFrom_List_CopyContructors(defaultFields, x => new SettingField(x));

            NotifyChanged(null);
        }

        public void SaveAsDefaults()
        {
            defaultFields.CopyFrom_List_CopyContructors(fields, x => new SettingField(x));

            lastDefaultSetDateTime = Stats.timestamp;
        }
        #endregion



        #region Getters
        public bool GetBool(string key) => fields.Find(x => x.key == key)?.boolValue ?? false;
        public int GetInt(string key) => fields.Find(x => x.key == key)?.intValue ?? 0;
        public float GetFloat(string key) => fields.Find(x => x.key == key)?.floatValue ?? 0f;
        public string GetString(string key) => fields.Find(x => x.key == key)?.stringValue ?? "";
        #endregion


        #region Setters
        public bool SetField(string key, object value)
        {
            var field = fields.Find(x => x.key == key);
            if (field == null) return false;

            switch (field.type)
            {
                case SettingType.Bool when value is bool b:
                    return SetBool(key, b);

                case SettingType.Int when value is int i:
                    return SetInt(key, i);

                case SettingType.Float when value is float f:
                    return SetFloat(key, f);

                case SettingType.String when value is string s:
                    return SetString(key, s);
            }

            return false;
        }

        public bool SetBool(string key, bool value)
        {
            var field = fields.Find(x => x.key == key && x.type == SettingType.Bool);
            if (field == null) return false;

            field.boolValue = value;
            NotifyChanged(key);
            return true;
        }
        public bool SetInt(string key, int value)
        {
            var field = fields.Find(x => x.key == key && x.type == SettingType.Int);
            if (field == null) return false;

            if (field.useMin) value = Mathf.Max(value, field.intMin);
            if (field.useMax) value = Mathf.Min(value, field.intMax);

            field.intValue = value;
            NotifyChanged(key);
            return true;
        }
        public bool SetFloat(string key, float value)
        {
            var field = fields.Find(x => x.key == key && x.type == SettingType.Float);
            if (field == null) return false;

            if (field.useMin) value = Mathf.Max(value, field.floatMin);
            if (field.useMax) value = Mathf.Min(value, field.floatMax);

            field.floatValue = value;
            NotifyChanged(key);
            return true;
        }
        public bool SetString(string key, string value)
        {
            var field = fields.Find(x => x.key == key && x.type == SettingType.String);
            if (field == null) return false;

            field.stringValue = value;
            NotifyChanged(key);
            return true;
        }
        #endregion
    }
}
