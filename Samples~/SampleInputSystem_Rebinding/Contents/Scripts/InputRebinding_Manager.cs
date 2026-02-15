using System.Collections.Generic;
using SHUU.Utils.InputSystem;
using UnityEngine;

public class InputRebinding_Manager : MonoBehaviour
{
    [Header("Maps")]
    [SerializeField] private List<InputBindingMap> inputBindingMaps;

    

    [Header("Prefabs")]
    [SerializeField] private InputRebinding_Header mapHeader_prefab;

    [SerializeField] private InputRebinding_SetDisplay setDisplay_prefab;
    [SerializeField] private InputRebinding_CompositeDisplay compositeDisplay_prefab;



    [Header("References")]
    [SerializeField] private RectTransform content;




    private void Awake()
    {
        foreach (var map in inputBindingMaps)
        {
            Instantiate(mapHeader_prefab, content).Init(map);

            foreach (var set in map.GetAllSingleSets()) Instantiate(setDisplay_prefab, content).Init(map, set);

            foreach (var set in map.GetAllCompositeSets()) Instantiate(compositeDisplay_prefab, content).Init(map, set, content);
        }
    }
}
