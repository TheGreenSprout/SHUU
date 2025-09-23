using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if SHUU_SAVE_DEPENDENCY
using Newtonsoft.Json;
#endif
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.PersistantInfo.SavingLoading
{

[RequireComponent(typeof(SingletonPersistance))]
#region XML doc
/// <summary>
/// Script that manages all save files.
/// </summary>
#endregion
public class SaveFilesManager : MonoBehaviour
{
#if SHUU_SAVE_DEPENDENCY
    [SerializeField] private List<string> saveFiles;


    [SerializeField] private bool autoSaveWhenClosing;



    public static int currentSaveFileIndex = 0;
    
    
    
    
    private void Awake()
    {
        for (int i = 0; i < saveFiles.Count; i++)
        {
            saveFiles[i] = Application.persistentDataPath + "/" + saveFiles[i];
        }
        
        
        
        Persistant_Globals.saveFilesManager = this;
    }



    void OnApplicationQuit()
    {
        //PlayerPrefs.Save();
        
        
        if (autoSaveWhenClosing)
        {
            SaveJsonInfo();
        }
    }



    #region XML doc
    /// <summary>
    /// Loads all saved info from save file.
    /// </summary>
    /// <param name="-1">Index of the save file to load info from.</param>
    /// <returns>Returns whether the loading of info was successful.</returns>
    #endregion
    public bool LoadJsonInfo(int loadIndex = -1)
    {
        if (loadIndex < 0)
        {
            loadIndex = currentSaveFileIndex;
        }
        else
        {
            currentSaveFileIndex = loadIndex;
        }
        
        if (!File.Exists(saveFiles[loadIndex]))
        {
            return false;
        }



        //Deserialize

        // ── ENABLE POLYMORPHIC DESERIALIZATION ──
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        string json = File.ReadAllText(saveFiles[loadIndex]);
        //MasterDTO masterDTO = JsonUtility.FromJson<MasterDTO>(json);
        MasterDTO masterDTO = JsonConvert.DeserializeObject<MasterDTO>(json, settings);



        #region Get (and load) all your DTO sinletons.
        ExampleSingleton exampleSingleton = GetComponent<ExampleSingleton>();


        exampleSingleton.ImportDTO(masterDTO.exampleData);
        #endregion
        
        
        
        return true;
    }


    #region XML doc
    /// <summary>
    /// Saves all saved info to save file.
    /// </summary>
    /// <param name="-1">Index of the save file to save info to.</param>
    #endregion
    public void SaveJsonInfo(int saveIndex = -1)
    {
        if (saveIndex < 0)
        {
            saveIndex = currentSaveFileIndex;
        }
        else
        {
            currentSaveFileIndex = saveIndex;
        }
        
        

        MasterDTO masterDTO = new MasterDTO();


        SingletonPersistance singletonPersistance = GetComponent<SingletonPersistance>();
        singletonPersistance.SaveAllSingletonInfo(SceneLoader.GetCurrentSceneName());
        
        
        #region  Get (and save) all your DTO sinletons.
        ExampleSingleton exampleSingleton = GetComponent<ExampleSingleton>();


        exampleSingleton.ExportDTO(ref masterDTO);
        #endregion



        //Serialize

        // ── ENABLE POLYMORPHIC SERIALIZATION ──
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,      // for pretty print
            TypeNameHandling = TypeNameHandling.Auto,
        };

        //string json = JsonUtility.ToJson(masterDTO, prettyPrint: true);
        string json = JsonConvert.SerializeObject(masterDTO, settings);
        File.WriteAllText(saveFiles[saveIndex], json);
    }



    #region XML doc
    /// <summary>
    /// Deletes all info from a save file.
    /// </summary>
    /// <param name="-1">Index of the save file to delete the info of.</param>
    #endregion
    public void DeleteSave(int fileIndex = -1){
        if (fileIndex < 0)
        {
            fileIndex = currentSaveFileIndex;
        }
        else
        {
            currentSaveFileIndex = fileIndex;
        }



        string fullPath = Path.Combine(Application.persistentDataPath, saveFiles[fileIndex]);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                Debug.Log($"Deleted save file: {fullPath}");
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"Failed to delete save file {fullPath}: {ioEx.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Save file not found (no need to delete): {fullPath}");
        }
    }
#else
    public void Awake() {
        CallLackOfDependency();
    }

    public void CallLackOfDependency()
    {
        Debug.LogWarning("Saving feature not enabled. Download dependency and define SHUU_SAVE_DEPENDENCY symbol to enable it. (check README)");
    }
    public static void CallLackOfDependencyStatic()
    {
        Debug.LogWarning("Saving feature not enabled. Download dependency and define SHUU_SAVE_DEPENDENCY symbol to enable it. (check README)");
    }
#endif
}

}
