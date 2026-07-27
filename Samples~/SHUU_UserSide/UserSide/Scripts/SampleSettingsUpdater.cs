using UnityEngine;
using System;

using SHUU.Utils.SettingsSystem;
using SHUU.Utils.Helpers;

using static SHUU.Utils.Helpers.HandyFunctions;

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


    private static Resolution[] resolutions => Screen.resolutions;



    [SerializeField] private bool fetchResolutionsOnFirstStart = true;

    [SerializeField] private SettingsAtlas settings;
    [SerializeField] private string resolutionMapName;
    [SerializeField] private string resolutionIndexField;
    #endregion




    #region Main
    private void Start()
    {
        if (!fetchResolutionsOnFirstStart) return;

        if (PlayerPrefs.GetInt(playerPrefKey, 0) == 0)
        {
            PlayerPrefs.SetInt(playerPrefKey, 1);

            Resolution cur = Screen.currentResolution;
            int index = Array.FindIndex(resolutions, r => r.width == cur.width && r.height == cur.height);
            settings.SetInt(resolutionMapName, resolutionIndexField, Mathf.Max(0, index));
        }
    }
    #endregion



    #region Logic
    public void FullscreenMode_Update(SettingsAtlas data, string mapName, string field) => Screen.fullScreenMode = data.GetEnum<FullScreenMode>(mapName, field);

    public void ResolutionIndex_Update(SettingsAtlas data, string mapName, string field)
    {
        int index = data.GetInt(mapName, field);
        if (!resolutions.IndexIsValid(index)) return;

        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);
    }

    public void QualityIndex_Update(SettingsAtlas data, string mapName, string field) => QualitySettings.SetQualityLevel(data.GetInt(mapName, field));
    #endregion
}
