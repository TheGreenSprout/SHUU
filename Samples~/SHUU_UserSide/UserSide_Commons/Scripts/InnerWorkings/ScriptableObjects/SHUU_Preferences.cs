using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects
{
    //[CreateAssetMenu(fileName = "SHUU_Preferences", menuName = "Scriptable Objects/SHUU_Preferences")]
    public class SHUU_Preferences : ScriptableObject
    {
        #region Variables

        #region Singleton
        private static SHUU_Preferences _instance;

        public static SHUU_Preferences instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<SHUU_Preferences>("SHUU/InnerWorkings/Preferences/SHUU_Preferences");

                return _instance;
            }
        }
        #endregion
        
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => _ = instance;


        protected virtual void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
#if UNITY_EDITOR
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_Preferences)}.\nRecorded instance: {AssetDatabase.GetAssetPath(_instance)}\nRepeated instance:{AssetDatabase.GetAssetPath(this)}\nDestroying newest instance...");
#else
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_Preferences)}.\nDestroying newest instance...");
#endif

                DestroyImmediate(this);
                return;
            }

            _instance = this;
        }
        #endregion



        #region Preferences

        #region Input System
        //[Header("Input System")]
        [SerializeField] public InputActionAsset inputSystem_actionAsset;


        public bool inputSystem_debugLogEmission = false;

        [SerializeField] private bool _inputSystem_mapDisabledWarning_debugLogEmission = false;
        public bool inputSystem_mapDisabledWarning_debugLogEmission => inputSystem_debugLogEmission && _inputSystem_mapDisabledWarning_debugLogEmission;
        #endregion



        #region Settings System
        //[Header("Settings System")]
        public bool settingsSystem_debugLogEmission = false;        
        #endregion



        #region Handy Classes
        //[Header("Handy Classes")]
        public bool singleton_debugLogEmission = false;
        #endregion



        #region Scene Loader
        //[Header("Scene Loader")]
        public string sceneLoader_fallbackSceneName = "ErrorScene";
        public string sceneLoader_loadingSceneName = "LoadingScene";


        public bool sceneLoader_useLoadingScreenDefault = true;

        public bool sceneLoader_debugLogEmission = false;
        #endregion



        #region Data Manager
        //[Header("Data Manager")]
        public bool dataManager_debugLogEmission = false;
        public bool dataManager_warningLogEmission = false;
        public bool dataManager_errorLogEmission = false;
        #endregion



        #region Saving
        //[Header("Saving")]
        public bool saving_debugLogEmission = false;
        #endregion



        #region Random System
        //[Header("Random System")]
        public bool randomSystem_debugLogEmission = false;
        #endregion



        #region UI
        //[Header("UI")]
        public bool ui_debugLogEmission = false;
        #endregion
        
        #endregion
    }
}
