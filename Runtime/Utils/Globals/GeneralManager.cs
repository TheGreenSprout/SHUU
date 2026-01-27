using UnityEngine;
using System;
using SHUU.Utils.SceneManagement;
using SHUU.Utils.PersistantInfo;
using SHUU.Utils.UI;
using SHUU.Utils.Helpers;
using System.Collections.Generic;
//using SHUU.Utils.UI.Dialogue;
using System.Collections;

namespace SHUU.Utils.Globals
{

    #region XML doc
    /// <summary>
    /// Script holding some static variables used by the package, must be in all scenes.
    /// </summary>
    #endregion
    public class GeneralManager : MonoBehaviour
    {
        [Header("Dialogue Settings")]
        [SerializeField] private List<GameObject> dialogueBox_PrefabList = new List<GameObject>();


        [SerializeField] private Canvas dialogueCanvas;



        [Header("Scene Transition Fade Settings")]
        [SerializeField] private bool fadeInAtSceneRoomEnter = true;

        [SerializeField] private float enterFadeIn_buffer = 0.05f;
        [SerializeField] private float enterFadeIn_duration = 1f;


        [SerializeField] private bool fadeInAtSceneRoomLeave = true;

        [SerializeField] private float exitFadeInBuffer = 0.05f;
        [SerializeField] private float exitFadeIn_duration = 1f;




        private void Awake()
        {
            SHUU_GlobalsProxy.generalManager = this;



            transform.SetParent(null);


            SceneLoader.nextScene = null;
        }
        
        private void Start()
        {
            if (fadeInAtSceneRoomEnter)
            {
                SHUU_GlobalsProxy.fadeManager.CreateFade_In(new FadeManager.FadeOptions { duration = enterFadeIn_duration, start_delay = enterFadeIn_buffer });
            }
        }


        #region XML doc
        /// <summary>
        /// Starts the process to travel to a different scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to go to.</param>
        /// <param name="fade">Whether to fade in when leaving the scene. If null, uses the default setting.</param>
        #endregion
        public void GoToScene(string sceneName, bool? fade = null)
        {
            if (fade == null) fade = fadeInAtSceneRoomLeave;


            if (fade.Value)
            {
                FadeManager.FadeOptions fadeOptions = new FadeManager.FadeOptions
                {
                    duration = exitFadeIn_duration,
                    end_delay = exitFadeInBuffer,
                    end_Action = () =>
                    {
                        SceneLoader.Load(sceneName);
                    },
                    clearOnEnd = false
                };

                SHUU_GlobalsProxy.fadeManager.CreateFade_Out(fadeOptions);
            }
            else
            {
                SceneLoader.Load(sceneName);
            }


            SHUU_GlobalsProxy.savingSystemManager.OnRoomChange();
        }

        public void ReloadScene(bool? fade = null)
        {
            GoToScene(SceneLoader.GetCurrentSceneName(), fade);
        }
        
        
        /*public SHUU_DialogueBox CreateDialogue(DialogueInstance dialogueInstance, int index, Action endDialogueLogic = null, Transform dialoguBoxSpawnParent = null)
        {
            if (dialoguBoxSpawnParent == null) dialoguBoxSpawnParent = dialogueCanvas.transform;
            
            
            
            if (!dialogueBox_PrefabList.IndexIsValid(index))
            {
                Debug.LogError("Dialogue prefab index is not valid for the current dialogue prefab list.");

                return null;
            }



            SHUU_DialogueBox dialogeBox = new SHUU_DialogueBox(dialogueBox_PrefabList[index], dialoguBoxSpawnParent);


            //dialogeBox.StartDialogue(dialogueInstance, endDialogueLogic);
            
            

            return dialogeBox;
        }*/
    }

}
