using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Alchemy.Inspector;

using SHUU.Utils.SceneManagement;
using SHUU.InnerWorkings.Preferences;

namespace SHUU.Utils.PersistantInfo
{
    [DefaultExecutionOrder(-20000)]
    public class IdentifierSingleton : MonoBehaviour
    {
        #region Variables
        public static List<IdentifierSingleton> AllInstances = new List<IdentifierSingleton>();



        [BoxGroup("IdentifierSingleton Settings")]
        public string identifier = "Singleton";


        [SerializeField, BoxGroup("IdentifierSingleton Settings")]
        protected bool persistantSingleton = true;
        [SerializeField, BoxGroup("IdentifierSingleton Settings")]
        protected bool handleGameobject = true;

        [SerializeField, BoxGroup("IdentifierSingleton Settings")]
        protected UnityEvent onCreation = null;


        [Tooltip("If set  to 0 or more, after that ammount of scene changes, on the next scene change the object will be destroyed.")]
        [BoxGroup("IdentifierSingleton Settings")]
        public int bridges = -1;
        [Tooltip("These scenes won't cost a bridge to enter.")]
        [SerializeField, BoxGroup("IdentifierSingleton Settings")]
        protected List<string> bridgeFree_Scenes = new List<string>() {"LoadingScene"};
        protected bool initialized = false;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField, BoxGroup("IdentifierSingleton Settings")]
        protected List<string> banned_Scenes = new List<string>();



        private static bool DebugLogEmission => SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (Check())
            {
                if (DebugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] Identifier collision detected. Destroying newest instance...");
                Dispose();

                return;
            }


            AllInstances.Add(this);

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
            if (AllInstances == null) return true;


            foreach (var singleton in AllInstances)
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

           AllInstances.Remove(this);

            
            Dispose();
        }
        #endregion



        #region Logic
        protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (banned_Scenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                if (DebugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] Banned scene entered. Destroying singleton...");

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
                    if (DebugLogEmission) Debug.LogWarning($"[IdentifierSingleton Singleton] All bridges burnt. Destroying singleton...");

                    DestroySingleton();
                }
                
                if (!bridgeFree_Scenes.Contains(newScene.name)) bridges--;
            }
        }
        #endregion
    }
}
