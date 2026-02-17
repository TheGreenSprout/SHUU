using System.Collections.Generic;
using SHUU.Utils.Helpers;
using SHUU.Utils.PersistantInfo;
using SHUU.Utils.SceneManagement;
using UnityEngine;

namespace SHUU.Utils.Globals
{

    [DefaultExecutionOrder(-10000)]
    public class SHUU_Saving : StaticInstance_Monobehaviour<SHUU_Saving>
    {
        [Header("AutoSave Settings")]
        [SerializeField] private bool backupWhenSave = true;

        [SerializeField] private bool localSave_OnRoomChange = true;
        [SerializeField] private bool saveToFile_OnRoomChange = false;

        [SerializeField] private bool localSave_OnApplicationQuit = true;
        [SerializeField] private bool saveToFile_OnApplicationQuit = false;



        private Dictionary<int, InvertedList<string>> backups = new();




        #region Init
        public static void OnRoomChange()
        {
            if (instance.localSave_OnRoomChange) SaveSingletonInfo();
            if (instance.saveToFile_OnRoomChange) SaveSingletonInfoToFile();
        }


        private void OnDestroy()
        {
            if (localSave_OnApplicationQuit) SaveSingletonInfo();
            if (saveToFile_OnApplicationQuit) SaveSingletonInfoToFile();
        }
        #endregion



        #region Save
        #region XML doc
        /// <summary>
        /// Triggers all singletons to save their information.
        /// </summary>
        #endregion
        public static void SaveSingletonInfo() => Persistant_Globals.savingInfo?.SaveAllSingletonInfo(SceneLoader.GetCurrentSceneName());
        
        #region XML doc
        /// <summary>
        /// Saves all singleton info to a file.
        /// </summary>
        #endregion
        public static void SaveSingletonInfoToFile()
        {
            Persistant_Globals.saveFilesManager?.SaveJsonInfo();

            if (instance.backupWhenSave) Backup();
        }

        #region XML doc
        /// <summary>
        /// Triggers all singletons to save their information and then saves the info to a file.
        /// </summary>
        #endregion
        public static void FullSave()
        {
            SaveSingletonInfo();
            SaveSingletonInfoToFile();
        }
        #endregion

        
        #region Load
        #region XML doc
        /// <summary>
        /// Triggers all singletons to load their information.
        /// </summary>
        #endregion
        public static void LoadSingletonInfo() => Persistant_Globals.savingInfo?.LoadAllSingletonInfo(SceneLoader.GetCurrentSceneName());
        
        #region XML doc
        /// <summary>
        /// Loads all singleton info from a file.
        /// </summary>
        /// <returns>Returns wether the load was successful.</returns>
        #endregion
        public static bool LoadSingletonInfoFromFile() => Persistant_Globals.saveFilesManager?.LoadJsonInfo() ?? false;
        
        #region XML doc
        /// <summary>
        /// Loads all singleton info from a file and then triggers all singletons to load their information.
        /// </summary>
        /// <returns>Returns wether the load was successful.</returns>
        #endregion
        public static bool FullLoad()
        {
            if (LoadSingletonInfoFromFile())
            {
                LoadSingletonInfo();

                return true;
            }
            else return false;
        }
        #endregion
        

        #region Backup
        public static string Backup()
        {
            int i = Persistant_Globals.saveFilesManager?.currentSaveFileIndex ?? -1;
            if (!instance.backups.ContainsKey(i)) instance.backups.Add(i, new());


            string b = Persistant_Globals.saveFilesManager?.Backup();

            instance.backups[i].RemoveAll(x => x.Equals(b));
            instance.backups[i].Add(b);

            return b;
        }

        public static void RestoreLatestBackup() => Persistant_Globals.saveFilesManager?.RestoreLatestBackup();
        public static void RestoreBackup(int index)
        {
            int i = Persistant_Globals.saveFilesManager?.currentSaveFileIndex ?? -1;
            if (!instance.backups.ContainsKey(i)) instance.backups.Add(i, new());


            if (instance.backups[i].Count == 0) return;

            if (!instance.backups[i].IndexIsValid(index)) index = 0;


            Persistant_Globals.saveFilesManager?.RestoreBackup(instance.backups[i][index]);
        }
        #endregion

        
        #region Delete
        #region XML doc
        /// <summary>
        /// Deletes a save file.
        /// </summary>
        /// <param name="-1">Index of the save file to delete.</param>
        #endregion
        public static void DeleteSaveInfo(int fileIndex = -1) => Persistant_Globals.saveFilesManager?.DeleteSave(fileIndex);
        
        public static void DeleteBackupInfo(int fileIndex = -1)
        {
            Persistant_Globals.saveFilesManager?.DeleteBackups(fileIndex);

            int i = Persistant_Globals.saveFilesManager?.currentSaveFileIndex ?? -1;
            if (instance.backups.ContainsKey(i)) instance.backups[i].Clear();
        }
        #endregion


        #region Misc
        public static void Change_CurrentSaveFileIndex(int changeIndex)
        {
            if (Persistant_Globals.saveFilesManager == null) return;

            Persistant_Globals.saveFilesManager.currentSaveFileIndex += changeIndex;
        }

        public static void Override_CurrentSaveFileIndex(int newIndex)
        {
            if (Persistant_Globals.saveFilesManager == null) return;

            Persistant_Globals.saveFilesManager.currentSaveFileIndex = newIndex;
        }
        #endregion
    }

}
