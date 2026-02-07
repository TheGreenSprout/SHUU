using System.Collections.Generic;
using SHUU.Utils.SettingsSytem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu_Manager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<SettingsData> settingsData;

    

    [Header("Prefabs")]
    [SerializeField] private SettingsMenu_Header dataHeader_prefab;

    [SerializeField] private SettingsMenu_Field fieldDisplay_prefab;



    [Header("References")]
    [SerializeField] private RectTransform content;



    private Dictionary<string, List<SettingsMenu_Field>> fields = new();




    private void Awake()
    {
        foreach (SettingsData data in settingsData)
        {
            fields.Add(data.settingsName, new());

            Instantiate(dataHeader_prefab, content).Init(data, OnRestoreDefault);

            foreach (var field in data.fields)
            {
                var f = Instantiate(fieldDisplay_prefab, content);
                f.Init(data, field.key);

                fields[data.settingsName].Add(f);
            }
        }
    }


    private void OnRestoreDefault(SettingsData data)
    {
        if (!fields.ContainsKey(data.settingsName)) return;


        foreach (var field in fields[data.settingsName])
        {
            field.Refresh();
        }
    }
}
