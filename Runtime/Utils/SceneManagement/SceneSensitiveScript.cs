using UnityEngine;
using System.Collections.Generic;

namespace SHUU.Utils.SceneManagement
{

#region XML doc
/// <summary>
/// Makes it's child object only run in certain scenes.
/// </summary>
#endregion
public class SceneSensitiveScript : MonoBehaviour
{
    [SerializeField] protected List<string> excludedScenes;




    #region XML doc
    /// <summary>
    /// Checks if the script can run in this scene.
    /// </summary>
    /// <param name="sceneName">Name of the current scene.</param>
    /// <returns>Wether the script can run in this scene.</returns>
    #endregion
    protected virtual bool IsValidScene(string sceneName)
    {
        foreach (string name in excludedScenes)
        {
            if (sceneName == name)
            {
                return false;
            }
        }


        return true;
    }
}

}
