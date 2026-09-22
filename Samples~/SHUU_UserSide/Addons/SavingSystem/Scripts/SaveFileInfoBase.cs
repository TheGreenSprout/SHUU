using System;
using Newtonsoft.Json;
using UnityEngine;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// The part of a save file's info that the SavingManager fills in by itself. Don't edit this one, add your own data to SaveFileInfo instead.
    /// </summary>
    #endregion
    public abstract class SaveFileInfoBase
    {
        #region Variables
        #region XML doc
        /// <summary>
        /// The name the player sees. Change it with SavingManager.RenameSave.
        /// </summary>
        #endregion
        public string displayName = "";

        public DateTime createdUtc;
        public DateTime lastSavedUtc;

        #region XML doc
        /// <summary>
        /// How long this save has been played, in seconds. Counts while the save is active and the current scene isn't one of the SavingManager's excluded scenes.
        /// </summary>
        #endregion
        public double playTimeSeconds;

        public string gameVersion = "";

        public bool hasThumbnail;



        [JsonIgnore] internal bool infoUnreadable;



        #region Thumbnail
        [JsonIgnore] private Texture2D _thumbnail;
        [JsonIgnore] private bool ownsThumbnail;
        [JsonIgnore] internal bool thumbnailChanged;

        [JsonIgnore] internal Func<Texture2D> thumbnailLoader;


        #region XML doc
        /// <summary>
        /// The image shown for this save. Reading it loads it from disk the first time (null if the save has none).
        /// Assigning a texture makes the next save write it as the thumbnail (it has to be a readable, uncompressed texture, like a screenshot).
        /// The SavingManager cleans up the textures it loaded by itself, textures you assign are yours to destroy.
        /// </summary>
        #endregion
        [JsonIgnore]
        public Texture2D thumbnail
        {
            get
            {
                if (_thumbnail == null && hasThumbnail && thumbnailLoader != null)
                {
                    _thumbnail = thumbnailLoader();
                    ownsThumbnail = _thumbnail != null;
                }

                return _thumbnail;
            }

            set => SetThumbnail(value, false);
        }

        internal void SetThumbnail(Texture2D texture, bool owned)
        {
            ReleaseThumbnail();

            _thumbnail = texture;
            ownsThumbnail = owned && texture != null;
            thumbnailChanged = texture != null;
        }
        internal void ReleaseThumbnail()
        {
            if (ownsThumbnail && _thumbnail != null) UnityEngine.Object.Destroy(_thumbnail);

            _thumbnail = null;
            ownsThumbnail = false;
        }
        #endregion

        #endregion
    }
}
