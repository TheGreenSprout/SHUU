using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu_Tab : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button   button;
    #endregion




    #region Main
    public void Init(string name, Action onClick)
    {
        label.text = name;
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    // Visually marks this tab as the selected one (button non-interactable = selected)
    public void SetSelected(bool selected)
    {
        if (button != null) button.interactable = !selected;
    }
    #endregion
}
