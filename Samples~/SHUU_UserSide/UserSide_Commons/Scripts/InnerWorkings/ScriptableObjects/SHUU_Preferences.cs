using UnityEngine;

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
                if (_instance == null) _instance = Resources.Load<SHUU_Preferences>("InnerWorkings/SHUU_Preferences");

                return _instance;
            }
        }
        #endregion
        
        #endregion




        #region Preferences

        #region Input System
        [Header("Input System")]
        public bool inputSystem_debugLogEmission = false;


        [SerializeField] private bool _inputSystem_mapDisabledWarning_debugLogEmission = false;
        public bool inputSystem_mapDisabledWarning_debugLogEmission => inputSystem_debugLogEmission && _inputSystem_mapDisabledWarning_debugLogEmission;
        #endregion



        #region Handy Classes
        [Header("Handy Classes")]
        public bool singleton_debugLogEmission = false;
        #endregion



        #region Scene Management
        [Header("Scene Loader")]
        public string sceneLoader_fallbackSceneName = "ErrorScene";
        public string sceneLoader_loadingSceneName = "LoadingScene";


        public bool sceneLoader_useLoadingScreenDefault = true;

        public bool sceneLoader_debugLogEmission = false;
        #endregion



        #region Saving
        [Header("Saving")]
        public bool saving_debugLogEmission = false;
        #endregion



        #region Random System
        [Header("Random System")]
        public bool randomSystem_debugLogEmission = false;
        #endregion



        #region UI
        [Header("UI")]
        public bool ui_debugLogEmission = false;
        #endregion

        
        #endregion



        #region Main
        protected virtual void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_Preferences)}.\nDestroying newest instance...");

                Destroy(this);
                return;
            }

            _instance = this;
        }
        #endregion
    }
}
