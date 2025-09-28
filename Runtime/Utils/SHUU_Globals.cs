using UnityEngine;
using System;
using SHUU.Utils.SceneManagement;
using SHUU.Utils.PersistantInfo;
using SHUU.Utils.UI;
using SHUU.Utils.Helpers;
using System.Collections.Generic;

namespace SHUU.Utils
{

#region XML doc
/// <summary>
/// Script holding some static variables used by the package, must be in all scenes.
/// </summary>
#endregion
public class SHUU_Globals : MonoBehaviour
{
    public static ManageFades manageFades;


    public static List<GameObject> dialoguePrefabList;
    [SerializeField] private List<GameObject> dialoguePrefabListRef = new List<GameObject>();



    public static Canvas canvas;
    [SerializeField] private Canvas canvasRef;
    
    
    
    [SerializeField] private bool fadeInAtSceneStart  = true;
    [SerializeField] private float startFadeInBuffer = 0.05f;




    private void Awake()
    {
        transform.SetParent(null);


        SceneLoader.nextScene = null;


        TryGetComponent<ManageFades>(out manageFades);


        canvas = canvasRef;


        dialoguePrefabList = new List<GameObject>();
        for (int i = 0; i < dialoguePrefabListRef.Count; i++)
        {
            dialoguePrefabList.Add(dialoguePrefabListRef[i]);
        }
    }
    
    private void Start()
    {
        if (!fadeInAtSceneStart)
        {
            return;
        }


        
        GameObject fadeIn = manageFades.CreateFadeIn();

        Action action = () => TriggerStartFade(fadeIn);
        SHUU_Timer.Create(startFadeInBuffer, action);
    }
    
    private void TriggerStartFade(GameObject fade)
    {
        manageFades.TriggerFadeIn(fade, 1f);
    }


    #region XML doc
        /// <summary>
        /// Starts the process to travel to a different scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to go to.</param>
    #endregion
    public static void GoToScene(string sceneName)
    {
        Action action = () => SceneLoader.Load(sceneName);


        manageFades.CreateFadeOut(action);



        SaveSingletonInfo();
    }
    
    
    /*public static DialogueManager CreateDialogue(DialogueInstance dialogueInstance, int index)
    {
        if (!HandyFunctions.IndexIsValid(index, dialoguePrefabList))
        {
            Debug.LogError("Dialogue prefab index is not valid for the current dialogue prefab list.");

            return null;
        }


        GameObject dialogue = Instantiate(dialoguePrefabList[index], canvas.transform);


        DialogueManager manageDialogue = dialogue.GetComponent<DialogueManager>();


        manageDialogue.CreateDialogue(dialogueInstance);
        
        

        return manageDialogue;
    }*/
    public static DialogueBox CreateDialogue(DialogueInstance dialogueInstance, int index, Action endDialogueLogic = null)
    {
        if (!HandyFunctions.IndexIsValid(index, dialoguePrefabList))
        {
            Debug.LogError("Dialogue prefab index is not valid for the current dialogue prefab list.");

            return null;
        }



        DialogueBox dialogeBox = new DialogueBox(dialoguePrefabList[index], canvas.transform);


        dialogeBox.CreateDialogue(dialogueInstance, endDialogueLogic);
        
        

        return dialogeBox;
    }
    
    
    #region XML doc
    /// <summary>
    /// Triggers all singletons to save their information.
    /// </summary>
    #endregion
    public static void SaveSingletonInfo()
    {
        Persistant_Globals.sigletonInfo.SaveAllSingletonInfo(SceneLoader.GetCurrentSceneName());
    }
    #region XML doc
    /// <summary>
    /// Saves all singleton info to a file.
    /// </summary>
    #endregion
    public static void SaveSingletonInfoToFile()
    {
        Persistant_Globals.saveFilesManager.SaveJsonInfo();
    }
    
    #region XML doc
    /// <summary>
    /// Triggers all singletons to load their information.
    /// </summary>
    #endregion
    public static void LoadSingletonInfo()
    {
        Persistant_Globals.sigletonInfo.LoadAllSingletonInfo(SceneLoader.GetCurrentSceneName());
    }
    #region XML doc
    /// <summary>
    /// Loads all singleton info from a file.
    /// </summary>
    /// <returns>Returns wether the load was successful.</returns>
    #endregion
    public static bool LoadSingletonInfoFromFile()
    {
        return Persistant_Globals.saveFilesManager.LoadJsonInfo();
    }
    
    #region XML doc
    /// <summary>
    /// Deletes a save file.
    /// </summary>
    /// <param name="-1">Index of the save file to delete.</param>
    #endregion
    public static void DeleteSaveInfo(int fileIndex = -1)
    {
        Persistant_Globals.saveFilesManager.DeleteSave(fileIndex);
    }
}

}
