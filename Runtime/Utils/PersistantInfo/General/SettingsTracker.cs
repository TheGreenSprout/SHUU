/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.IO;
using SHUU.UserSide;
using UnityEngine;

namespace SHUU.Utils.PersistantInfo.General
{
    public class SettingsTracker : MonoBehaviour
    {
        private static string settingsPath => Path.Combine(Application.persistentDataPath, "game_settings" + ".json");



        [SerializeField] private SettingsData settingsData;
        public static SettingsData settings;




        private void Awake()
        {
            settings = settingsData;


            Load();
        }


        private void OnDestroy()
        {
            Save();
        }



        public void Save()
        {
            if (settings == null)
            {
                Debug.LogWarning("GameSettings reference missing.");
                return;
            }

            string json = JsonUtility.ToJson(settings, true);

            try
            {
                File.WriteAllText(settingsPath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex}");
            }
        }


        public void Load()
        {
            if (settings == null)
            {
                Debug.LogWarning("GameSettings reference missing.");
                return;
            }

            if (!File.Exists(settingsPath))
                return;

            try
            {
                string json = File.ReadAllText(settingsPath);
                JsonUtility.FromJsonOverwrite(json, settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex}");
            }
        }
    }
}
