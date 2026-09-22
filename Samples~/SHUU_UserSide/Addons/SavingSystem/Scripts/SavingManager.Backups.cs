using System;
using System.Collections.Generic;

namespace SHUU.UserSide.Addons.SavingSystem
{
    public partial class SavingManager
    {
        #region Logic

        #region Create
        #region XML doc
        /// <summary>
        /// Backs up a save file right now (ignoring "Min Seconds Between Backups"). Saving already does this by itself when "Backup Before Save" is on.
        /// A damaged save is never backed up, so it can't push the good backups out.
        /// </summary>
        /// <param name="id">Which save to back up. Null uses the current one.</param>
        /// <returns>Whether a backup was made.</returns>
        #endregion
        public bool CreateBackup(string id = null)
        {
            if (!Ready()) return false;

            SaveEntry entry = id == null ? Current : GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning(id == null ? "There's no save selected to back up." : $"There's no save with the id '{id}'.");

                return false;
            }

            return TryCreateBackup(entry.id, true) == BackupOutcome.Created;
        }


        private BackupOutcome TryCreateBackup(string id, bool force)
        {
            try
            {
                BackupOutcome outcome = storage.CreateBackup(id, backupsToKeep, minSecondsBetweenBackups, stored => IsIntactText(id, stored), force, out SaveBackup created);

                switch (outcome)
                {
                    case BackupOutcome.Created:
                        SavingLog.Info($"Backed up '{id}' (backup {created.sequence}).");
                        break;

                    case BackupOutcome.DamagedSave:
                        SavingLog.Warning($"The save file of '{id}' is damaged, so it wasn't backed up (that would push a good backup out).");
                        break;

                    case BackupOutcome.Disabled:
                        if (force) SavingLog.Warning("Backups are turned off (Backups To Keep is 0).");
                        break;
                }

                return outcome;
            }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't back up '{id}': {e}");

                return BackupOutcome.Failed;
            }
        }
        #endregion



        #region Fetch
        #region XML doc
        /// <summary>
        /// Gets the backups of a save file, newest first.
        /// </summary>
        /// <param name="id">Which save to get the backups of. Null uses the current one.</param>
        #endregion
        public IReadOnlyList<SaveBackup> GetBackups(string id = null)
        {
            if (!Ready()) return new List<SaveBackup>();

            SaveEntry entry = id == null ? Current : GetEntry(id);

            if (entry == null) return new List<SaveBackup>();


            try { return storage.GetBackups(entry.id); }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't read the backups of '{entry.displayName}': {e}");

                return new List<SaveBackup>();
            }
        }
        #endregion



        #region Restore
        #region XML doc
        /// <summary>
        /// Puts a backup back as the save file. The save file that was there is kept as a new backup first, so this can be undone.
        /// This only changes the file, use Load afterwards to load it into the game.
        /// </summary>
        /// <param name="id">Which save to restore. Null uses the current one.</param>
        /// <returns>Whether the backup was restored.</returns>
        #endregion
        public bool RestoreBackup(SaveBackup backup, string id = null)
        {
            if (!Ready()) return false;


            SaveEntry entry = id == null ? Current : GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning(id == null ? "There's no save selected to restore." : $"There's no save with the id '{id}'.");

                return false;
            }


            try
            {
                RestoreOutcome outcome = storage.RestoreBackup(entry.id, backup, backupsToKeep, stored => IsIntactText(entry.id, stored), stored => IsLoadableText(entry.id, stored));

                switch (outcome)
                {
                    case RestoreOutcome.Restored:
                        SavingLog.Info($"Restored '{entry.displayName}' from a backup.");
                        FinishRestore(entry);
                        return true;

                    case RestoreOutcome.BackupMissing:
                        SavingLog.Warning("That backup doesn't exist anymore.");
                        return false;

                    default:
                        SavingLog.Warning("That backup is damaged or can't be read by this version of the game, nothing was changed.");
                        return false;
                }
            }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't restore a backup of '{entry.displayName}': {e}");

                return false;
            }
        }

        #region XML doc
        /// <summary>
        /// Restores the newest backup that works.
        /// </summary>
        /// <param name="id">Which save to restore. Null uses the current one.</param>
        #endregion
        public bool RestoreLatestBackup(string id = null)
        {
            IReadOnlyList<SaveBackup> backups = GetBackups(id);

            foreach (SaveBackup backup in backups)
                if (RestoreBackup(backup, id)) return true;

            if (backups.Count == 0) SavingLog.Warning("There are no backups to restore.");

            return false;
        }


        private void FinishRestore(SaveEntry entry)
        {
            entry.hasData = storage.SaveExists(entry.id);

            if (entry.hasData)
            {
                if (entry.info == null) entry.info = ReadInfo(entry.id);

                if (!entries.Contains(entry))
                {
                    entries.Add(entry);
                    SortEntries();
                }
            }

            OnSavesChanged?.Invoke();
        }
        #endregion



        #region Delete
        #region XML doc
        /// <summary>
        /// Deletes all the backups of a save file (the save file itself is kept).
        /// </summary>
        /// <param name="id">Which save to delete the backups of. Null uses the current one.</param>
        #endregion
        public bool DeleteBackups(string id = null)
        {
            if (!Ready()) return false;

            SaveEntry entry = id == null ? Current : GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning(id == null ? "There's no save selected to delete the backups of." : $"There's no save with the id '{id}'.");

                return false;
            }


            try { storage.DeleteBackups(entry.id); }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't delete the backups of '{entry.displayName}': {e}");

                return false;
            }

            SavingLog.Info($"Deleted the backups of '{entry.displayName}'.");

            return true;
        }
        #endregion
        
        #endregion
    }
}
