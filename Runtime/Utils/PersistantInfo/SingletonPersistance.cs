using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.PersistantInfo
{

#region XML doc
/// <summary>
/// Handles logic related to keeping track of all the Singleton persistance scripts, and manages them.
/// </summary>
#endregion
public class SingletonPersistance : SceneSensitiveScript
{
    public static SingletonPersistance Singleton_Instance { get; private set; }



    private List<IfaceSingletonInfo> allSingletonInfoScrs;




    private void Awake()
    {
        if (Singleton_Instance != null && Singleton_Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Singleton_Instance = this;

        transform.parent = null;

        DontDestroyOnLoad(gameObject);



        // Get all singleton info scripts
        allSingletonInfoScrs = new List<IfaceSingletonInfo>(GetComponents<IfaceSingletonInfo>());


        Persistant_Globals.sigletonInfo = this;



        SceneManager.sceneLoaded += OnEverySceneLoaded;
    }


    private void OnDestroy()
    {
        // Avoids leaks
        SceneManager.sceneLoaded -= OnEverySceneLoaded;
    }


    #region XML doc
    /// <summary>
    /// Logic executed everytime a new scene is loaded.
    /// </summary>
    #endregion
    private void OnEverySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsValidScene(scene.name))
        {
            return;
        }



        LoadAllSingletonInfo(scene.name);
    }


    #region XML doc
    /// <summary>
    /// Triggers all Singletons persistance scripts to save their info. (Temporary info)
    /// </summary>
    /// <param name="sceneName">Name of the current scene.</param>
    #endregion
    public void SaveAllSingletonInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        //Debug.Log(allSingletonInfoScrs.Count);
        foreach (IfaceSingletonInfo singInfo in allSingletonInfoScrs)
        {
            singInfo.SaveInfo(sceneName);
        }
    }
    
    #region XML doc
    /// <summary>
    /// Triggers all Singletons persistance scripts to load their info. (Temporary info)
    /// </summary>
    /// <param name="sceneName">Name of the current scene.</param>
    #endregion
    public void LoadAllSingletonInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        //Debug.Log(allSingletonInfoScrs.Count);
        foreach (IfaceSingletonInfo singInfo in allSingletonInfoScrs)
        {
            singInfo.LoadInfo(sceneName);
        }
    }
}

}
