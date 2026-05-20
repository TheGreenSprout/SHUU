using UnityEngine;
using UnityEngine.SceneManagement;

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

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
        public static string nextScene = "";



        private static bool useLoadingScreen => SHUU_Preferences.instance.sceneLoader_useLoadingScreenDefault;

        private static bool debugLogEmission => SHUU_Preferences.instance.sceneLoader_debugLogEmission;
        #endregion




        #region Logic
        #region XML doc
        /// <summary>
        /// Get the current scene's name.
        /// </summary>
        /// <returns>The current scene's name.</returns>
        #endregion
        public static string GetCurrentSceneName() => SceneManager.GetActiveScene().name;


        #region XML doc
        /// <summary>
        /// Loads a scene.
        /// </summary>
        /// <param name="null">The name of the scene to load.</param>
        #endregion
        public static void Load(string sceneName = null) => Load(sceneName, !useLoadingScreen);
        public static void Load(string sceneName = null, bool useLoadingScreen = true)
        {
            if (sceneName == null || sceneName == "") sceneName = "ErrorScene";

            if (!useLoadingScreen)
            {
                if (SceneExists(sceneName)) SceneManager.LoadScene(sceneName);
                else if (debugLogEmission)
                    Debug.LogError("Failed to load scene: " + sceneName + "\nTry adding the scene to the Scene List or writing the name of the scene correctly.");

                return;
            }


            nextScene = sceneName;

            if (SceneExists(sceneName)) SceneManager.LoadScene("LoadingScene");
            else if (debugLogEmission)
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
