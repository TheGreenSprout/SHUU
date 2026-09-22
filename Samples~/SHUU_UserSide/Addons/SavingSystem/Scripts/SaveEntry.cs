using System;

using SHUU.UserSide.Addons.SavingSystem.ForUser;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// One save file, as far as a save menu is concerned. In Fixed Slots mode every slot has an entry (empty ones have <see cref="hasData"/> = false),
    /// in Dynamic mode only saves that exist do.
    /// </summary>
    #endregion
    public sealed class SaveEntry
    {
        #region Variables
        #region XML doc
        /// <summary>
        /// Stable identifier of this save (its folder name). Never changes, unlike the display name.
        /// </summary>
        #endregion
        public string id { get; }



        #region XML doc
        /// <summary>
        /// Index of the slot in Fixed Slots mode (0 is the first slot). -1 in Dynamic mode.
        /// </summary>
        #endregion
        public int slotIndex { get; }

        public bool isSlot => slotIndex >= 0;


        #region XML doc
        /// <summary>
        /// Whether there is a save file on disk. Always true in Dynamic mode, except for a save that was just created and hasn't been saved yet.
        /// </summary>
        #endregion
        public bool hasData { get; internal set; }

        #region XML doc
        /// <summary>
        /// The information about this save (name, thumbnail, your own fields...). Null for an empty slot, use hasData to know if a save exists.
        /// </summary>
        #endregion
        public SaveFileInfo info { get; internal set; }


        public string displayName
        {
            get
            {
                if (info != null && !string.IsNullOrWhiteSpace(info.displayName)) return info.displayName;

                return isSlot ? $"Slot {slotIndex + 1}" : id;
            }
        }

        public DateTime lastSavedUtc => info != null ? info.lastSavedUtc : default;
        #endregion




        #region Main
        internal SaveEntry(string id, int slotIndex)
        {
            this.id = id;
            this.slotIndex = slotIndex;
        }
        #endregion
    }
}
