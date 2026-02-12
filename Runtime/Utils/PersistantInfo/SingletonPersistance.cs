using System.Collections.Generic;
using SHUU.Utils.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU.Utils.PersistantInfo
{
    [DefaultExecutionOrder(-20000)]
    #region XML doc
    /// <summary>
    /// Handles logic related to keeping track of all the Singleton persistance scripts, and manages them.
    /// </summary>
    #endregion
    public class SingletonPersistance : MonoBehaviour
    {
        public static List<SingletonPersistance> Singleton_Instance = new List<SingletonPersistance>();



        [SerializeField] private string identifier = "Singleton";
        public string IDENTIFIER => identifier;



        [SerializeField] private GameObject scripts;



        [Tooltip("If set  to 0 or more, after that ammount of scene changes, on the next scene change the object will be destroyed.")]
        public int bridges = -1;
        [Tooltip("These scenes won't cost a bridge to enter.")]
        [SerializeField] private List<string> bridgeFree_Scenes = new List<string>() {"LoadingScene"};
        private bool initialized = false;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField] private List<string> banned_Scenes = new List<string>();




        private void Awake()
        {
            if (Check())
            {
                Destroy(gameObject);

                return;
            }



            Singleton_Instance.Add(this);

            transform.parent = null;

            DontDestroyOnLoad(gameObject);


            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnSceneChanged;


            if (scripts != null) scripts.SetActive(true);
        }

        private bool Check()
        {
            if (Singleton_Instance == null) return true;


            foreach (var singleton in Singleton_Instance)
            {
                if (singleton.IDENTIFIER != IDENTIFIER) continue;

                if (singleton.bridges == 0) return false;

                return true;
            }

            return false;
        }


        public void DestroySingleton()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnSceneChanged;

           Singleton_Instance.Remove(this);

            
            Destroy(this.gameObject);
        }



        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (banned_Scenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                DestroySingleton();

                return;
            }
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            if (!initialized)
            {
                initialized = true;

                return;
            }

            
            if (bridges > -1)
            {
                if (bridges == 0) DestroySingleton();
                
                if (!bridgeFree_Scenes.Contains(newScene.name)) bridges--;
            }
        }
    }
}
