using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.SettingsSystem;

public class SettingsMenu_Manager : MonoBehaviour
{
    #region Variables
    [Header("Data")]
    [SerializeField] private List<SettingsAtlas> settingsData;



    [Header("Prefabs")]
    [SerializeField] private SettingsMenu_Tab    tab_prefab;
    [SerializeField] private SettingsMenu_Header header_prefab;
    [SerializeField] private SettingsMenu_Field  field_prefab;



    [Header("References")]
    [SerializeField] private Transform tabBar;

    // The Content child of a ScrollRect — panels are spawned here
    [SerializeField] private Transform panelParent;



    private readonly List<SettingsMenu_Tab>         tabs        = new();
    private readonly List<GameObject>               panels      = new();
    private readonly List<List<SettingsMenu_Field>> fieldsByTab = new();

    private int activeTab = -1;
    #endregion




    #region Main
    private void Awake()
    {
        foreach (var data in settingsData)
        {
            foreach (var map in data.maps)
            {
                int index = tabs.Count;

                var tab = Instantiate(tab_prefab, tabBar);
                tab.Init(map.mapName, () => SwitchToTab(index));
                tabs.Add(tab);

                var panel  = CreatePanel(map.mapName);
                var fields = new List<SettingsMenu_Field>();
                fieldsByTab.Add(fields);

                // Map header with per-map restore
                Instantiate(header_prefab, panel).Init(map.mapName, () =>
                {
                    map.RestoreDefaults();
                    data.NotifyChanged(map.mapName, null);
                    foreach (var f in fields) f.Refresh();
                });

                // Ungrouped fields first
                foreach (var field in map.fields)
                    SpawnField(data, map.mapName, field.key, panel, fields);

                // Group headers + their fields
                foreach (var group in map.groups)
                {
                    Instantiate(header_prefab, panel).Init(group.groupName);

                    foreach (var field in group.fields)
                        SpawnField(data, map.mapName, field.key, panel, fields);
                }

                panel.gameObject.SetActive(false);
                panels.Add(panel.gameObject);
            }
        }

        if (tabs.Count > 0) SwitchToTab(0);
    }
    #endregion




    #region Logic
    public void SwitchToTab(int index)
    {
        if (index < 0 || index >= tabs.Count || index == activeTab) return;

        if (activeTab >= 0)
        {
            panels[activeTab].SetActive(false);
            tabs[activeTab].SetSelected(false);
        }

        activeTab = index;
        panels[activeTab].SetActive(true);
        tabs[activeTab].SetSelected(true);
    }


    private void SpawnField(SettingsAtlas data, string mapName, string key, Transform parent, List<SettingsMenu_Field> fields)
    {
        var f = Instantiate(field_prefab, parent);
        f.Init(data, mapName, key);
        fields.Add(f);
    }

    // Creates a panel inside panelParent that auto-sizes vertically to its children
    private Transform CreatePanel(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(panelParent, false);

        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment         = TextAnchor.UpperLeft;

        var csf = go.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return go.transform;
    }
    #endregion
}
