using System;
using System.Collections.Generic;
using SHUU.Utils.InputSystem;
using TMPro;
using UnityEngine;

public class InputRebinding_AxisDisplay : MonoBehaviour, InputRebinding_ISetDisplay
{
    [SerializeField] private TMP_Text label;


    [SerializeField] private InputRebinding_BindingButton bindingButton_prefab;

    [SerializeField] private RectTransform content;



    private List<InputRebinding_BindingButton> allButtons = new();

    private Composite_InputSet compositeSet;

    private InputRebinding_CompositeDisplay compositeDisplay;
    private int index;




    public void Init(InputRebinding_CompositeDisplay compositeDisplay, Composite_InputSet set, int index)
    {
        this.compositeDisplay = compositeDisplay;
        compositeSet = set;
        this.index = index;

        bool direction = index > 0;
        int i = Math.Abs(index)-1;

        NAMED_InputSet s = direction ? set.axes[i].positiveSet : set.axes[i].negativeSet;

        label.text = direction ? s.name : s.name;


        List<KeyCode> keys = direction ? s.set.valid_keyBinds : s.set.valid_keyBinds;
        List<int> mouse = direction ? s.set.valid_mouseBinds : s.set.valid_mouseBinds;

        
        for (int j = 0; j < keys.Count; j++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, s.set, j+1);

            allButtons.Add(button);
        }

        for (int j = 0; j < mouse.Count; j++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, s.set, -(j+1));

            allButtons.Add(button);
        }
    }

    public void UpdateNames()
    {
        /*bool direction = index > 0;
        int i = Math.Abs(index)-1;

        NAMED_InputSet s = direction ? compositeSet.axes[i].positiveSet : compositeSet.axes[i].negativeSet;


        List<KeyCode> keys = direction ? s.set.valid_keyBinds : s.set.valid_keyBinds;
        List<int> mouse = direction ? s.set.valid_mouseBinds : s.set.valid_mouseBinds;

        
        int k = 0;
        for (int j = 0; j < keys.Count; j++)
        {
            allButtons[k].UpdateName(s.set, j+1);
            k++;
        }

        for (int j = 0; j < mouse.Count; j++)
        {
            allButtons[k].UpdateName(s.set, -(j+1));
            k++;
        }*/
        foreach (var button in allButtons) Destroy(button.gameObject);
        allButtons.Clear();


        bool direction = index > 0;
        int i = Math.Abs(index)-1;

        NAMED_InputSet s = direction ? compositeSet.axes[i].positiveSet : compositeSet.axes[i].negativeSet;

        label.text = direction ? s.name : s.name;


        List<KeyCode> keys = direction ? s.set.valid_keyBinds : s.set.valid_keyBinds;
        List<int> mouse = direction ? s.set.valid_mouseBinds : s.set.valid_mouseBinds;

        
        for (int j = 0; j < keys.Count; j++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, s.set, j+1);

            allButtons.Add(button);
        }

        for (int j = 0; j < mouse.Count; j++)
        {
            var button = Instantiate(bindingButton_prefab, content);
            button.Init(this, s.set, -(j+1));

            allButtons.Add(button);
        }
    }

    public void UpdateNames(NAMED_Composite_InputSet newSet)
    {
        if (!this.gameObject.activeInHierarchy) return;


        compositeSet = newSet.set;

        UpdateNames();
    }


    public void ChangeBinding() => compositeDisplay.ChangeBinding();

    public List<string> GetBindings()
    {
        bool direction = index > 0;

        string prefix = direction ? "+" : "-";
        
        List<string> output = new();
        foreach (var button in allButtons) output.Add(prefix + button.label.text);

        return output;
    }
}
