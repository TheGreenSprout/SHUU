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
        [Tooltip("If null or empty, the path will be Application.persistentDataPath + customSaveFilesFolder")]
        [SerializeField] private string customSaveFilesPath = null;
        [Tooltip("If null or empty, the path will be Application.persistentDataPath")]
        [SerializeField] private string customSaveFilesFolder = "/Saves";



        public List<string> saveFiles;


        [SerializeField] private bool autoSaveWhenClosing;



        private static int _currentSaveFileIndex = 0;
        public static int currentSaveFileIndex
        {
            get => _currentSaveFileIndex;

            set
            {
                if (value >= Persistant_Globals.saveFilesManager.saveFiles.Count) value = 0;
                else if (value < 0) value = Persistant_Globals.saveFilesManager.saveFiles.Count-1;

                _currentSaveFileIndex = value;
            }
        }
        
        
        
        
        private void Awake()
        {
            if (string.IsNullOrEmpty(customSaveFilesPath)) customSaveFilesPath = Application.persistentDataPath;
            else customSaveFilesPath = Path.Combine(Application.persistentDataPath);

            customSaveFilesPath += customSaveFilesFolder;


            Directory.CreateDirectory(customSaveFilesPath);



            for (int i = 0; i < saveFiles.Count; i++)
            {
                saveFiles[i] = customSaveFilesPath + "/" + saveFiles[i] + ".json";
            }
            
            
            
            Persistant_Globals.saveFilesManager = this;
        }



        void OnApplicationQuit()
        {
            PlayerPrefs.Save();
            
            
            if (autoSaveWhenClosing) SaveJsonInfo();
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
                DTO_Info dto = masterDTO.dataDictionary.FirstOrDefault(x => x.Key == singleton.identifier).Value;
                
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
                if (masterDTO.dataDictionary.ContainsKey(singleton.identifier)) masterDTO.dataDictionary.Remove(singleton.identifier);

                masterDTO.dataDictionary.Add(singleton.identifier, singleton.ExportDTO());
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



            string fullPath = saveFiles[fileIndex];

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
