using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SHUU.UserSide.Addons.SavingSystem
{
    internal sealed class SaveStorage
    {
        #region Variables
        public const string SaveFileName = "save.json";
        public const string InfoFileName = "info.json";
        public const string ThumbnailFileName = "thumbnail.png";

        private const string TempSuffix = ".tmp";

        // Windows can refuse to replace a file for a moment while an antivirus, a cloud sync or the indexer has it open.
        private const int ReplaceAttempts = 3;
        private const string CorruptSuffix = ".corrupt";

        private const string BackupPrefix = "backup_";
        private const string BackupExtension = ".json";
        private const string BackupTimeFormat = "yyyyMMdd'T'HHmmss'Z'";

        private static readonly Regex BackupNameRegex = new Regex(@"^backup_(\d+)_(\d{8}T\d{6}Z)\.json\z", RegexOptions.CultureInvariant);

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);



        public string savesRoot { get; }
        public string backupsRoot { get; }
        #endregion




        #region Main
        public SaveStorage(string savesRoot, string backupsRoot)
        {
            if (string.IsNullOrWhiteSpace(savesRoot)) throw new ArgumentException("The saves folder can't be empty.", nameof(savesRoot));
            if (string.IsNullOrWhiteSpace(backupsRoot)) throw new ArgumentException("The backups folder can't be empty.", nameof(backupsRoot));

            this.savesRoot = Path.GetFullPath(savesRoot);
            this.backupsRoot = Path.GetFullPath(backupsRoot);
        }
        #endregion



        #region Logic

        #region Paths
        #region XML doc
        /// <summary>
        /// Save ids are used as folder names, so only letters, numbers, '_' and '-' are allowed (and it can't start with a symbol).
        /// </summary>
        #endregion
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 64) return false;

            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];

                bool alphanumeric = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
                if (alphanumeric) continue;

                if (i > 0 && (c == '_' || c == '-')) continue;

                return false;
            }

            return true;
        }


        public string SaveDirectory(string id) => Resolve(savesRoot, id, backupsRoot);
        public string BackupDirectory(string id) => Resolve(backupsRoot, id, savesRoot);

        public string SavePath(string id) => Path.Combine(SaveDirectory(id), SaveFileName);
        public string InfoPath(string id) => Path.Combine(SaveDirectory(id), InfoFileName);
        public string ThumbnailPath(string id) => Path.Combine(SaveDirectory(id), ThumbnailFileName);


        private static string Resolve(string root, string id, string otherRoot)
        {
            if (!IsValidId(id)) throw new ArgumentException($"'{id}' is not a valid save id (use letters, numbers, '_' and '-').", nameof(id));

            string directory = Path.GetFullPath(Path.Combine(root, id));

            if (IsSameOrParent(directory, otherRoot))
                throw new ArgumentException($"The save id '{id}' clashes with the {(otherRoot == root ? "" : "other ")}root folder.", nameof(id));

            return directory;
        }

        private static bool IsSameOrParent(string parent, string path)
        {
            parent = parent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.Equals(parent, path, StringComparison.OrdinalIgnoreCase)) return true;

            return path.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(parent + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        #endregion



        #region Reading/Writing
        public static void WriteAtomic(string path, string text) => WriteAtomic(path, Utf8.GetBytes(text));
        public static void WriteAtomic(string path, byte[] bytes)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);


            string temp = path + TempSuffix;

            using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }


            if (!File.Exists(path))
            {
                File.Move(temp, path);

                return;
            }

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.Replace(temp, path, null);

                    return;
                }
                catch (Exception e) when (e is PlatformNotSupportedException || e is NotImplementedException)
                {
                    File.Delete(path);
                    File.Move(temp, path);

                    return;
                }
                catch (Exception e) when (attempt < ReplaceAttempts && (e is IOException || e is UnauthorizedAccessException))
                    { Thread.Sleep(25 * attempt); }
            }
        }


        public static string ReadText(string path) => File.ReadAllText(path, Utf8);


        public bool SaveExists(string id) => File.Exists(SavePath(id));

        public bool TryReadSaveText(string id, out string text)
        {
            string path = SavePath(id);

            text = File.Exists(path) ? ReadText(path) : null;

            return text != null;
        }


        #region XML doc
        /// <summary>
        /// If the game crashed while replacing a save file (only possible on platforms without atomic replace), the finished temporary file is all that's left. Restores it.
        /// </summary>
        /// <returns>Whether a save file was recovered.</returns>
        #endregion
        public bool PromoteTempIfMainMissing(string id, Func<string, bool> isIntact)
        {
            string main = SavePath(id);
            string temp = main + TempSuffix;

            if (File.Exists(main) || !File.Exists(temp)) return false;

            if (isIntact != null && !isIntact(ReadText(temp))) return false;


            File.Move(temp, main);

            return true;
        }


        #region XML doc
        /// <summary>
        /// Lists the ids of every save file that has data on disk. This only checks that the files exist, it never reads the save data
        /// (the one exception: a save whose main file is missing but has a finished temp file, which is checked with <paramref name="isIntact"/> (id, text) and recovered).
        /// </summary>
        #endregion
        public List<string> ListSaveIds(Func<string, string, bool> isIntact = null)
        {
            List<string> ids = new List<string>();

            if (!Directory.Exists(savesRoot)) return ids;


            foreach (string directory in Directory.EnumerateDirectories(savesRoot))
            {
                string id = Path.GetFileName(directory);

                if (!IsValidId(id) || IsSameOrParent(directory, backupsRoot)) continue;

                if (!File.Exists(Path.Combine(directory, SaveFileName)) && !PromoteTempIfMainMissing(id, isIntact == null ? null : new Func<string, bool>(text => isIntact(id, text)))) continue;


                ids.Add(id);
            }

            return ids;
        }
        #endregion



        #region Backups
        #region XML doc
        /// <summary>
        /// Gets the backups of a save file, newest first.
        /// </summary>
        #endregion
        public List<SaveBackup> GetBackups(string id)
        {
            List<SaveBackup> backups = new List<SaveBackup>();

            string directory = BackupDirectory(id);
            if (!Directory.Exists(directory)) return backups;


            foreach (string file in Directory.EnumerateFiles(directory, BackupPrefix + "*" + BackupExtension))
            {
                Match match = BackupNameRegex.Match(Path.GetFileName(file));
                if (!match.Success) continue;

                if (!int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int sequence)) continue;

                if (!DateTime.TryParseExact(match.Groups[2].Value, BackupTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime createdUtc)) continue;


                backups.Add(new SaveBackup(file, sequence, createdUtc));
            }

            backups.Sort((a, b) => b.sequence.CompareTo(a.sequence));

            return backups;
        }


        #region XML doc
        /// <summary>
        /// Copies the current save file into the backups (and deletes the oldest backups if there are more than <paramref name="keep"/>).
        /// A damaged save is never backed up, otherwise it would slowly push every good backup out.
        /// </summary>
        /// <param name="keep">How many backups to keep. 0 or less turns backups off.</param>
        /// <param name="minSecondsBetween">Skips the backup if the newest one is younger than this. Ignored when <paramref name="force"/> is true.</param>
        /// <param name="isIntact">Tells whether the text of the save file is undamaged.</param>
        #endregion
        public BackupOutcome CreateBackup(string id, int keep, double minSecondsBetween, Func<string, bool> isIntact, bool force, out SaveBackup created, DateTime? utcNow = null)
        {
            created = null;

            if (keep <= 0) return BackupOutcome.Disabled;

            string savePath = SavePath(id);
            if (!File.Exists(savePath)) return BackupOutcome.NoSave;


            DateTime now = (utcNow ?? DateTime.UtcNow).ToUniversalTime();
            now = new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);

            List<SaveBackup> existing = GetBackups(id);

            if (!force && minSecondsBetween > 0 && existing.Count > 0)
            {
                double age = (now - existing[0].createdUtc).TotalSeconds;

                if (age >= 0 && age < minSecondsBetween) return BackupOutcome.TooSoon;
            }


            string text = ReadText(savePath);
            if (isIntact != null && !isIntact(text)) return BackupOutcome.DamagedSave;


            int sequence = existing.Count > 0 ? existing[0].sequence + 1 : 1;
            string fileName = $"{BackupPrefix}{sequence:D6}_{now.ToString(BackupTimeFormat, CultureInfo.InvariantCulture)}{BackupExtension}";
            string destination = Path.Combine(BackupDirectory(id), fileName);

            WriteAtomic(destination, text);
            created = new SaveBackup(destination, sequence, now);

            Prune(id, keep);

            return BackupOutcome.Created;
        }

        private void Prune(string id, int keep)
        {
            List<SaveBackup> backups = GetBackups(id);

            for (int i = keep; i < backups.Count; i++)
                TryDelete(backups[i].path);


            string directory = BackupDirectory(id);
            if (!Directory.Exists(directory)) return;

            foreach (string leftover in Directory.EnumerateFiles(directory, "*" + TempSuffix))
                TryDelete(leftover);
        }


        #region XML doc
        /// <summary>
        /// Puts a backup back as the save file. Whatever save file was there is kept as a new backup first, so a restore can be undone.
        /// </summary>
        #endregion
        public RestoreOutcome RestoreBackup(string id, SaveBackup backup, int keep, Func<string, bool> isIntact, Func<string, bool> isLoadable)
        {
            if (backup == null || !File.Exists(backup.path)) return RestoreOutcome.BackupMissing;

            string text = ReadText(backup.path);
            if (isLoadable != null && !isLoadable(text)) return RestoreOutcome.BackupDamaged;


            if (File.Exists(SavePath(id)))
            {
                BackupOutcome kept = CreateBackup(id, keep, 0, isIntact, true, out _);

                if (kept == BackupOutcome.DamagedSave) QuarantineSave(id);
            }

            WriteAtomic(SavePath(id), text);

            return RestoreOutcome.Restored;
        }


        #region XML doc
        /// <summary>
        /// Replaces a damaged save file with the newest backup that still works. The damaged file is kept next to it as "save.json.corrupt".
        /// </summary>
        #endregion
        public bool TryRecoverFromBackups(string id, Func<string, bool> isLoadable, out string text, out SaveBackup used)
        {
            text = null;
            used = null;

            foreach (SaveBackup backup in GetBackups(id))
            {
                string candidate;

                try { candidate = ReadText(backup.path); }
                catch (IOException) { continue; }

                if (isLoadable != null && !isLoadable(candidate)) continue;


                QuarantineSave(id);
                WriteAtomic(SavePath(id), candidate);

                text = candidate;
                used = backup;

                return true;
            }

            return false;
        }

        #region XML doc
        /// <summary>
        /// Keeps a copy of the save file next to it as "save.json.corrupt", for when it's about to be replaced but might still be worth looking at.
        /// </summary>
        #endregion
        public void QuarantineSave(string id)
        {
            string main = SavePath(id);

            if (File.Exists(main)) File.Copy(main, main + CorruptSuffix, true);
        }

        #region XML doc
        /// <summary>
        /// Same as <see cref="QuarantineSave"/>, for the info file ("info.json.corrupt").
        /// </summary>
        #endregion
        public void QuarantineInfo(string id)
        {
            string info = InfoPath(id);

            if (File.Exists(info)) File.Copy(info, info + CorruptSuffix, true);
        }
        #endregion



        #region Deleting
        public void DeleteBackups(string id) => DeleteDirectory(BackupDirectory(id));

        #region XML doc
        /// <summary>
        /// Deletes a save file with everything that belongs to it (info, thumbnail and backups).
        /// </summary>
        #endregion
        public void DeleteSave(string id)
        {
            string saveDirectory = SaveDirectory(id);
            string backupDirectory = BackupDirectory(id);

            DeleteDirectory(saveDirectory);
            DeleteDirectory(backupDirectory);
        }


        private static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        private static void TryDelete(string path)
        {
            try { File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        #endregion

        #endregion
    }





    #region Backup info
    #region XML doc
    /// <summary>
    /// A backup of a save file that exists on disk.
    /// </summary>
    #endregion
    public sealed class SaveBackup
    {
        #region Variables
        public string path { get; }

        #region XML doc
        /// <summary>
        /// Goes up with every backup taken of a save file (higher = newer).
        /// </summary>
        #endregion
        public int sequence { get; }


        public DateTime createdUtc { get; }
        #endregion



        #region Main
        public SaveBackup(string path, int sequence, DateTime createdUtc)
        {
            this.path = path;
            this.sequence = sequence;
            this.createdUtc = createdUtc;
        }
        #endregion
    }


    internal enum BackupOutcome
    {
        Created,
        Disabled,
        NoSave,
        TooSoon,
        DamagedSave,
        Failed
    }

    internal enum RestoreOutcome
    {
        Restored,
        BackupMissing,
        BackupDamaged
    }
    #endregion
}
