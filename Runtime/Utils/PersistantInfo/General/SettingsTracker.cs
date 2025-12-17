/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.IO;
using SHUU.UserSide.Commons;
using UnityEngine;

namespace SHUU.Utils.PersistantInfo.General
{
    public class SettingsTracker : MonoBehaviour
    {
        private static string filePath => Path.Combine(Application.persistentDataPath, "game_settings" + ".json");



        [SerializeField] private SettingsData settingsData;
        public static SettingsData settings;




        private void Awake()
        {
            settings = settingsData;


            Load();
        }


        private void OnApplicationQuit()
        {
            Save();
        }



        public void Save()
        {
            #if !UNITY_EDITOR
            if (settings == null)
            {
                Debug.LogWarning("GameSettings reference missing.");
                return;
            }

            string json = JsonUtility.ToJson(settings, true);

            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex}");
            }
            #endif
        }


        public void Load()
        {
            #if !UNITY_EDITOR
            if (settings == null)
            {
                Debug.LogWarning("GameSettings reference missing.");
                return;
            }

            if (!File.Exists(filePath))
                return;

            try
            {
                string json = File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(json, settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex}");
            }
            #endif
        }
    }
}
