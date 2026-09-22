using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

using SHUU.UserSide.Addons.SavingSystem.ForUser;
using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.SavingSystem
{
    public partial class SavingManager
    {
        #region Logic

        #region Create
        #region XML doc
        /// <summary>
        /// Starts a new save and selects it. Nothing is written to disk until the game is saved.
        /// Dynamic mode: makes a brand new save. Fixed Slots mode: uses the given slot, or the first empty one.
        /// </summary>
        /// <param name="displayName">The name the player sees. A default one is used if empty.</param>
        /// <param name="slotIndex">Fixed Slots mode only: which slot to use (0 is the first). -1 picks the first empty slot.</param>
        /// <returns>The new save. Null if it couldn't be created (all slots are taken, or the slot already has a save: delete it first).</returns>
        #endregion
        public SaveEntry CreateSave(string displayName = null, int slotIndex = -1)
        {
            if (!Ready()) return null;


            SaveEntry entry;

            if (IsFixedMode)
            {
                entry = slotIndex >= 0 ? GetSlot(slotIndex) : entries.Find(e => !e.hasData);

                if (entry == null)
                {
                    SavingLog.Warning(slotIndex >= 0 ? $"There's no slot {slotIndex}." : "Every slot already has a save. Delete one first.");

                    return null;
                }

                if (entry.hasData)
                {
                    SavingLog.Warning($"'{entry.displayName}' already has a save. Delete it first, or save into it to overwrite it.");

                    return null;
                }
            }
            else entry = new SaveEntry(Guid.NewGuid().ToString("N"), -1);


            entry.info = NewInfo(entry, displayName);

            SetCurrent(entry, true);

            return entry;
        }


        private SaveFileInfo NewInfo(SaveEntry entry, string displayName)
        {
            SaveFileInfo info = new SaveFileInfo
            {
                displayName = string.IsNullOrWhiteSpace(displayName) ? DefaultName(entry) : displayName.Trim(),
                createdUtc = DateTime.UtcNow,
            };
            info.lastSavedUtc = info.createdUtc;
            info.gameVersion = Application.version;

            AttachThumbnailLoader(entry.id, info);

            return info;
        }

        private string DefaultName(SaveEntry entry)
        {
            if (entry.isSlot) return $"Slot {entry.slotIndex + 1}";

            int number = entries.Count + 1;
            while (NameInUse($"Save {number}")) number++;

            return $"Save {number}";
        }

        private bool NameInUse(string name) => entries.Exists(e => e.displayName == name);
        #endregion



        #region Save
        #region XML doc
        /// <summary>
        /// Saves the game into a save file: snapshots the scene into memory and writes it all to disk.
        /// </summary>
        /// <param name="id">Which save to write to. Null uses the current one (Dynamic mode makes a new save if there's none).</param>
        /// <returns>Whether the save was written.</returns>
        #endregion
        public bool Save(string id = null)
        {
            if (!Ready()) return false;

            bool snapshotOk = SaveLocalInfo(SceneLoader.GetCurrentSceneName());

            if (!CanWriteAfterSnapshot(snapshotOk)) return false;

            return SaveToFile(id);
        }

        #region XML doc
        /// <summary>
        /// Writes what is in memory (see SaveLocalInfo) to a save file. Use Save to snapshot the scene first.
        /// The save file is replaced in one step, so a crash while saving can never leave a half written file.
        /// </summary>
        /// <param name="id">Which save to write to. Null uses the current one (Dynamic mode makes a new save if there's none).</param>
        /// <returns>Whether the save was written.</returns>
        #endregion
        public bool SaveToFile(string id = null)
        {
            if (!Ready()) return false;


            SaveEntry entry = ResolveForSave(id);
            if (entry == null) return false;

            bool isCurrent = entry == Current;


            string json;

            try
            {
                MasterDTO master = new MasterDTO();

                foreach (ISavingInfo savingInfo in savingInfoScrs)
                {
                    DTO_Info dto = savingInfo.ExportDTO();

                    if (dto == null)
                    {
                        SavingLog.Warning($"'{savingInfo.Identifier}' exported no data, it's left out of the save.");

                        continue;
                    }

                    master.dataDictionary[savingInfo.Identifier] = SaveSerialization.ToToken(dto);
                }

                json = EncodeData(entry.id, SaveSerialization.Serialize(master));
            }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't save '{entry.displayName}', something failed while gathering or encoding the data. Nothing was written. {e}");

                return false;
            }


            if (!IsSafeToOverwrite(entry)) return false;


            SaveFileInfo info = entry.info ?? NewInfo(entry, null);

            double addedPlayTime = isCurrent && currentIsActive ? pendingPlayTime : 0d;

            info.lastSavedUtc = DateTime.UtcNow;
            info.gameVersion = Application.version;
            info.playTimeSeconds += addedPlayTime;

            try
            {
                info.OnSaving();
                OnPopulatingInfo?.Invoke(info);
            }
            catch (Exception e) { SavingLog.Error($"Something failed while filling in the info of '{entry.displayName}', it gets saved as it is. {e}"); }


            if (backupBeforeSave) TryCreateBackup(entry.id, false);


            try { SaveStorage.WriteAtomic(storage.SavePath(entry.id), json); }
            catch (Exception e)
            {
                info.playTimeSeconds -= addedPlayTime;

                SavingLog.Error($"Couldn't write the save '{entry.displayName}': {e}");

                return false;
            }

            WriteInfo(entry.id, info);


            entry.info = info;
            entry.hasData = true;

            if (isCurrent)
            {
                currentIsActive = true;
                pendingPlayTime -= addedPlayTime;
            }

            if (!entries.Contains(entry)) entries.Add(entry);
            SortEntries();


            SavingLog.Info($"Saved '{entry.displayName}' ({entry.id}).");

            OnSaved?.Invoke(entry);
            OnSavesChanged?.Invoke();

            return true;
        }


        #region Helpers
        private SaveEntry ResolveForSave(string id)
        {
            if (id != null)
            {
                SaveEntry known = GetEntry(id);

                if (known == null) SavingLog.Warning($"There's no save with the id '{id}'.");

                return known;
            }

            if (Current != null) return Current;

            if (!IsFixedMode) return CreateSave();

            SavingLog.Warning("No save slot is selected. Select one first (SelectSlot).");

            return null;
        }


        private void WriteInfo(string id, SaveFileInfo info)
        {
            try
            {
                if (info.thumbnailChanged)
                {
                    Texture2D texture = info.thumbnail;

                    if (texture != null)
                    {
                        byte[] png = texture.EncodeToPNG();

                        if (png == null || png.Length == 0) throw new InvalidOperationException("The texture couldn't be encoded (it has to be readable and uncompressed).");

                        SaveStorage.WriteAtomic(storage.ThumbnailPath(id), png);

                        info.hasThumbnail = true;
                    }

                    info.thumbnailChanged = false;
                }
            }
            catch (Exception e) { SavingLog.Warning($"Couldn't write the thumbnail of the save '{id}'. {e.Message}"); }


            try { WriteInfoFile(id, info); }
            catch (Exception e) { SavingLog.Warning($"The save '{id}' was written, but its info couldn't be. {e.Message}"); }
        }


        private void WriteInfoFile(string id, SaveFileInfo info)
        {
            string encoded = EncodeInfo(id, info);

            if (info.infoUnreadable) storage.QuarantineInfo(id);

            SaveStorage.WriteAtomic(storage.InfoPath(id), encoded);

            info.infoUnreadable = false;
        }


        private bool IsSafeToOverwrite(SaveEntry entry)
        {
            string id = entry.id;

            try
            {
                if (!storage.TryReadSaveText(id, out string existing)) return true;


                SaveParseResult result = ParseStored(id, existing, out _, out string error);

                if (result == SaveParseResult.Incompatible)
                {
                    SavingLog.Error($"Not saving over '{entry.displayName}': the file on disk can't be read by this version of the game, so writing to it would destroy it. {error} Delete the save first if it should be replaced.");

                    return false;
                }

                if (result == SaveParseResult.Corrupt)
                {
                    storage.QuarantineSave(id);

                    SavingLog.Warning($"The save file of '{entry.displayName}' is damaged. It's kept as {SaveStorage.SaveFileName}.corrupt and replaced by this save.");
                }
            }
            catch (Exception e) { SavingLog.Warning($"Couldn't check the existing save file of '{entry.displayName}' before writing it. {e.Message}"); }

            return true;
        }
        #endregion

        #endregion



        #region Load
        #region XML doc
        /// <summary>
        /// Loads a save file into memory and then into the scene.
        /// </summary>
        /// <param name="id">Which save to load. Null uses the current one.</param>
        /// <returns>Whether the save was loaded.</returns>
        #endregion
        public bool Load(string id = null)
        {
            if (!LoadFromFileCore(id, out SaveEntry entry)) return false;

            LoadLocalInfo(SceneLoader.GetCurrentSceneName());

            OnLoaded?.Invoke(entry);

            return true;
        }

        #region XML doc
        /// <summary>
        /// Loads a save file into memory (see LoadLocalInfo to apply it to the scene, or use Load to do both).
        /// If the file is damaged it's replaced by the newest working backup (when "Recover From Backup If Corrupt" is on).
        /// </summary>
        /// <param name="id">Which save to load. Null uses the current one.</param>
        /// <returns>Whether the save was loaded.</returns>
        #endregion
        public bool LoadFromFile(string id = null)
        {
            if (!LoadFromFileCore(id, out SaveEntry entry)) return false;

            OnLoaded?.Invoke(entry);

            return true;
        }

        private bool LoadFromFileCore(string id, out SaveEntry entry)
        {
            entry = null;

            if (!Ready()) return false;


            entry = id == null ? Current : GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning(id == null ? "There's no save selected to load." : $"There's no save with the id '{id}'.");

                return false;
            }


            if (!TryReadMaster(entry, out MasterDTO master)) return false;


            foreach (ISavingInfo savingInfo in savingInfoScrs)
            {
                if (!master.dataDictionary.TryGetValue(savingInfo.Identifier, out JToken token) || token == null || token.Type == JTokenType.Null)
                {
                    SavingLog.Info($"No data found for '{savingInfo.Identifier}' in '{entry.displayName}', skipping it.");

                    continue;
                }

                try { savingInfo.ImportDTO(SaveSerialization.ToDTO(token, savingInfo.DtoType)); }
                catch (Exception e) { SavingLog.Error($"Failed to load '{savingInfo.Identifier}': {e}"); }
            }

            WarnAboutUnclaimedData(entry, master);


            SetCurrent(entry, true);

            SavingLog.Info($"Loaded '{entry.displayName}' ({entry.id}).");

            return true;
        }


        #region Helpers
        private void WarnAboutUnclaimedData(SaveEntry entry, MasterDTO master)
        {
            foreach (string key in master.dataDictionary.Keys)
            {
                bool claimed = false;

                foreach (ISavingInfo savingInfo in savingInfoScrs)
                {
                    if (savingInfo.Identifier != key) continue;

                    claimed = true;

                    break;
                }

                if (!claimed) SavingLog.Warning($"'{entry.displayName}' has saved data for '{key}', but no saving info uses that identifier (was a DTO renamed or removed?). It's ignored, and it's gone from the file the next time the save is written (older backups still have it).");
            }
        }

        private bool TryReadMaster(SaveEntry entry, out MasterDTO master)
        {
            master = null;

            string id = entry.id;

            try
            {
                storage.PromoteTempIfMainMissing(id, stored => IsIntactText(id, stored));

                if (!storage.TryReadSaveText(id, out string text))
                {
                    SavingLog.Warning($"'{entry.displayName}' has no save file.");

                    return false;
                }


                SaveParseResult result = ParseStored(id, text, out master, out string error);

                if (result == SaveParseResult.Ok) return true;

                if (result == SaveParseResult.Incompatible)
                {
                    SavingLog.Error($"'{entry.displayName}' can't be read by this version of the game, the file was not changed. {error}");

                    return false;
                }


                SavingLog.Warning($"The save file of '{entry.displayName}' is damaged. {error}");

                if (!recoverFromBackupIfCorrupt) return false;


                if (storage.TryRecoverFromBackups(id, stored => IsLoadableText(id, stored), out string recoveredText, out SaveBackup used)
                    && ParseStored(id, recoveredText, out master, out _) == SaveParseResult.Ok)
                {
                    SavingLog.Warning($"Recovered '{entry.displayName}' from a backup ({used.createdUtc:yyyy-MM-dd HH:mm:ss} UTC). The damaged file was kept as {SaveStorage.SaveFileName}.corrupt.");

                    OnRecoveredFromBackup?.Invoke(entry, used);

                    return true;
                }

                SavingLog.Error($"The save file of '{entry.displayName}' is damaged and there's no working backup to recover it from.");

                return false;
            }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't read the save '{entry.displayName}': {e}");

                return false;
            }
        }

        private SaveParseResult ParseStored(string id, string stored, out MasterDTO master, out string error)
        {
            string json;

            try { json = DecodeData(id, stored); }
            catch (Exception e)
            {
                master = null;
                error = $"The data couldn't be decoded: {e.Message}";

                return SaveParseResult.Corrupt;
            }

            return SaveSerialization.ParseMaster(json, out master, out error);
        }

        private bool IsIntactText(string id, string stored) => ParseStored(id, stored, out _, out _) != SaveParseResult.Corrupt;
        private bool IsLoadableText(string id, string stored) => ParseStored(id, stored, out _, out _) == SaveParseResult.Ok;
        #endregion

        #endregion



        #region Data hooks
        private string EncodeData(string id, string json) => RunHook(onSaveDataWrite, requireDataTransform, "save data", id, json);
        private string DecodeData(string id, string stored) => RunHook(onSaveDataRead, requireDataTransform, "save data", id, stored);

        private string EncodeInfo(string id, SaveFileInfo info) => RunHook(onInfoWrite, requireInfoTransform, "info", id, SaveSerialization.Serialize(info));
        private SaveFileInfo DecodeInfo(string id, string stored) => SaveSerialization.Deserialize<SaveFileInfo>(RunHook(onInfoRead, requireInfoTransform, "info", id, stored));


        private static string RunHook(SavePayloadEvent hook, bool required, string what, string id, string text)
        {
            SavePayload payload = new SavePayload(id, text);

            hook?.Invoke(payload);

            if (required && !payload.modified)
                throw new InvalidOperationException($"'Require Transform' is on for the {what}, but no listener changed payload.text (is the listener missing or did it fail?).");

            if (payload.text == null) throw new InvalidOperationException($"A listener left the {what} payload empty.");

            return payload.text;
        }
        #endregion



        #region Delete/Rename
        #region XML doc
        /// <summary>
        /// Deletes a save file with everything that belongs to it (info, thumbnail and backups).
        /// In Fixed Slots mode the slot stays, it's just empty afterwards.
        /// </summary>
        /// <param name="id">Which save to delete. Null uses the current one.</param>
        #endregion
        public bool DeleteSave(string id = null)
        {
            if (!Ready()) return false;


            SaveEntry entry = id == null ? Current : GetEntry(id);

            if (entry == null)
            {
                SavingLog.Warning(id == null ? "There's no save selected to delete." : $"There's no save with the id '{id}'.");

                return false;
            }


            try { storage.DeleteSave(entry.id); }
            catch (Exception e)
            {
                SavingLog.Error($"Couldn't delete the save '{entry.displayName}': {e}");

                return false;
            }


            string deletedId = entry.id;

            entry.info?.ReleaseThumbnail();

            if (entry.isSlot)
            {
                entry.hasData = false;
                entry.info = null;

                if (Current == entry)
                {
                    currentIsActive = false;
                    pendingPlayTime = 0d;
                }
            }
            else
            {
                entries.Remove(entry);

                if (Current == entry) SetCurrent(null, false);
            }


            SavingLog.Info($"Deleted the save '{deletedId}'.");

            OnSaveDeleted?.Invoke(deletedId);
            OnSavesChanged?.Invoke();

            return true;
        }


        #region XML doc
        /// <summary>
        /// Changes the name of a save (what the player sees, the id stays the same).
        /// </summary>
        #endregion
        public bool RenameSave(string id, string newName)
        {
            if (!Ready()) return false;


            SaveEntry entry = GetEntry(id);

            if (entry == null || entry.info == null)
            {
                SavingLog.Warning($"There's no save with the id '{id}'.");

                return false;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                SavingLog.Warning("A save can't have an empty name.");

                return false;
            }


            string previous = entry.info.displayName;

            entry.info.displayName = newName.Trim();

            if (entry.hasData)
            {
                try { WriteInfoFile(entry.id, entry.info); }
                catch (Exception e)
                {
                    entry.info.displayName = previous;

                    SavingLog.Error($"Couldn't rename the save '{previous}': {e}");

                    return false;
                }
            }

            OnSavesChanged?.Invoke();

            return true;
        }
        #endregion



        #region Thumbnails
        private void AttachThumbnailLoader(string id, SaveFileInfo info) => info.thumbnailLoader = () => LoadThumbnailTexture(id);

        private Texture2D LoadThumbnailTexture(string id)
        {
            try
            {
                string path = storage.ThumbnailPath(id);

                if (!File.Exists(path)) return null;


                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (!texture.LoadImage(File.ReadAllBytes(path)))
                {
                    Destroy(texture);

                    return null;
                }

                return texture;
            }
            catch (Exception e)
            {
                SavingLog.Warning($"Couldn't load the thumbnail of the save '{id}'. {e.Message}");

                return null;
            }
        }


        #region XML doc
        /// <summary>
        /// Saves the game with a screenshot of what's on screen as the thumbnail. The screenshot is taken at the end of the frame, so it includes any UI that's showing.
        /// </summary>
        /// <param name="id">Which save to write to. Null uses the current one (Dynamic mode makes a new save if there's none).</param>
        /// <param name="thumbnailHeight">Height of the thumbnail in pixels (the width follows the screen's aspect ratio).</param>
        #endregion
        public Coroutine SaveWithScreenshot(string id = null, int thumbnailHeight = 180) => StartCoroutine(SaveWithScreenshotRoutine(id, thumbnailHeight));

        private IEnumerator SaveWithScreenshotRoutine(string id, int thumbnailHeight)
        {
            if (!Ready()) yield break;


            SaveEntry entry = ResolveForSave(id);
            if (entry == null) yield break;

            yield return new WaitForEndOfFrame();


            Texture2D thumbnail = CaptureThumbnail(thumbnailHeight);

            if (thumbnail != null)
            {
                entry.info ??= NewInfo(entry, null);
                entry.info.SetThumbnail(thumbnail, true);
            }

            Save(entry.id);
        }

        private static Texture2D CaptureThumbnail(int height)
        {
            Texture2D screenshot = null;

            try
            {
                screenshot = ScreenCaptureHelper.CaptureScreenshotAsTexture();

                return screenshot != null ? ScreenCaptureHelper.ResizeTexture(screenshot, height) : null;
            }
            catch (Exception e)
            {
                SavingLog.Warning($"Couldn't take the screenshot for the thumbnail. {e.Message}");

                return null;
            }
            finally { if (screenshot != null) Destroy(screenshot); }
        }
        #endregion

        #endregion
    }
}
