using System.Collections.Generic;
using SHUU.Utils.PersistantInfo;
using UnityEngine;

#region XML doc
/// <summary>
/// Example of how to create your own info singletons.
/// </summary>
#endregion
public class ExampleSingleton : SingletonInfo
{
    [SerializeField] private Dictionary<int, bool> colorOfItems = new Dictionary<int, bool>();




    #region Save info
    #region XML doc
    /// <summary>
    /// Saves info (temporary).
    /// </summary>
    /// <param name="sceneName">Current scene name.</param>
    #endregion
    public override void SaveInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        colorOfItems = new Dictionary<int, bool>();


        InteractablesManager interactablesManager = GameObject.FindGameObjectWithTag("Player").GetComponent<InteractablesManager>();

        for (int i = 0; i < interactablesManager.itemList.Count; i++)
        {
            colorOfItems.Add(i, interactablesManager.itemList[i].GetBooleanFromCurrentMaterial());
        }
    }
    #endregion


    #region Load info
    #region XML doc
    /// <summary>
    /// Loads info (temporary).
    /// </summary>
    /// <param name="sceneName">Current scene name.</param>
    #endregion
    public override void LoadInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        InteractablesManager interactablesManager = GameObject.FindGameObjectWithTag("Player").GetComponent<InteractablesManager>();

        foreach (KeyValuePair<int, bool> info in colorOfItems)
        {
            interactablesManager.itemList[info.Key].SetMaterialFromBool(info.Value);
        }
    }
    #endregion



    #region DTO import/export
    #region XML doc
    /// <summary>
    /// Exports info to DTO (DTO is contained in the MasterDTO, that will then get serialized to a save file).
    /// </summary>
    /// <param name="masterDTO">MasterDTO reference.</param>
    #endregion
    public override void ExportDTO(ref MasterDTO masterDTO)
    {
        masterDTO.exampleData = new DTO_ExampleInfo
        {
            colorOfItems = colorOfItems
        };
    }


    #region XML doc
    /// <summary>
    /// Imports info from DTO contained in the MasterDTO.
    /// </summary>
    /// <param name="dto">DTO that corresponds to this singleton.</param>
    #endregion
    public override void ImportDTO(DTO_InfoClass dto)
    {
        DTO_ExampleInfo exampleDTO = (DTO_ExampleInfo)dto;


        colorOfItems = exampleDTO.colorOfItems;
    }
    #endregion
}
