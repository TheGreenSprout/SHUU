using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;
using SHUU.Utils.PersistantInfo.SavingLoading;

namespace SHUU.Utils.Globals
{
    [DefaultExecutionOrder(-10000)]
    public class SHUU_Saving : Singleton_MonoBehaviour<SHUU_Saving>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        [Header("Saving Settings")]
        [SerializeField] private bool backupBeforeSave = true;

        [SerializeField] private bool localSave_OnRoomChange = true;
        [SerializeField] private bool saveToFile_OnRoomChange = false;

        [SerializeField] private bool localSave_OnApplicationQuit = true;
        [SerializeField] private bool saveToFile_OnApplicationQuit = false;



        private Dictionary<int, InvertedList<string>> backups = new();
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            SHUU_General.OnSceneChange += OnRoomChange;
        }

        public void OnRoomChange() => FullSave(localSave_OnRoomChange, saveToFile_OnRoomChange);


        private void OnApplicationQuit()
        {
            SHUU_General.OnSceneChange -= OnRoomChange;

            FullSave(localSave_OnApplicationQuit, saveToFile_OnApplicationQuit);
        }


        public static void FullSave(bool localSave, bool saveToFile)
        {
            if (localSave) SaveInfo();
            if (saveToFile) SaveInfoToFile();
        }
        #endregion



        #region Logic

        #region Save
        #region XML doc
        /// <summary>
        /// Triggers all singletons to save their information.
        /// </summary>
        #endregion
        public static void SaveInfo() => SavingManager.instance?.SaveLocalInfo(SceneLoader.GetCurrentSceneName());
        
        #region XML doc
        /// <summary>
        /// Saves all singleton info to a file.
        /// </summary>
        #endregion
        public static void SaveInfoToFile()
        {
            if (instance.backupBeforeSave) Backup();

            SavingManager.instance?.SaveJsonInfo();
        }


        #region XML doc
        /// <summary>
        /// Triggers all singletons to save their information and then saves the info to a file.
        /// </summary>
        #endregion
        public static void FullSave()
        {
            SaveInfo();
            SaveInfoToFile();
        }
        #endregion

        
        
        #region Load
        #region XML doc
        /// <summary>
        /// Triggers all singletons to load their information.
        /// </summary>
        #endregion
        public static void LoadInfo() => SavingManager.instance?.LoadLocalInfo(SceneLoader.GetCurrentSceneName());
        
        #region XML doc
        /// <summary>
        /// Loads all singleton info from a file.
        /// </summary>
        /// <returns>Returns wether the load was successful.</returns>
        #endregion
        public static bool LoadInfoFromFile() => SavingManager.instance?.LoadJsonInfo() ?? false;


        #region XML doc
        /// <summary>
        /// Loads all singleton info from a file and then triggers all singletons to load their information.
        /// </summary>
        /// <returns>Returns wether the load was successful.</returns>
        #endregion
        public static bool FullLoad()
        {
            if (LoadInfoFromFile())
            {
                LoadInfo();

                return true;
            }
            else return false;
        }
        #endregion
        


        #region Backup
        public static string Backup()
        {
            int i = SavingManager.instance?.currentSaveFileIndex ?? -1;
            if (!instance.backups.ContainsKey(i)) instance.backups.Add(i, new());


            string b = SavingManager.instance?.Backup();

            instance.backups[i].RemoveAll(x => x.Equals(b));
            instance.backups[i].Add(b);

            return b;
        }


        public static void RestoreLatestBackup() => SavingManager.instance?.RestoreLatestBackup();

        public static void RestoreBackup(int index)
        {
            int i = SavingManager.instance?.currentSaveFileIndex ?? -1;
            if (!instance.backups.ContainsKey(i)) instance.backups.Add(i, new());


            if (instance.backups[i].Count == 0) return;

            if (!instance.backups[i].IndexIsValid(index)) index = 0;


            SavingManager.instance?.RestoreBackup(instance.backups[i][index]);
        }
        #endregion

        

        #region Delete
        #region XML doc
        /// <summary>
        /// Deletes a save file.
        /// </summary>
        /// <param name="-1">Index of the save file to delete.</param>
        #endregion
        public static void DeleteSaveInfo(int fileIndex = -1) => SavingManager.instance?.DeleteSave(fileIndex);
        

        public static void DeleteBackupInfo(int fileIndex = -1)
        {
            SavingManager.instance?.DeleteBackups(fileIndex);

            int i = SavingManager.instance?.currentSaveFileIndex ?? -1;
            if (instance.backups.ContainsKey(i)) instance.backups[i].Clear();
        }
        #endregion



        #region Misc
        public static void Iterate_CurrentSaveFileIndex(int changeIndex)
        {
            if (SavingManager.instance == null) return;

            SavingManager.instance.currentSaveFileIndex += changeIndex;
        }


        public static void Set_CurrentSaveFileIndex(int newIndex)
        {
            if (SavingManager.instance == null) return;

            SavingManager.instance.currentSaveFileIndex = newIndex;
        }
        #endregion
        
        #endregion
    }
}
