using System;
using SHUU.Utils.PersistantInfo;
using SHUU.Utils.SceneManagement;
using UnityEngine;

namespace SHUU.Utils.Globals
{

    public class SavingSystemManager : MonoBehaviour
    {
        [Header("AutoSave Settings")]
        [SerializeField] private bool saveToFile_OnRoomChange = false;
        [SerializeField] private bool saveToFile_OnApplicationQuit = false;




        private void Awake()
        {
            SHUU_GlobalsProxy.savingSystemManager = this;
        }


        public void OnRoomChange()
        {
            if (saveToFile_OnRoomChange) SaveSingletonInfoToFile();
            else SaveSingletonInfo();
        }

        private void OnDestroy()
        {
            if (saveToFile_OnApplicationQuit) SaveSingletonInfoToFile();
        }


        #region XML doc
        /// <summary>
        /// Triggers all singletons to save their information.
        /// </summary>
        #endregion
        public void SaveSingletonInfo()
        {
            Persistant_Globals.savingInfo.SaveAllSingletonInfo(SceneLoader.GetCurrentSceneName());
        }
        #region XML doc
        /// <summary>
        /// Saves all singleton info to a file.
        /// </summary>
        #endregion
        public void SaveSingletonInfoToFile()
        {
            Persistant_Globals.saveFilesManager.SaveJsonInfo();
        }
        
        #region XML doc
        /// <summary>
        /// Triggers all singletons to load their information.
        /// </summary>
        #endregion
        public void LoadSingletonInfo()
        {
            Persistant_Globals.savingInfo.LoadAllSingletonInfo(SceneLoader.GetCurrentSceneName());
        }
        #region XML doc
        /// <summary>
        /// Loads all singleton info from a file.
        /// </summary>
        /// <returns>Returns wether the load was successful.</returns>
        #endregion
        public bool LoadSingletonInfoFromFile()
        {
            return Persistant_Globals.saveFilesManager.LoadJsonInfo();
        }
        
        #region XML doc
        /// <summary>
        /// Deletes a save file.
        /// </summary>
        /// <param name="-1">Index of the save file to delete.</param>
        #endregion
        public void DeleteSaveInfo(int fileIndex = -1)
        {
            Persistant_Globals.saveFilesManager.DeleteSave(fileIndex);
        }
    }

}
