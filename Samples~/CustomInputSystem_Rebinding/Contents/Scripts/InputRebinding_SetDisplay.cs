using System.Collections.Generic;
using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using TMPro;
using UnityEngine;

public class InputRebinding_SetDisplay : MonoBehaviour, InputRebinding_ISetDisplay
{
    [SerializeField] private TMP_Text label;


    [SerializeField] private InputRebinding_BindingButton bindingButton_prefab;

    [SerializeField] private RectTransform content;



    private List<InputRebinding_BindingButton> allButtons = new();

    private InputBindingMap map;
    private InputSet set;




    public void Init(InputBindingMap map, NAMED_InputSet set)
    {
        this.map = map;
        this.set = set.set;
        label.text = set.name;

        set.overrideAction += UpdateButtons;


        for (int i = 0; i < set.set.valid_keyBinds.Count; i++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, set.set, i+1);

            allButtons.Add(button);
        }

        for (int i = 0; i < set.set.valid_mouseBinds.Count; i++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, set.set, -(i+1));

            allButtons.Add(button);
        }
    }

    private void UpdateButtons()
    {
        //foreach (var button in allButtons) button.UpdateName();
        foreach (var button in allButtons) Destroy(button.gameObject);
        allButtons.Clear();


        for (int i = 0; i < set.valid_keyBinds.Count; i++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, set, i+1);

            allButtons.Add(button);
        }

        for (int i = 0; i < set.valid_mouseBinds.Count; i++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, set, -(i+1));

            allButtons.Add(button);
        }
    }
    public void UpdateButtons(NAMED_InputSet newSet)
    {
        if (!this.gameObject.activeInHierarchy) return;

        //foreach (var button in allButtons) button.UpdateName(newSet);
        set = newSet.set;
        
        UpdateButtons();
    }


    public void ChangeBinding()
    {
        string[] allBindings = new string[allButtons.Count];
        for (int i = 0; i < allButtons.Count; i++)
        {
            allBindings[i] = allButtons[i].label.text;
        }

        map.RebindInputSet(label.text, SHUU_Input.CreateDynamicInputArray(allBindings));


        UpdateButtons();
    }
}
