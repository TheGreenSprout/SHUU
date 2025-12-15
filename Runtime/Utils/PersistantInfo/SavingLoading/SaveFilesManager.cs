using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using SHUU.Utils.SceneManagement;
using SHUU.UserSide.Commons;
using System.Linq;

namespace SHUU.Utils.PersistantInfo.SavingLoading
{

    [RequireComponent(typeof(SavingPersistance))]
    #region XML doc
    /// <summary>
    /// Script that manages all save files.
    /// </summary>
    #endregion
    public class SaveFilesManager : MonoBehaviour
    {
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



            // Get (and load) all your DTO sinletons.
            foreach (SavingInfo singleton in gameObject.GetComponents<SavingInfo>())
            {
                DTO_Info dto = masterDTO.dataDictionary.FirstOrDefault(x => x.Value == singleton.identifier).Key;
                
                singleton.ImportDTO(dto);
            }
            
            
            
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


            SavingPersistance savingPersistance = GetComponent<SavingPersistance>();
            savingPersistance.SaveAllSingletonInfo(SceneLoader.GetCurrentSceneName());
            
            
            // Get (and save) all your DTO sinletons.
            foreach (SavingInfo singleton in gameObject.GetComponents<SavingInfo>())
            {
                masterDTO.dataDictionary.Add(singleton.ExportDTO(), singleton.identifier);
            }



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
    }

}
