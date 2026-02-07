using System;
using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using TMPro;
using UnityEngine;

public class InputRebinding_BindingButton : MonoBehaviour
{
    public TMP_Text label;



    private Action onClick = null;




    public void Init(InputRebinding_ISetDisplay setDisplay, InputSet set, int index)
    {
        onClick = setDisplay.ChangeBinding;

        label.text = set.validSources[index].String();
    }


    public void OnClick()
    {
        label.text = "...";

        StartCoroutine(HandyFunctions.ListenForInput_Enumerator(Callback));
    }

    private void Callback(string input)
    {
        if (input == InputParser.InputToString(KeyCode.Escape)) return;

        
        label.text = input;

        onClick?.Invoke();
    }
}
