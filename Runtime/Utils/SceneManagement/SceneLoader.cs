using UnityEngine;
using UnityEngine.SceneManagement;
using System;

using SHUU.InnerWorkings.Preferences;
using SHUU.Utils.Globals;

namespace SHUU.Utils.SceneManagement
{
    #region XML doc
    /// <summary>
    /// Manages loading scenes when moving between them.
    /// </summary>
    #endregion
    public static class SceneLoader
    {
        #region Variables
        #region XML doc
        /// <summary>
        /// Invoked right when a scene load is requested, before anything actually starts loading.
        /// </summary>
        #endregion
        public static event Action<string> OnSceneLoadRequested;

        #region XML doc
        /// <summary>
        /// Invoked when Unity's SceneManager.sceneLoaded fires — after Awake and Start
        /// have run on all objects in the newly loaded scene, but before their first Update.
        /// </summary>
        #endregion
        public static event Action<Scene, LoadSceneMode> OnSceneLoaded;

        #region XML doc
        /// <summary>
        /// Invoked one frame after OnSceneLoaded — after the first Update has run on all
        /// objects in the newly loaded scene. Use this if you need to wait for anything
        /// kicked off in Start (e.g. coroutines) to have had a chance to begin.
        /// </summary>
        #endregion
        public static event Action<Scene, LoadSceneMode> OnSceneLoadedDelayed;


        public static string NextScene = "";



        // Internal
        private static bool Initialized;


        public static string FallbackSceneName => SHUUPreferences_SceneLoader.Instance?.fallbackSceneName;
        private static string LoadingSceneName => SHUUPreferences_SceneLoader.Instance?.loadingSceneName;

        private static bool UseLoadingScreen => SHUUPreferences_SceneLoader.Instance != null && SHUUPreferences_SceneLoader.Instance.useLoadingScreenDefault;
        public static bool DebugLogEmission => SHUUPreferences_SceneLoader.Instance != null && SHUUPreferences_SceneLoader.Instance.debugLogEmission;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            if (Initialized) return;
            Initialized = true;

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }


        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoaded?.Invoke(scene, mode);

            SHUU_Time.OnNextFrame += () => OnSceneLoadedDelayed?.Invoke(scene, mode);
        }
        #endregion



        #region Logic
        public static Scene GetCurrentScene() => SceneManager.GetActiveScene();

        #region XML doc
        /// <summary>
        /// Get the current scene's name.
        /// </summary>
        /// <returns>The current scene's name.</returns>
        #endregion
        public static string GetCurrentSceneName() => GetCurrentScene().name;


        #region XML doc
        /// <summary>
        /// Loads a scene.
        /// </summary>
        /// <param name="null">The name of the scene to load.</param>
        #endregion
        public static void Load(string sceneName) => Load(sceneName, UseLoadingScreen);
        public static void Load(string sceneName, bool useLoadingScreen)
        {
            if (sceneName == null || sceneName == "") sceneName = FallbackSceneName;

            OnSceneLoadRequested?.Invoke(sceneName);

            if (!useLoadingScreen)
            {
                if (SceneExists(sceneName)) SceneManager.LoadScene(sceneName);
                else if (DebugLogEmission)
                    Debug.LogError("Failed to load scene: " + sceneName + "\nTry adding the scene to the Scene List or writing the name of the scene correctly.");

                return;
            }


            NextScene = sceneName;

            if (SceneExists(sceneName)) SceneManager.LoadScene(LoadingSceneName);
            else if (DebugLogEmission)
                Debug.LogError("Failed to load scene: " + sceneName + "\nTry adding the scene to the Scene List or writing the name of the scene correctly.");
        }
        
        #region XML doc
        /// <summary>
        /// Checks if a scene exists.
        /// </summary>
        /// <param name="sceneName">The name of the scene to check.</param>
        /// <returns>Wether the scene exists.</returns>
        #endregion
        public static bool SceneExists(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string scene = System.IO.Path.GetFileNameWithoutExtension(path);
                if (scene == sceneName) return true;
            }
            return false;
        }
        #endregion
    }
}
