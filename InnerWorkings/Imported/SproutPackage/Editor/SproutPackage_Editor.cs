#if UNITY_EDITOR
using UnityEngine;

using SETB;

using static SETB.HandyEditorFunctions;

namespace SproutPackage._Editor
{
    public static class SproutPackage_Editor
    {
        #region Variables
        private static Texture2D _idleImage = null;
        public static Texture2D idleImage
        {
            get
            {
                if (_idleImage == null) _idleImage = Resources.Load<Texture2D>("SproutPackage_Resources/IdleImage");

                return _idleImage;
            }
        }

        private static Texture2D _successImage = null;
        public static Texture2D successImage
        {
            get
            {
                if (_successImage == null) _successImage = Resources.Load<Texture2D>("SproutPackage_Resources/SuccessImage");

                return _successImage;
            }
        }

        private static Texture2D _restoreImage = null;
        public static Texture2D restoreImage
        {
            get
            {
                if (_restoreImage == null) _restoreImage = Resources.Load<Texture2D>("SproutPackage_Resources/RestoreImage");

                return _restoreImage;
            }
        }

        private static Texture2D _deleteImage = null;
        public static Texture2D deleteImage
        {
            get
            {
                if (_deleteImage == null) _deleteImage = Resources.Load<Texture2D>("SproutPackage_Resources/DeleteImage");

                return _deleteImage;
            }
        }

        private static Texture2D _errorImage = null;
        public static Texture2D errorImage
        {
            get
            {
                if (_errorImage == null) _errorImage = Resources.Load<Texture2D>("SproutPackage_Resources/ErrorImage");

                return _errorImage;
            }
        }


        private static AudioClip _successClip = null;
        public static AudioClip successClip
        {
            get
            {
                if (_successClip == null) _successClip = Resources.Load<AudioClip>("SproutPackage_Resources/SuccessClip");

                return _successClip;
            }
        }

        private static AudioClip _errorClip = null;
        public static AudioClip errorClip
        {
            get
            {
                if (_errorClip == null) _errorClip = Resources.Load<AudioClip>("SproutPackage_Resources/ErrorClip");

                return _errorClip;
            }
        }



        public enum PopupType
        {
            Idle,
            Success,
            Restore,
            Delete,
            Error
        }
        #endregion




        #region Common methods
        public static void SproutPopup(string message, PopupType type, float width = 250f, float height = 250f, float imageWidth = 128f, float imageHeight = 128f, PopupOptions options = null)
        {
            if (options == null) options = new() { Locked = true };
            options.ImageWidth = imageWidth;
            options.ImageHeight = imageHeight;

            switch (type)
            {
                case PopupType.Idle:
                    options.Image = options.Image ?? idleImage;
                    options.Silent = true;
                    break;

                case PopupType.Success:
                    options.Image = options.Image ?? successImage;
                    options.Sound = options.Sound ?? successClip;
                    break;

                case PopupType.Restore:
                    options.Image = options.Image ?? restoreImage;
                    options.Sound = options.Sound ?? successClip;
                    break;

                case PopupType.Delete:
                    options.Image = options.Image ?? deleteImage;
                    options.Sound = options.Sound ?? successClip;
                    break;

                case PopupType.Error:
                    options.Image = options.Image ?? errorImage;
                    options.Sound = options.Sound ?? errorClip;
                    break;
            }

            PopupWindow(message, width, height, options);
        }
        #endregion
    }
}
#endif
