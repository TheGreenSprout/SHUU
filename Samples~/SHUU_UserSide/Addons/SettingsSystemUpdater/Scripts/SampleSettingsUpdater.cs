using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;

using Alchemy.Inspector;

using SHUU.Utils.SettingsSystem;
using SHUU.Utils.Helpers;
using SHUU.Utils.Developer.Console;

using static SHUU.Utils.Helpers.HandyFunctions;
using SHUU.Utils.Developer.Debugging.Systems;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;

namespace SHUU.UserSide.Addons.SettingsSystemUpdater
{
    public class SampleSettingsUpdater : MonoBehaviour
    {
        #region Enum
        public enum QualityLevel
        {
            Low,
            Medium,
            High,
            Ultra
        }
        #endregion




        #region Variables
        private const string playerPrefKey = "SampleSettingsUpdater_InstantiatedToggle";



        [SerializeField] private SettingsAtlas settings;

        
        #region Audio
        [Title("Audio")]
        [SerializeField] private bool fetchVolumeOnFirstStart = true;

        [SerializeField] private string volumeMapName;

        [SerializeField] private AudioMixer audioMixer;
        
        private Dictionary<string, string> volumeFieldsDict = new();
        [SerializeField] private List<VolumeField> volumeFields = new();
        [Serializable]
        private class VolumeField
        {
            public string settingsFieldName;
            public string mixerExposedParameterName;
        }

        private const float MIN_DB = -80f;
        #endregion


        #region Resolution
        [Title("Resolution")]
        [SerializeField] private bool fetchResolutionsOnFirstStart = true;

        [SerializeField] private string resolutionMapName;
        [SerializeField] private string resolutionIndexField;

        private static Resolution[] Resolutions => Screen.resolutions;
        #endregion


        #region Developer
        [Title("Developer")]
        [SerializeField] private DevConsole_Options devConsole_Options;
        #endregion

        #endregion




        #region Main
        private void Start()
        {
            // Audio
            foreach (var field in volumeFields)
            {
                volumeFieldsDict[field.settingsFieldName] = field.mixerExposedParameterName;

                if (fetchVolumeOnFirstStart) Volume_Update(settings, volumeMapName, field.settingsFieldName);
            }


            // Resolutions
            if (fetchResolutionsOnFirstStart)
            {
                if (PlayerPrefs.GetInt(playerPrefKey, 0) == 0)
                {
                    PlayerPrefs.SetInt(playerPrefKey, 1);

                    Resolution cur = Screen.currentResolution;
                    int index = Array.FindIndex(Resolutions, r => r.width == cur.width && r.height == cur.height);
                    settings.SetInt(resolutionMapName, resolutionIndexField, Mathf.Max(0, index));
                }
            }
        }
        #endregion



        #region Logic

        #region Audio
        public void Volume_Update(SettingsAtlas data, string mapName, string field)
        {
            if (volumeFieldsDict.ContainsKey(field)) audioMixer.SetFloat(volumeFieldsDict[field], VolumePercentage_ToDB(data.GetFloat(mapName, field)));
        }
        #endregion


        #region Video/Quality
        public void FullscreenMode_Update(SettingsAtlas data, string mapName, string field) => Screen.fullScreenMode = data.GetEnum<FullScreenMode>(mapName, field);

        public void ResolutionIndex_Update(SettingsAtlas data, string mapName, string field)
        {
            int index = data.GetInt(mapName, field);
            if (!Resolutions.IndexIsValid(index)) return;

            Resolution r = Resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
        }

        public void QualityIndex_Update(SettingsAtlas data, string mapName, string field) => QualitySettings.SetQualityLevel(data.GetInt(mapName, field));
        #endregion


        #region Developer
        public void ConsoleEnabled_Update(SettingsAtlas data, string mapName, string field) => devConsole_Options.canToggle_devConsole = data.GetBool(mapName, field);
        public void ConsoleScroll_Update(SettingsAtlas data, string mapName, string field)
            => SHUU_Time.OnNextFrame += () => DevConsoleManager.Instance.devConsoleUI.scrollSensitivity = data.GetFloat(mapName, field);

        public void InfoMenuEnabled_Update(SettingsAtlas data, string mapName, string field) => SHUU_Debug.Instance.debugInfo_enabled = data.GetBool(mapName, field);

        public void ScreenLogsEnabled_Update(SettingsAtlas data, string mapName, string field) => SHUU_Debug.Instance.screenLogs_enabled = data.GetBool(mapName, field);
        #endregion

        #endregion
    }
}