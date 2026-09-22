using UnityEngine;

using SHUU.Utils.Helpers.ScriptableObjects;
using SHUU.UserSide.Commons;

namespace SHUU.InnerWorkings
{
    public static class SHUU_PackageUtils
    {
        #region Variables
        private static CustomFilePathsAsset _pathsAsset = null;
        public static CustomFilePathsAsset PathsAsset
        {
            get
            {
                if (_pathsAsset == null) _pathsAsset = Resources.Load<CustomFilePathsAsset>("SHUUResources/SHUU_CustomFilePathsAsset");

                return _pathsAsset;
            }
        }


        private static ScriptableObjectLoader _scriptableObjectLoader = null;
        public static ScriptableObjectLoader ScriptableObjectLoader
        {
            get
            {
                if (_pathsAsset == null) _scriptableObjectLoader = Resources.Load<ScriptableObjectLoader>("SHUUResources/ScriptableObjectLoader_Asset");

                return _scriptableObjectLoader;
            }
        }
        #endregion




        #region Logic
        public static string GetPath(string id, string endPoint = null) => PathsAsset.GetPath(id, endPoint, true);

        
        #if UNITY_EDITOR
        public static void TrackScriptableObject(ScriptableObject obj) => ScriptableObjectLoader.Track(obj);
        #endif
        #endregion
    }
}
