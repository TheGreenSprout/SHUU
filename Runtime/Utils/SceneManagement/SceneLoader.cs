using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU.Utils.SceneManagement
{

#region XML doc
/// <summary>
/// Manages loading scenes when moving between them.
/// </summary>
#endregion
public static class SceneLoader
{
    public static string nextScene = "";




    #region XML doc
    /// <summary>
    /// Get the current scene's name.
    /// </summary>
    /// <returns>The current scene's name.</returns>
    #endregion
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }


    #region XML doc
    /// <summary>
    /// Loads a scene.
    /// </summary>
    /// <param name="null">The name of the scene to load.</param>
    #endregion
    public static void Load(string sceneName = null)
    {
        if (sceneName == null || sceneName == "")
        {
            sceneName = "ErrorScene";
        }



        nextScene = sceneName;

        if (SceneExists(sceneName))
        {
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            Debug.LogError("Failed to load scene: " + sceneName + "\nTry adding the scene to the Scene List or writing the name of the scene correctly.");
        }
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
            if (scene == sceneName)
                return true;
        }
        return false;
    }
}

}
