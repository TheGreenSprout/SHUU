using UnityEngine;
using SHUU.Utils.SceneManagement;
using SHUU.Utils.Helpers;
using System.Collections.Generic;
//using SHUU.Utils.UI.Dialogue;

namespace SHUU.Utils.Globals
{

    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Script holding some static variables used by the package, must be in all scenes.
    /// </summary>
    #endregion
    public class SHUU_General : StaticInstance_Monobehaviour<SHUU_General>
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




        protected override void Awake()
        {
            base.Awake();



            transform.SetParent(null);


            SceneLoader.nextScene = null;
        }
        
        private void Start()
        {
            if (fadeInAtSceneRoomEnter) SHUU_Fades.CreateFade_In(new FadeOptions { duration = enterFadeIn_duration, start_delay = enterFadeIn_buffer });
        }


        #region XML doc
        /// <summary>
        /// Starts the process to travel to a different scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to go to.</param>
        /// <param name="fade">Whether to fade in when leaving the scene. If null, uses the default setting.</param>
        #endregion
        public static void GoToScene(string sceneName, bool? fade = null) => instance._GoToScene(sceneName, fade);
        private void _GoToScene(string sceneName, bool? fade = null)
        {
            if (fade == null) fade = fadeInAtSceneRoomLeave;


            if (fade.Value)
            {
                FadeOptions fadeOptions = new FadeOptions
                {
                    duration = exitFadeIn_duration,
                    end_delay = exitFadeInBuffer,
                    end_Action = () =>
                    {
                        SceneLoader.Load(sceneName);
                    },
                    clearOnEnd = false
                };

                SHUU_Fades.CreateFade_Out(fadeOptions);
            }
            else
            {
                SceneLoader.Load(sceneName);
            }


            SHUU_Saving.OnRoomChange();
        }

        public static void ReloadScene(bool? fade = null)
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
