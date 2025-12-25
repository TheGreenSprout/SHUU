using System.Collections.Generic;
using SHUU.Utils.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU.Utils.PersistantInfo
{
    #region XML doc
    /// <summary>
    /// Handles logic related to keeping track of all the Singleton persistance scripts, and manages them.
    /// </summary>
    #endregion
    public class SingletonPersistance : MonoBehaviour
    {
        public static List<SingletonPersistance> Singleton_Instance { get; private set; }


        public string IDENTIFIER = "Singleton";



        [SerializeField] private GameObject scripts;



        [Tooltip("If set  to 0 or more, after that ammount of scene changes, on the next scene change the object will be destroyed.")]
        [SerializeField] private int bridges = -1;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField] private List<string> bannedScenes = new List<string>();




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


            scripts.SetActive(true);
        }

        private void Start() => SceneManager.sceneLoaded += OnSceneLoaded;


        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            Singleton_Instance = null;
        }


        private bool Check()
        {
            if (Singleton_Instance == null) Singleton_Instance = new List<SingletonPersistance>();


            foreach (SingletonPersistance singleton in Singleton_Instance)
            {
                if(this.IDENTIFIER == singleton.IDENTIFIER) return true;
            }

            return false;
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (bannedScenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                Destroy(gameObject);

                return;
            }


            if (bridges > -1)
            {
                if (bridges == 0) Destroy(gameObject);
                
                bridges--;
            }
        }
    }
}
