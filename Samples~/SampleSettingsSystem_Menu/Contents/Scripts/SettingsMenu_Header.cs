using System;
using TMPro;
using UnityEngine;

public class SettingsMenu_Header : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_Text label;

    [SerializeField] private GameObject restoreButton;


    private Action onRestore;
    #endregion




    #region Main
    // onRestore = null → hides the restore button
    public void Init(string name, Action onRestore = null)
    {
        label.text = name;
        this.onRestore = onRestore;

        if (restoreButton != null) restoreButton.SetActive(onRestore != null);
    }
    #endregion




    #region Logic
    public void ResetToDefault() => onRestore?.Invoke();
    #endregion
}
