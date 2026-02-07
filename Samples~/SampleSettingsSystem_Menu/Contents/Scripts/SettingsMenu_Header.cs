using System;
using SHUU.Utils.SettingsSytem;
using TMPro;
using UnityEngine;

public class SettingsMenu_Header : MonoBehaviour
{
    [SerializeField] private TMP_Text label;



    private SettingsData data;


    private Action<SettingsData> onRestoreDefault;




    public void Init(SettingsData data, Action<SettingsData> callback)
    {
        this.data = data;

        label.text = data.settingsName;


        onRestoreDefault = callback;
    }

    public void Init(string name) => label.text = name;


    public void ResetToDefault()
    {
        data?.RestoreDefaults();

        onRestoreDefault?.Invoke(data);
    }
}
