using UnityEngine;
using System;
using SHUU.Utils.SceneManagement;
using SHUU.Utils.PersistantInfo;
using SHUU.Utils.UI;
using SHUU.Utils.Helpers;
using System.Collections.Generic;
using SHUU.Utils.UI.Dialogue;

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


        public static List<GameObject> dialogueBox_PrefabList;
        [SerializeField] private List<GameObject> dialogueBox_PrefabList_StaticRef = new List<GameObject>();



        public static Canvas canvas;
        [SerializeField] private Canvas canvas_StaticRef;



        public static bool saveToFile_OnRoomExit;
        [SerializeField] private bool saveToFile_OnRoomExit_Ref = false;



        [SerializeField] private bool fadeInAtSceneRoomEnter = true;
        [SerializeField] private float enterFadeInBuffer = 0.05f;

        public static bool fadeInAtSceneRoomLeave;
        [SerializeField] private bool fadeInAtSceneRoomLeave_Ref = true;
        public static float exitFadeInBuffer;
        [SerializeField] private float exitFadeInBuffer_Ref = 0.05f;




        private void Awake()
        {
            transform.SetParent(null);


            SceneLoader.nextScene = null;


            TryGetComponent<ManageFades>(out manageFades);


            canvas = canvas_StaticRef;


            dialogueBox_PrefabList = new List<GameObject>();
            for (int i = 0; i < dialogueBox_PrefabList_StaticRef.Count; i++)
            {
                dialogueBox_PrefabList.Add(dialogueBox_PrefabList_StaticRef[i]);
            }


            saveToFile_OnRoomExit = saveToFile_OnRoomExit_Ref;


            fadeInAtSceneRoomLeave = fadeInAtSceneRoomLeave_Ref;
            exitFadeInBuffer = exitFadeInBuffer_Ref;
        }
        
        private void Start()
        {
            if (!fadeInAtSceneRoomEnter)
            {
                return;
            }

            
            GameObject fadeIn = manageFades.CreateFadeIn();

            Action action = () =>
            {
                manageFades.TriggerFadeIn(fadeIn, 1f);
            };

            SHUU_Timer.Create(enterFadeInBuffer, action);
        }


        #region XML doc
        /// <summary>
        /// Starts the process to travel to a different scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to go to.</param>
        #endregion
        public static void GoToScene(string sceneName, Color? fadeStarterColor = null, float? fadeDuration = null)
        {
            if (fadeInAtSceneRoomLeave)
            {
                manageFades.TriggerFadeOut(fadeStarterColor, fadeDuration, () =>
                {
                    SHUU_Timer.Create(exitFadeInBuffer, () =>
                    {
                        SceneLoader.Load(sceneName);
                    });
                });
            }
            else
            {
                SceneLoader.Load(sceneName);
            }


            if (saveToFile_OnRoomExit) SaveSingletonInfoToFile();
            else SaveSingletonInfo();
        }
        
        
        public static SHUU_DialogueBox CreateDialogue(DialogueInstance dialogueInstance, int index, Action endDialogueLogic = null, Transform dialoguBoxSpawnParent = null)
        {
            if (dialoguBoxSpawnParent == null) dialoguBoxSpawnParent = canvas.transform;
            
            
            
            if (!HandyFunctions.IndexIsValid(index, dialogueBox_PrefabList))
            {
                Debug.LogError("Dialogue prefab index is not valid for the current dialogue prefab list.");

                return null;
            }



            SHUU_DialogueBox dialogeBox = new SHUU_DialogueBox(dialogueBox_PrefabList[index], canvas.transform);


            dialogeBox.StartDialogue(dialogueInstance, endDialogueLogic);
            
            

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
