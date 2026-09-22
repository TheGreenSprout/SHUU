using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

using Alchemy.Inspector;

using SHUU.UserSide.Addons.SavingSystem.ForUser;
using SHUU.Utils.Helpers.ScriptableObjects;
using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// Script that manages all save files: creating them, saving/loading the game into them, their info (name, thumbnail...) and their backups.
    /// This file has the setup, the list of saves and which one is selected. See SavingManager.Saves.cs and SavingManager.Backups.cs for the rest.
    /// </summary>
    #endregion
    public partial class SavingManager : SceneSensitiveScript
    {
        #region Variables
        public static SavingManager Instance { get; private set; }



        #region Callbacks
        public static event Action<SaveEntry> OnSaved;
        public static event Action<SaveEntry> OnLoaded;
        public static event Action<string> OnSaveDeleted;

        public static event Action<SaveEntry> OnCurrentChanged;

        #region XML doc
        /// <summary>
        /// Something changed in the list of saves (one was saved, created, deleted, renamed...). The place to refresh a save menu.
        /// </summary>
        #endregion
        public static event Action OnSavesChanged;

        #region XML doc
        /// <summary>
        /// A save file was damaged when loading it, and a backup was loaded instead.
        /// </summary>
        #endregion
        public static event Action<SaveEntry, SaveBackup> OnRecoveredFromBackup;

        #region XML doc
        /// <summary>
        /// Every time the info of a save is about to be written. Add your own data to it (an alternative to SaveFileInfo.OnSaving for code that doesn't live there).
        /// </summary>
        #endregion
        public static event Action<SaveFileInfo> OnPopulatingInfo;
        #endregion



        #region Inspector
        [Title("Save Files")]
        [Tooltip("Dynamic: the player can create as many save files as they want.\nFixed Slots: the game has a set amount of save slots (like 3).")]
        [SerializeField] private SaveFileMode mode = SaveFileMode.Dynamic;

        [Tooltip("How many save slots the player has.")]
        [SerializeField, Min(1), ShowIf(nameof(IsFixedMode))] private int slotCount = 3;


        [Title("File Paths")]
        [SerializeField, Required] private CustomFilePathsAsset filePathsAsset;

        [SerializeField] private string saveFilesPath_ID = "Saves";
        [SerializeField] private string backupFilesPath_ID = "Backups";


        [Title("Backups")]
        [Tooltip("Backs up the existing save file right before it gets overwritten.")]
        [SerializeField] private bool backupBeforeSave = true;

        [Tooltip("How many backups are kept for each save file, the oldest ones are deleted first. 0 turns backups off.")]
        [SerializeField, Min(0), FormerlySerializedAs("backupFilesAmmount")] private int backupsToKeep = 5;

        [Tooltip("Automatic backups are skipped if the newest backup is younger than this. Stops a game that saves often from pushing every older backup out. 0 = back up on every save.")]
        [SerializeField, Min(0)] private float minSecondsBetweenBackups = 120f;

        [Tooltip("If a save file is damaged when it gets loaded, load the newest backup that works instead.")]
        [SerializeField] private bool recoverFromBackupIfCorrupt = true;


        [Title("Safety")]
        [Tooltip("If any saving info fails while the scene is being snapshotted, the save file isn't written. Writing it would replace a good save with stale or half filled data. Turn this off to save whatever could be gathered anyway.")]
        [SerializeField] private bool abortSaveIfInfoFails = true;


        [Title("Auto Save")]
        [Tooltip("Snapshots the scene into memory when the scene changes, so the next scene can restore it. Nothing gets written to disk.")]
        [SerializeField] private bool localSaveOnSceneChange = true;

        [Tooltip("Also writes the save file when the scene changes.")]
        [SerializeField] private bool fileSaveOnSceneChange = false;

        [Tooltip("Writes the save file when the game closes.")]
        [SerializeField] private bool fileSaveOnQuit = false;

        [Tooltip("Writes the save file every X seconds. 0 = off.")]
        [SerializeField, Min(0)] private float autoSaveIntervalSeconds = 0f;


        [Title("Data Hooks")]
        [Tooltip("Runs right before the save data (json) is written to disk. Change payload.text to change what gets written (encrypt, compress...).")]
        [SerializeField] private SavePayloadEvent onSaveDataWrite = new SavePayloadEvent();

        [Tooltip("Runs right after the save data is read from disk. Change payload.text back into the json (decrypt, decompress...).")]
        [SerializeField] private SavePayloadEvent onSaveDataRead = new SavePayloadEvent();

        [Tooltip("Same as Save Data Write, but for the info of a save (name, thumbnail flag, your own fields). This is what save menus read.")]
        [SerializeField] private SavePayloadEvent onInfoWrite = new SavePayloadEvent();

        [Tooltip("Same as Save Data Read, but for the info of a save.")]
        [SerializeField] private SavePayloadEvent onInfoRead = new SavePayloadEvent();

        [Tooltip("Refuse to save or load the save data unless a listener changed payload.text. Stops the data from being written unencrypted (or an unencrypted file from being loaded) if a listener is missing or failed.")]
        [SerializeField] private bool requireDataTransform = false;

        [Tooltip("Same as Require Data Transform, but for the info of a save.")]
        [SerializeField] private bool requireInfoTransform = false;
        #endregion



        #region Internal
        private const string SlotIdPrefix = "slot_";


        private SaveStorage storage;

        private readonly List<SaveEntry> entries = new List<SaveEntry>();

        private List<ISavingInfo> savingInfoScrs;

        private bool inValidScene = true;

        private bool currentIsActive;
        private double pendingPlayTime;
        private float autoSaveTimer;


        private bool IsFixedMode => mode == SaveFileMode.FixedSlots;

        private bool CanAutoSave => Current != null && currentIsActive;


        #region XML doc
        /// <summary>
        /// Every save the player can pick from. In Fixed Slots mode: one entry per slot, in order (empty slots have hasData = false).
        /// In Dynamic mode: only saves that exist, the most recently saved first.
        /// </summary>
        #endregion
        public IReadOnlyList<SaveEntry> Saves => entries;

        #region XML doc
        /// <summary>
        /// The save that saving and loading (without an id) works on. Null if none is selected.
        /// </summary>
        #endregion
        public SaveEntry Current { get; private set; }

        public SaveFileMode Mode => mode;

        #region XML doc
        /// <summary>
        /// How many slots there are in Fixed Slots mode. -1 in Dynamic mode (there's no limit).
        /// </summary>
        #endregion
        public int SlotCount => IsFixedMode ? slotCount : -1;


        #region XML doc
        /// <summary>
        /// The data hooks, for adding listeners from code (they can also be assigned in the inspector). Each one receives a SavePayload whose text can be replaced.
        /// The saves listed when the manager wakes up are read before any listener added from code exists, so call RefreshSaves after adding yours.
        /// </summary>
        #endregion
        public SavePayloadEvent SaveDataWrite => onSaveDataWrite;
        public SavePayloadEvent SaveDataRead => onSaveDataRead;
        public SavePayloadEvent InfoWrite => onInfoWrite;
        public SavePayloadEvent InfoRead => onInfoRead;
        #endregion
        #endregion




        #region Main
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                SavingLog.Warning("Multiple SavingManagers detected. Destroying the newest one.");
                Destroy(gameObject);

                return;
            }

            Instance = this;


            if (!InitializeStorage()) return;


            savingInfoScrs = new List<ISavingInfo>(GetComponents<ISavingInfo>());
            CheckIdentifiers();

            inValidScene = IsValidScene(SceneLoader.GetCurrentSceneName());


            RefreshSaves();

            if (IsFixedMode) SetCurrent(GetSlot(0), false);


            SceneLoader.OnSceneLoaded += OnEverySceneLoaded;

            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            SceneLoader.OnSceneLoadRequested += OnSceneChangeRequested;
        }

        private bool InitializeStorage()
        {
            if (filePathsAsset == null)
            {
                SavingLog.Error("The SavingManager has no CustomFilePathsAsset assigned, saving is disabled.");

                return false;
            }


            string savesPath = filePathsAsset.GetPath(saveFilesPath_ID);
            string backupsPath = filePathsAsset.GetPath(backupFilesPath_ID);

            if (string.IsNullOrEmpty(savesPath) || string.IsNullOrEmpty(backupsPath))
            {
                SavingLog.Error($"Couldn't find the '{saveFilesPath_ID}' and '{backupFilesPath_ID}' paths in the CustomFilePathsAsset, saving is disabled.");

                return false;
            }


            try
            {
                storage = new SaveStorage(savesPath, backupsPath);

                Directory.CreateDirectory(storage.savesRoot);
            }
            catch (Exception e)
            {
                storage = null;
                SavingLog.Error($"Couldn't set up the saves folder, saving is disabled. {e}");

                return false;
            }

            return true;
        }

        private void CheckIdentifiers()
        {
            HashSet<string> seen = new HashSet<string>();

            foreach (ISavingInfo savingInfo in savingInfoScrs)
                if (!seen.Add(savingInfo.Identifier))
                    SavingLog.Error($"More than one saving info uses the identifier '{savingInfo.Identifier}', they will overwrite each other's data. Each DTO needs a unique identifier.");
        }


        private void OnDestroy()
        {
            if (Instance != this) return;


            SceneLoader.OnSceneLoaded -= OnEverySceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneLoader.OnSceneLoadRequested -= OnSceneChangeRequested;

            foreach (SaveEntry entry in entries)
                entry.info?.ReleaseThumbnail();


            Instance = null;

            OnSaved = null;
            OnLoaded = null;
            OnSaveDeleted = null;
            OnCurrentChanged = null;
            OnSavesChanged = null;
            OnRecoveredFromBackup = null;
            OnPopulatingInfo = null;
        }

        private void OnApplicationQuit()
        {
            if (storage == null || !fileSaveOnQuit) return;

            AutoSave();
        }


        private void Update()
        {
            if (storage == null || !CanAutoSave || !inValidScene) return;


            float delta = Time.unscaledDeltaTime;

            pendingPlayTime += delta;

            if (autoSaveIntervalSeconds <= 0f) return;

            autoSaveTimer += delta;

            if (autoSaveTimer >= autoSaveIntervalSeconds)
            {
                autoSaveTimer = 0f;

                AutoSave();
            }
        }
        #endregion



        #region Logic

        #region Scene
        #region XML doc
        /// <summary>
        /// Logic executed everytime a new scene is loaded.
        /// </summary>
        #endregion
        private void OnEverySceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (!IsValidScene(scene.name)) return;

            LoadLocalInfo(scene.name);
        }

        private void OnActiveSceneChanged(Scene previous, Scene next) => inValidScene = IsValidScene(next.name);


        // "_" is the scene being requested, not the one to snapshot: we still want the CURRENT (about to be left) scene, same as before.
        private void OnSceneChangeRequested(string _)
        {
            if (!localSaveOnSceneChange && !fileSaveOnSceneChange) return;

            bool snapshotOk = SaveLocalInfo(SceneLoader.GetCurrentSceneName());

            if (fileSaveOnSceneChange && CanAutoSave && CanWriteAfterSnapshot(snapshotOk)) SaveToFile(Current.id);
        }
        #endregion



        #region Local info
        #region XML doc
        /// <summary>
        /// Triggers all saving info scripts to save their info from the scene into memory (temporary, nothing is written to disk).
        /// </summary>
        /// <param name="sceneName">Name of the current scene.</param>
        /// <returns>False if a saving info failed (the errors are logged). True if everything worked, or if there was nothing to do in this scene.</returns>
        #endregion
        public bool SaveLocalInfo(string sceneName)
        {
            if (savingInfoScrs == null) return false;

            if (!IsValidScene(sceneName)) return true;


            bool allSucceeded = true;

            foreach (ISavingInfo savingInfo in savingInfoScrs)
            {
                try { savingInfo.SaveInfo(sceneName); }
                catch (Exception e)
                {
                    allSucceeded = false;

                    SavingLog.Error($"'{savingInfo.Identifier}' failed to save its info: {e}");
                }
            }

            return allSucceeded;
        }

        private bool CanWriteAfterSnapshot(bool snapshotOk)
        {
            if (snapshotOk || !abortSaveIfInfoFails) return true;

            SavingLog.Error("The save file was not written because a saving info failed to save its info (see the errors above). Fix it, or turn off 'Abort Save If Info Fails' to save what could be gathered.");

            return false;
        }

        #region XML doc
        /// <summary>
        /// Triggers all saving info scripts to load their info from memory into the scene.
        /// </summary>
        /// <param name="sceneName">Name of the current scene.</param>
        #endregion
        public void LoadLocalInfo(string sceneName)
        {
            if (savingInfoScrs == null || !IsValidScene(sceneName)) return;

            foreach (ISavingInfo savingInfo in savingInfoScrs)
            {
                try { savingInfo.LoadInfo(sceneName); }
                catch (Exception e) { SavingLog.Error($"'{savingInfo.Identifier}' failed to load its info: {e}"); }
            }
        }


        private void AutoSave()
        {
            if (!CanAutoSave) return;

            bool snapshotOk = SaveLocalInfo(SceneLoader.GetCurrentSceneName());

            if (CanWriteAfterSnapshot(snapshotOk)) SaveToFile(Current.id);
        }
        #endregion



        #region Save list
        #region XML doc
        /// <summary>
        /// Reads the list of saves from disk again. Only needed if files changed outside of the game, the manager keeps the list up to date by itself.
        /// </summary>
        #endregion
        public void RefreshSaves()
        {
            if (!Ready()) return;


            string currentId = Current?.id;

            foreach (SaveEntry old in entries) old.info?.ReleaseThumbnail();

            entries.Clear();


            try
            {
                if (IsFixedMode)
                {
                    for (int i = 0; i < slotCount; i++)
                        entries.Add(LoadEntry(SlotId(i), i));

                    WarnAboutHiddenSaves();
                }
                else
                {
                    foreach (string id in storage.ListSaveIds(IsIntactText))
                        entries.Add(LoadEntry(id, -1));
                }
            }
            catch (Exception e) { SavingLog.Error($"Couldn't read the list of saves: {e}"); }

            SortEntries();


           if (currentId != null)
            {
                SaveEntry match = entries.Find(e => e.id == currentId);

                if (match != null) Current = match;
                else if (Current != null && Current.hasData) SetCurrent(null, false);
            }

            OnSavesChanged?.Invoke();
        }

        private void WarnAboutHiddenSaves()
        {
            foreach (string id in storage.ListSaveIds(IsIntactText))
                if (!IsListedSlotId(id))
                    SavingLog.Warning($"'{id}' has a save file, but Fixed Slots mode only lists the {slotCount} slots ({SlotId(0)} to {SlotId(slotCount - 1)}). It is kept on disk but not listed.");
        }

        private bool IsListedSlotId(string id)
        {
            if (!id.StartsWith(SlotIdPrefix, StringComparison.Ordinal)) return false;

            if (!int.TryParse(id.Substring(SlotIdPrefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int number)) return false;

            return number >= 1 && number <= slotCount && id == SlotId(number - 1);
        }


        private SaveEntry LoadEntry(string id, int slotIndex)
        {
            SaveEntry entry = new SaveEntry(id, slotIndex);

            entry.hasData = storage.SaveExists(id);

            if (entry.hasData) entry.info = ReadInfo(id);

            return entry;
        }

        private SaveFileInfo ReadInfo(string id)
        {
            SaveFileInfo info = null;
            bool unreadable = false;

            try
            {
                string path = storage.InfoPath(id);

                if (File.Exists(path))
                {
                    info = DecodeInfo(id, SaveStorage.ReadText(path));

                    unreadable = info == null;
                }
            }
            catch (Exception e)
            {
                unreadable = true;

                SavingLog.Warning($"The info of the save '{id}' couldn't be read, using defaults instead. The file is kept as {SaveStorage.InfoFileName}.corrupt if the save gets written again. {e.Message}");
            }


            if (info == null)
            {
                info = new SaveFileInfo();

                info.createdUtc = info.lastSavedUtc = File.GetLastWriteTimeUtc(storage.SavePath(id));
                info.infoUnreadable = unreadable;
            }

            AttachThumbnailLoader(id, info);

            return info;
        }


        private void SortEntries()
        {
            if (IsFixedMode) entries.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
            else entries.Sort((a, b) =>
            {
                int byDate = b.lastSavedUtc.CompareTo(a.lastSavedUtc);

                return byDate != 0 ? byDate : string.CompareOrdinal(a.id, b.id);
            });
        }


        private static string SlotId(int index) => SlotIdPrefix + (index + 1);


        #region XML doc
        /// <summary>
        /// Gets a save by its id. Null if there isn't one.
        /// </summary>
        #endregion
        public SaveEntry GetEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (Current != null && Current.id == id) return Current;

            return entries.Find(e => e.id == id);
        }

        #region XML doc
        /// <summary>
        /// Gets a slot (Fixed Slots mode). 0 is the first slot. Null if there isn't one.
        /// </summary>
        #endregion
        public SaveEntry GetSlot(int index) => IsFixedMode ? entries.Find(e => e.slotIndex == index) : null;

        #region XML doc
        /// <summary>
        /// Gets the save that was saved last (what a "Continue" button would load). Null if there are no saves.
        /// </summary>
        #endregion
        public SaveEntry GetMostRecent()
        {
            SaveEntry best = null;

            foreach (SaveEntry entry in entries)
                if (entry.hasData && (best == null || entry.lastSavedUtc > best.lastSavedUtc)) best = entry;

            return best;
        }
        #endregion



        #region Selection
        #region XML doc
        /// <summary>
        /// Makes a save the one that saving and loading (without an id) works on.
        /// </summary>
        /// <returns>Whether there is a save with that id.</returns>
        #endregion
        public bool SelectSave(string id)
        {
            if (!Ready()) return false;

            SaveEntry entry = GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning($"There's no save with the id '{id}'.");

                return false;
            }

            SetCurrent(entry, false);

            return true;
        }

        #region XML doc
        /// <summary>
        /// Fixed Slots mode: selects a slot. 0 is the first slot.
        /// </summary>
        #endregion
        public bool SelectSlot(int index)
        {
            if (!Ready()) return false;

            SaveEntry entry = GetSlot(index);

            if (entry == null)
            {
                SavingLog.Warning($"There's no slot {index} (slots go from 0 to {slotCount - 1}, and only exist in Fixed Slots mode).");

                return false;
            }

            SetCurrent(entry, false);

            return true;
        }

        #region XML doc
        /// <summary>
        /// Selects the next save. It goes by a stable order (slot order, or oldest save first in Dynamic mode), which doesn't change when a save is written.
        /// </summary>
        #endregion
        public void SelectNext() => Cycle(1);
        public void SelectPrevious() => Cycle(-1);

        public void ClearSelection()
        {
            if (Ready()) SetCurrent(null, false);
        }


        private void Cycle(int direction)
        {
            if (!Ready() || entries.Count == 0) return;


            List<SaveEntry> order = new List<SaveEntry>(entries);

            if (!IsFixedMode) order.Sort(CompareByCreation);


            int index = Current != null ? order.IndexOf(Current) : -1;

            if (index < 0) index = direction > 0 ? 0 : order.Count - 1;
            else index = (index + direction + order.Count) % order.Count;

            SetCurrent(order[index], false);
        }

        private static int CompareByCreation(SaveEntry a, SaveEntry b)
        {
            DateTime createdA = a.info != null ? a.info.createdUtc : default;
            DateTime createdB = b.info != null ? b.info.createdUtc : default;

            int byDate = createdA.CompareTo(createdB);

            return byDate != 0 ? byDate : string.CompareOrdinal(a.id, b.id);
        }


        private void SetCurrent(SaveEntry entry, bool activate)
        {
            if (Current == entry)
            {
                if (activate && entry != null) currentIsActive = true;

                return;
            }


            Current = entry;
            currentIsActive = activate && entry != null;

            pendingPlayTime = 0d;
            autoSaveTimer = 0f;

            OnCurrentChanged?.Invoke(Current);
        }
        #endregion



        #region Misc
        private bool Ready()
        {
            if (storage != null) return true;

            SavingLog.Error("The SavingManager isn't set up (see the errors from when it started).");

            return false;
        }


        #if UNITY_EDITOR
        [Button]
        private void OpenSavesFolder()
        {
            if (filePathsAsset == null) return;

            string path = filePathsAsset.GetPath(saveFilesPath_ID);

            if (string.IsNullOrEmpty(path)) return;


            Directory.CreateDirectory(path);

            UnityEditor.EditorUtility.RevealInFinder(path);
        }
        #endif
        #endregion

        #endregion
    }





    #region SaveFileMode
    public enum SaveFileMode
    {
        Dynamic,
        FixedSlots
    }
    #endregion
}
