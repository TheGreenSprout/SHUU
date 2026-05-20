using System;
using TMPro;
using UnityEngine;

using SHUU.Utils.SettingsSytem;

public class SettingsMenu_Header : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_Text label;


    private SettingsData data;

    private Action<SettingsData> onRestoreDefault;
    #endregion




    #region Main
    public void Init(SettingsData data, Action<SettingsData> callback)
    {
        this.data = data;

        label.text = data.settingsName;


        onRestoreDefault = callback;
    }

    public void Init(string name) => label.text = name;
    #endregion



    #region Logic
    public void ResetToDefault()
    {
        data?.RestoreDefaults();

        onRestoreDefault?.Invoke(data);
    }
    #endregion
}
