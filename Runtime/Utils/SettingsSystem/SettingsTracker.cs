using System;
using System.Collections.Generic;
using System.IO;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.SettingsSytem
{
    public class SettingsTracker : MonoBehaviour
    {
        public static SettingsTracker instance;

        private static string filePath =>
            Path.Combine(Application.persistentDataPath, "game_settings.json");

        public List<SettingsData> tracked_settingsData = new();

        private void Awake()
        {
            instance = this;

            Load();
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        public static SettingsData GetSettingsData(string name) => HandyFunctions.GetSettingsData(name);

        // ---------------- SAVE ----------------

        public void Save()
        {
#if !UNITY_EDITOR
            var saveFile = new SavedSettingsFile();

            foreach (var data in tracked_settingsData)
            {
                var savedData = new SavedSettingsData
                {
                    settingsName = data.settingsName
                };

                // Current values
                foreach (var field in data.fields)
                {
                    savedData.fields.Add(new SavedSettingField
                    {
                        key = field.key,
                        boolValue = field.boolValue,
                        intValue = field.intValue,
                        floatValue = field.floatValue,
                        stringValue = field.stringValue
                    });
                }

                // Default values
                foreach (var field in data.defaultFields)
                {
                    savedData.defaultFields.Add(new SavedSettingField
                    {
                        key = field.key,
                        boolValue = field.boolValue,
                        intValue = field.intValue,
                        floatValue = field.floatValue,
                        stringValue = field.stringValue
                    });
                }

                saveFile.settings.Add(savedData);
            }

            try
            {
                File.WriteAllText(
                    filePath,
                    JsonUtility.ToJson(saveFile, true)
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex}");
            }
#endif
        }

        // ---------------- LOAD ----------------

        public void Load()
        {
#if !UNITY_EDITOR
            if (!File.Exists(filePath))
                return;

            try
            {
                var json = File.ReadAllText(filePath);
                var saveFile = JsonUtility.FromJson<SavedSettingsFile>(json);

                foreach (var savedData in saveFile.settings)
                {
                    var targetData = tracked_settingsData
                        .Find(d => d.settingsName == savedData.settingsName);

                    if (targetData == null)
                        continue;

                    // Restore current values
                    ApplyFields(savedData.fields, targetData.fields);

                    // Restore defaults
                    ApplyFields(savedData.defaultFields, targetData.defaultFields);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex}");
            }
#endif
        }

        // ---------------- HELPERS ----------------

        void ApplyFields(
            List<SavedSettingField> saved,
            List<SettingField> target)
        {
            target.Clear();

            foreach (var savedField in saved)
            {
                var field = new SettingField
                {
                    key = savedField.key,
                    boolValue = savedField.boolValue,
                    intValue = savedField.intValue,
                    floatValue = savedField.floatValue,
                    stringValue = savedField.stringValue
                };

                target.Add(field);
            }
        }
    }

    [Serializable]
    public class SavedSettingsFile
    {
        public List<SavedSettingsData> settings = new();
    }

    [Serializable]
    public class SavedSettingsData
    {
        public string settingsName;

        public List<SavedSettingField> fields = new();
        public List<SavedSettingField> defaultFields = new();
    }

    [Serializable]
    public class SavedSettingField
    {
        public string key;

        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;
    }
}
