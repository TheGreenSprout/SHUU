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
    }
}
