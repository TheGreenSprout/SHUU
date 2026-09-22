using System;
using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Developer.Console;

namespace SHUU.UserSide.Addons.SavingSystem
{
    public class SavingSystem_DevConsoleCommands : MonoBehaviour
    {
        #region Variables
        private static CommandReturn NoManager() => CommandReturn.Red("There's no SavingManager in the game.");


        private static string PlayTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            return $"{(int)time.TotalHours}h {time.Minutes}m";
        }
        #endregion




        #region Saving/Loading
        [DevConsoleCommand("save", "Saves the scene into memory (temporary, nothing is written to disk)", "Debug")]
        public static CommandReturn Save()
            => SHUU_Saving.SnapshotScene()
                ? CommandReturn.Green("Data saved to memory successfully.")
                : CommandReturn.Red("Some of the info couldn't be saved to memory (check the console).");

        [DevConsoleCommand("filesave", "Saves the game into the current save file", "Debug")]
        public static CommandReturn FileSave()
            => SHUU_Saving.FullSave() ? CommandReturn.Green("Game saved successfully.") : CommandReturn.Red("The game couldn't be saved (check the console).");


        [DevConsoleCommand("load", "Loads the data in memory into the scene", "Debug")]
        public static CommandReturn Load()
        {
            SHUU_Saving.ApplySnapshot();

            return CommandReturn.Green("Data loaded from memory successfully.");
        }

        [DevConsoleCommand("fileload", "Loads the current save file into the game", "Debug")]
        public static CommandReturn FileLoad()
            => SHUU_Saving.FullLoad() ? CommandReturn.Green("Game loaded successfully.") : CommandReturn.Red("The game couldn't be loaded (check the console).");
        #endregion



        #region Save files
        [DevConsoleCommand("saves", "Lists all the save files (the one being used is marked with >)", "Debug")]
        public static CommandReturn Saves()
        {
            SavingManager manager = SavingManager.Instance;
            if (manager == null) return NoManager();

            if (manager.Saves.Count == 0) return CommandReturn.Yellow("There are no saves.");


            List<string> lines = new List<string>();

            foreach (SaveEntry entry in manager.Saves)
            {
                string line = (entry == manager.Current ? "> " : "  ") + entry.id + "  " + entry.displayName;

                line += entry.hasData ? "  " + entry.lastSavedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm") + "  " + PlayTime(entry.info.playTimeSeconds) : "  (empty)";

                lines.Add(line);
            }

            return new CommandReturn(lines.ToArray());
        }

        [DevConsoleCommand("newsave", "Starts a new save and selects it (it's written on the next save)", "Debug")]
        public static CommandReturn NewSave(OptionalParameter<string> name)
        {
            SavingManager manager = SavingManager.Instance;
            if (manager == null) return NoManager();


            SaveEntry entry = manager.CreateSave(name.TryGetValue(out string value) ? value : null);

            return entry != null ? CommandReturn.Green($"Started '{entry.displayName}' ({entry.id}).") : CommandReturn.Red("A new save couldn't be started (check the console).");
        }

        [DevConsoleCommand("selectsave", "Selects a save by its id (in Fixed Slots mode, also by slot number starting at 1)", "Debug")]
        public static CommandReturn SelectSave(string idOrSlot)
        {
            SavingManager manager = SavingManager.Instance;
            if (manager == null) return NoManager();


            bool selected = manager.Mode == SaveFileMode.FixedSlots && int.TryParse(idOrSlot, out int slot) ? manager.SelectSlot(slot - 1) : manager.SelectSave(idOrSlot);

            return selected ? CommandReturn.Green($"Selected '{manager.Current.displayName}'.") : CommandReturn.Red("There's no such save (use 'saves' to see them).");
        }

        [DevConsoleCommand("deletesave", "Deletes a save file with its backups (the current one if no id is given)", "Debug")]
        public static CommandReturn DeleteSave(OptionalParameter<string> id)
            => SHUU_Saving.DeleteSave(id.TryGetValue(out string value) ? value : null)
                ? CommandReturn.Green("Save deleted.")
                : CommandReturn.Red("The save couldn't be deleted (check the console).");
        #endregion



        #region Backups
        [DevConsoleCommand("backup", "Backs up the current save file right now", "Debug")]
        public static CommandReturn Backup()
            => SHUU_Saving.Backup() ? CommandReturn.Green("Backup created successfully.") : CommandReturn.Red("The save couldn't be backed up (does it exist and work?).");

        [DevConsoleCommand("backups", "Lists the backups of the current save file (0 is the newest)", "Debug")]
        public static CommandReturn Backups()
        {
            SavingManager manager = SavingManager.Instance;
            if (manager == null) return NoManager();


            var backups = manager.GetBackups();

            if (backups.Count == 0) return CommandReturn.Yellow("There are no backups.");


            List<string> lines = new List<string>();

            for (int i = 0; i < backups.Count; i++)
                lines.Add($"{i}:  backup {backups[i].sequence}  {backups[i].createdUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}");

            return new CommandReturn(lines.ToArray());
        }

        [DevConsoleCommand("restorebackup", "Restores a backup into the save file (0 is the newest), then use fileload to load it", "Debug")]
        public static CommandReturn RestoreBackup(OptionalParameter<int> index)
        {
            int i = index.TryGetValue(out int o) ? o : 0;

            return SHUU_Saving.RestoreBackup(i) ? CommandReturn.Green("Backup restored into the save file, use fileload to load it.") : CommandReturn.Red("The backup couldn't be restored (check the console).");
        }
        #endregion



        #region Player prefs
        [DevConsoleCommand("saveprefs", "Saves all player prefs", "Debug")]
        public static CommandReturn SavePrefs()
        {
            PlayerPrefs.Save();

            return CommandReturn.Green("PlayerPrefs saved successfully.");
        }

        [DevConsoleCommand("deleteprefs", "Deletes all player prefs", "Debug")]
        public static CommandReturn DeletePrefs()
        {
            PlayerPrefs.DeleteAll();

            return CommandReturn.Green("PlayerPrefs deleted successfully.");
        }
        #endregion
    }
}
