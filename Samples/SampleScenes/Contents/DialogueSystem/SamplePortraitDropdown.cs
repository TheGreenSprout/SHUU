using SHUU.Utils.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SamplePortraitDropdown : MonoBehaviour
{
    public SetSamplePortraits setSamplePortraits;



    private TMP_Dropdown myDropdown;


    public bool character = false;




    private void Start()
    {
        myDropdown = GetComponent<TMP_Dropdown>();


        // Listen for value changes
        myDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        setSamplePortraits.SetPortrait(character, index);
    }
}
