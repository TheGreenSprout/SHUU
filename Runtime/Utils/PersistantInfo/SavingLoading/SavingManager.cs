using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

using SHUU.UserSide.Commons;
using SHUU.Utils.Helpers;
using SHUU.Utils.Helpers.ScriptableObjects;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

using static SHUU.Utils.Data.DataManager;

namespace SHUU.Utils.PersistantInfo.SavingLoading
{
    #region XML doc
    /// <summary>
    /// Script that manages all save files.
    /// </summary>
    #endregion
    public class SavingManager : AutoSave_Json_MonoBehaviour<BackupTracker>
    {
        #region Variables
        public static SavingManager instance { get; private set; }



        [SerializeField] protected List<string> excludedScenes;



        [Header("File Paths")]
        [SerializeField] private CustomFilePathsAsset filePathsAsset;

        [SerializeField] private string saveFilesPath_ID = "Saves";
        [SerializeField] private string backupFilesPath_ID = "Backups";



        [Header("Saving Settings")]
        [SerializeField] private List<string> saveFiles;

        [SerializeField] private int backupFilesAmmount = 5;
        private List<CircularQueue<string>> backupFiles;

        private BackupTracker backupTracker;
        protected override string FileAddress() => Path.Combine(filePathsAsset.GetPath(backupFilesPath_ID), "BackupTracker.json");


        [Header("Saving Settings")]
        [SerializeField] private bool saveToFileWhenClosing = false;



        private int _currentSaveFileIndex = 0;
        public int currentSaveFileIndex
        {
            get => _currentSaveFileIndex;

            set
            {
                if (value >= instance?.saveFiles.Count) value = 0;
                else if (value < 0) value = instance?.saveFiles.Count-1 ?? 0;

                _currentSaveFileIndex = value;
            }
        }


        private List<ISavingInfo> savingInfoScrs;



        private static bool debugLogEmission => SHUU_Preferences.instance.saving_debugLogEmission;
        #endregion
        
        
        
        
        #region Main
        protected override void Awake()
        {
            base.Awake();

            
            string saveFilesPath = filePathsAsset.GetPath(saveFilesPath_ID);

            Directory.CreateDirectory(saveFilesPath);

            for (int i = 0; i < saveFiles.Count; i++)
                saveFiles[i] = Path.Combine(saveFilesPath, saveFiles[i] + ".json");


            BackupsInit();


            savingInfoScrs = new List<ISavingInfo>(GetComponents<ISavingInfo>());

            SceneManager.sceneLoaded += OnEverySceneLoaded;
            
            
            instance = this;
        }

        private void BackupsInit()
        {
            string root = filePathsAsset.GetPath(backupFilesPath_ID);

            backupFiles = new();

            foreach (var save in saveFiles)
            {
                string directory = Path.Combine(root, Path.GetFileNameWithoutExtension(save));
                Directory.CreateDirectory(directory);


                CircularQueue<string> backups = new();

                for (int i = 1; i <= backupFilesAmmount; i++)
                    backups.Enqueue(Path.Combine(directory, $"Backup_{i}.json"));

                backupFiles.Add(backups);
            }

            
            backupTracker.AdjustQueues(GetComparator(), ref backupFiles);
        }

        private string GetComparator() => string.Join("_", saveFiles);


        private bool dispose = false;
        protected override void Dispose()
        {
            base.Dispose();


            if (dispose) return;
            dispose = true;


            PlayerPrefs.Save();
            if (saveToFileWhenClosing) SaveJsonInfo();

            SceneManager.sceneLoaded -= OnEverySceneLoaded;
        }
        #endregion



        #region Logic

        #region Scene
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
                if (sceneName == name) return false;

            return true;
        }


        #region XML doc
        /// <summary>
        /// Logic executed everytime a new scene is loaded.
        /// </summary>
        #endregion
        private void OnEverySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!IsValidScene(scene.name)) return;

            LoadLocalInfo(scene.name);
        }
        #endregion



        #region Saves
        #region XML doc
        /// <summary>
        /// Saves all saved info to save file.
        /// </summary>
        /// <param name="-1">Index of the save file to save info to.</param>
        #endregion
        public void SaveJsonInfo(int fileIndex = -1)
        {
            if (fileIndex < 0 || !saveFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;
            else currentSaveFileIndex = fileIndex;


            MasterDTO masterDTO = new MasterDTO();

            foreach (ISavingInfo savingInfo in savingInfoScrs)
                masterDTO.dataDictionary[savingInfo.Identifier] = savingInfo.ExportDTO();


            WriteText_ToFile(saveFiles[fileIndex], SerializeJson(masterDTO));
        }

        #region XML doc
        /// <summary>
        /// Triggers all Singletons persistance scripts to save their info. (Temporary info)
        /// </summary>
        /// <param name="sceneName">Name of the current scene.</param>
        #endregion
        public void SaveLocalInfo(string sceneName)
        {
            if (!IsValidScene(sceneName)) return;

            foreach (ISavingInfo singInfo in savingInfoScrs)
                singInfo.SaveInfo(sceneName);
        }


        #region XML doc
        /// <summary>
        /// Loads all saved info from save file.
        /// </summary>
        /// <param name="-1">Index of the save file to load info from.</param>
        /// <returns>Returns whether the loading of info was successful.</returns>
        #endregion
        public bool LoadJsonInfo(int fileIndex = -1)
        {
            if (fileIndex < 0 || !saveFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;
            else currentSaveFileIndex = fileIndex;
            
            if (!File.Exists(saveFiles[fileIndex])) return false;


            MasterDTO masterDTO = DeserializeJson<MasterDTO>(ReadText_FromFile(saveFiles[fileIndex]));

            foreach (ISavingInfo savingInfo in savingInfoScrs)
            {
                if (!masterDTO.dataDictionary.TryGetValue(savingInfo.Identifier, out DTO_Info dto) || dto == null)
                {
                    if (debugLogEmission) Debug.LogWarning($"No DTO found for ISavingInfo with identifier {savingInfo.Identifier}. Skipping loading info for this ISavingInfo.");
                    continue;
                }
                            
                try { savingInfo.ImportDTO(dto); }
                catch (Exception e) { if (debugLogEmission) Debug.LogError($"Failed to load {savingInfo.Identifier}: {e}"); }
            }
            
            
            return true;
        }

        #region XML doc
        /// <summary>
        /// Triggers all Singletons persistance scripts to load their info. (Temporary info)
        /// </summary>
        /// <param name="sceneName">Name of the current scene.</param>
        #endregion
        public void LoadLocalInfo(string sceneName)
        {
            if (!IsValidScene(sceneName)) return;

            foreach (ISavingInfo singInfo in savingInfoScrs)
                singInfo.LoadInfo(sceneName);
        }


        #region XML doc
        /// <summary>
        /// Deletes all info from a save file.
        /// </summary>
        /// <param name="-1">Index of the save file to delete the info of.</param>
        #endregion
        public void DeleteSave(int fileIndex = -1)
        {
            if (fileIndex < 0 || !saveFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;


            string fullPath = saveFiles[fileIndex];

            if (DeleteFile(fullPath)) if (debugLogEmission) Debug.Log($"Deleted save file: {fullPath}");
            else if (debugLogEmission) Debug.LogWarning($"Failed to delete save file: {fullPath}");
        }
        #endregion


        
        #region Backups
        public string Backup(int fileIndex = -1)
        {
            if (fileIndex < 0 || !backupFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;

            if (!File.Exists(saveFiles[fileIndex])) return null;


            string address = backupFiles[fileIndex].Dequeue();
            WriteText_ToFile(address, ReadText_FromFile(saveFiles[fileIndex]));

            return address;
        }


        public void RestoreLatestBackup(int fileIndex = -1)
        {
            if (fileIndex < 0 || !backupFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;

            if (!File.Exists(backupFiles[fileIndex].PeekLast())) return;


            WriteText_ToFile(saveFiles[fileIndex], ReadText_FromFile(backupFiles[fileIndex].PeekLast()));
        }

        public void RestoreBackup(string address, int fileIndex = -1)
        {
            if (fileIndex < 0 || !backupFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;

            if (string.IsNullOrEmpty(address) || !File.Exists(address)) return;


            WriteText_ToFile(saveFiles[fileIndex], ReadText_FromFile(address));
        }


        public void DeleteBackups(int fileIndex = -1)
        {
            if (fileIndex < 0 || !backupFiles.IndexIsValid(fileIndex)) fileIndex = currentSaveFileIndex;

            bool loop = true;
            string backup = backupFiles[fileIndex].Dequeue();
            string compare = backup;
            while (loop)
            {
                if (File.Exists(backup)) WriteText_ToFile(backup, "");

                backup = backupFiles[fileIndex].Dequeue();
                if (backup.Equals(compare)) loop = false;
            }            
        }

        
        protected override BackupTracker SaveData() => backupTracker ?? new();

        protected override void LoadData(BackupTracker data)
        {
            if (data != null) backupTracker = data;
            else backupTracker = new();
        }
        #endregion

        #endregion
    }




    #region Helper class
    [Serializable]
    public class BackupTracker
    {
        private string comparator = null;



        public bool initialized = false;

        public List<int> backups;




        private void Init(string comparator, List<CircularQueue<string>> backupFiles)
        {
            if (initialized) 
            {
                if (string.IsNullOrWhiteSpace(this.comparator) || !this.comparator.Equals(comparator) || backups == null || backups.Count == 0) Reset(comparator, backupFiles);
                return;
            }
            initialized = true;


            Reset(comparator, backupFiles);
        }

        private void Reset(string comparator, List<CircularQueue<string>> backupFiles)
        {
            this.comparator = comparator;

            backups = new();
            for (int i = 0; i < backupFiles.Count; i++)
                backups.Add(0);
        }


        public void AdjustQueues(string comparator, ref List<CircularQueue<string>> backupFiles)
        {
            Init(comparator, backupFiles);

            for (int i = 0; i < backupFiles.Count; i++)
                for (int j = 0; j < backups[i]; j++)
                    backupFiles[i].Dequeue();
        }
    }
    #endregion
}
