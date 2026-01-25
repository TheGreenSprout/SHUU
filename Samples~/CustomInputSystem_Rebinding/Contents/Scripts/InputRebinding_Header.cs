using SHUU.Utils.InputSystem;
using TMPro;
using UnityEngine;

public class InputRebinding_Header : MonoBehaviour
{
    [SerializeField] private TMP_Text label;



    private InputBindingMap map;




    public void Init(InputBindingMap map)
    {
        this.map = map;

        label.text = map.name;
    }

    public void Init(string name) => label.text = name;


    public void ResetToDefault() => map?.ResetToDefault();
}
