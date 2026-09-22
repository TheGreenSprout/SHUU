using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.SavingSystem
{
    public static class SHUU_Saving
    {
        #region Variables
        private static SavingManager Manager()
        {
            if (SavingManager.Instance == null) SavingLog.Warning("There's no SavingManager in the game (is the SAVING SINGLETON prefab in the scene?).");

            return SavingManager.Instance;
        }



        public static SaveEntry Current => SavingManager.Instance?.Current;

        public static IReadOnlyList<SaveEntry> Saves => SavingManager.Instance?.Saves;

        public static SaveFileMode Mode => SavingManager.Instance != null ? SavingManager.Instance.Mode : SaveFileMode.Dynamic;
        public static int SlotCount => SavingManager.Instance != null ? SavingManager.Instance.SlotCount : -1;
        #endregion




        #region Logic

        #region Save
        public static bool SnapshotScene() => Manager()?.SaveLocalInfo(SceneLoader.GetCurrentSceneName()) ?? false;
        public static bool WriteToFile(string id = null) => Manager()?.SaveToFile(id) ?? false;

        public static bool FullSave(string id = null) => Manager()?.Save(id) ?? false;

        
        public static Coroutine SaveWithScreenshot(string id = null, int thumbnailHeight = 180) => Manager()?.SaveWithScreenshot(id, thumbnailHeight);
        #endregion



        #region Load
        public static void ApplySnapshot() => Manager()?.LoadLocalInfo(SceneLoader.GetCurrentSceneName());
        public static bool ReadFromFile(string id = null) => Manager()?.LoadFromFile(id) ?? false;

        public static bool FullLoad(string id = null) => Manager()?.Load(id) ?? false;
        #endregion



        #region Save files
        public static SaveEntry CreateSave(string displayName = null, int slotIndex = -1) => Manager()?.CreateSave(displayName, slotIndex);


        public static bool SelectSave(string id) => Manager()?.SelectSave(id) ?? false;
        public static bool SelectSlot(int index) => Manager()?.SelectSlot(index) ?? false;

        public static void SelectNext() => Manager()?.SelectNext();
        public static void SelectPrevious() => Manager()?.SelectPrevious();

        public static void ClearSelection() => Manager()?.ClearSelection();

        public static SaveEntry GetEntry(string id) => Manager()?.GetEntry(id);
        public static SaveEntry GetSlot(int index) => Manager()?.GetSlot(index);
        public static SaveEntry GetMostRecent() => Manager()?.GetMostRecent();


        public static void RefreshSaves() => Manager()?.RefreshSaves();


        public static bool RenameSave(string id, string newName) => Manager()?.RenameSave(id, newName) ?? false;


        public static bool DeleteSave(string id = null) => Manager()?.DeleteSave(id) ?? false;
        #endregion



        #region Backup
        #region XML doc
        /// <summary>
        /// Backs up a save file right now.
        /// </summary>
        /// <param name="id">Which save to back up. Null uses the current one.</param>
        /// <returns>Returns whether a backup was made.</returns>
        #endregion
        public static bool Backup(string id = null) => Manager()?.CreateBackup(id) ?? false;

        #region XML doc
        /// <summary>
        /// Gets the backups of a save file, newest first (index 0 is the newest, see RestoreBackup).
        /// </summary>
        /// <param name="id">Which save to get the backups of. Null uses the current one.</param>
        #endregion
        public static IReadOnlyList<SaveBackup> GetBackups(string id = null) => Manager()?.GetBackups(id) ?? new List<SaveBackup>();

        #region XML doc
        /// <summary>
        /// Restores the newest backup that works. (Then use FullLoad to load it into the game)
        /// </summary>
        #endregion
        public static bool RestoreLatestBackup(string id = null) => Manager()?.RestoreLatestBackup(id) ?? false;

        #region XML doc
        /// <summary>
        /// Restores a backup by its position in the list of backups (0 is the newest). (Then use FullLoad to load it into the game)
        /// </summary>
        #endregion
        public static bool RestoreBackup(int index, string id = null)
        {
            SavingManager manager = Manager();
            if (manager == null) return false;


            IReadOnlyList<SaveBackup> backups = manager.GetBackups(id);

            if (index < 0 || index >= backups.Count)
            {
                SavingLog.Warning($"There's no backup number {index} ({backups.Count} available).");

                return false;
            }

            return manager.RestoreBackup(backups[index], id);
        }

        #region XML doc
        /// <summary>
        /// Deletes all the backups of a save file (the save file itself is kept).
        /// </summary>
        /// <param name="id">Which save to delete the backups of. Null uses the current one.</param>
        #endregion
        public static bool DeleteBackups(string id = null) => Manager()?.DeleteBackups(id) ?? false;
        #endregion

        #endregion
    }
}
