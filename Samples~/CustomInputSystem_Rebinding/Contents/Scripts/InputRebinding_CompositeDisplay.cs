using System.Collections.Generic;
using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using TMPro;
using UnityEngine;

public class InputRebinding_CompositeDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text label;


    [SerializeField] private InputRebinding_AxisDisplay axisDisplay_prefab;



    private List<InputRebinding_AxisDisplay> axisDisplays = new();

    private InputBindingMap map;




    public void Init(InputBindingMap map, NAMED_Composite_InputSet set, RectTransform content)
    {
        this.map = map;
        label.text = set.name;

        set.overrideAction += UpdateAxis;

        for (int i = 0; i < set.set.axes.Count; i++)
        {
            var ax = Instantiate(axisDisplay_prefab, content);
            ax.Init(this, set.set, i+1);
            axisDisplays.Add(ax);

            ax = Instantiate(axisDisplay_prefab, content);
            ax.Init(this, set.set, -(i+1));
            axisDisplays.Add(ax);
        }
    }

    private void UpdateAxis()
    {
        foreach (var axis in axisDisplays) axis.UpdateNames();
    }
    private void UpdateAxis(NAMED_Composite_InputSet newSet)
    {
        if (!this.gameObject.activeInHierarchy) return;

        foreach (var axis in axisDisplays) axis.UpdateNames(newSet);
    }


    public void ChangeBinding()
    {
        List<List<string>> allAxis = new();
        for (int i = 0; i < axisDisplays.Count; i++)
        {
            allAxis.Add(axisDisplays[i].GetBindings());
        }

        List<string> merge = new();
        foreach (var list in allAxis)
        {
            merge = (List<string>)HandyFunctions.MergeLists(merge, list);
        }

        map.RebindInputSet(label.text, SHUU_Input.CreateDynamicInputArray(merge.ToArray()));


        UpdateAxis();
    }
}
