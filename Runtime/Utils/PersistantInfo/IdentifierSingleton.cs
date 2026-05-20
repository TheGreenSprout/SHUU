using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.PersistantInfo
{
    [DefaultExecutionOrder(-20000)]
    public class IdentifierSingleton : MonoBehaviour
    {
        #region Variables
        public static List<IdentifierSingleton> allInstances = new List<IdentifierSingleton>();



        [Header("Singleton Settings")]
        public string identifier = "Singleton";


        [SerializeField] protected bool persistantSingleton = true;
        [SerializeField] protected bool handleGameobject = true;

        [SerializeField] protected UnityEvent onCreation = null;


        [Tooltip("If set  to 0 or more, after that ammount of scene changes, on the next scene change the object will be destroyed.")]
        public int bridges = -1;
        [Tooltip("These scenes won't cost a bridge to enter.")]
        [SerializeField] protected List<string> bridgeFree_Scenes = new List<string>() {"LoadingScene"};
        protected bool initialized = false;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField] protected List<string> banned_Scenes = new List<string>();



        private static bool debugLogEmission => SHUU_Preferences.instance.singleton_debugLogEmission;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (Check())
            {
                if (debugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] Identifier collision detected. Destroying newest instance...");
                Dispose();

                return;
            }


            allInstances.Add(this);

            if (persistantSingleton)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnSceneChanged;

            onCreation?.Invoke();
        }

        protected bool Check()
        {
            if (allInstances == null) return true;


            foreach (var singleton in allInstances)
            {
                if (singleton.identifier != identifier) continue;

                if (singleton.bridges == 0) return false;

                return true;
            }

            return false;
        }


        private void Dispose() => Destroy(handleGameobject ? gameObject : this);

        public void DestroySingleton()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnSceneChanged;

           allInstances.Remove(this);

            
            Dispose();
        }
        #endregion



        #region Logic
        protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (banned_Scenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                if (debugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] Banned scene entered. Destroying singleton...");

                DestroySingleton();

                return;
            }
        }

        protected void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            if (!initialized)
            {
                initialized = true;

                return;
            }

            
            if (bridges > -1)
            {
                if (bridges == 0)
                {
                    if (debugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] All bridges burnt. Destroying singleton...");

                    DestroySingleton();
                }
                
                if (!bridgeFree_Scenes.Contains(newScene.name)) bridges--;
            }
        }
        #endregion
    }
}
