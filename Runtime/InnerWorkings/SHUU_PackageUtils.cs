using UnityEngine;

using SHUU.Utils.Helpers.ScriptableObjects;
using SHUU.UserSide.Commons;

namespace SHUU.InnerWorkings
{
    public static class SHUU_PackageUtils
    {
        #region Variables
        private static CustomFilePathsAsset _pathsAsset = null;
        public static CustomFilePathsAsset pathsAsset
        {
            get
            {
                if (_pathsAsset == null) _pathsAsset = Resources.Load<CustomFilePathsAsset>("SHUU_Runtime_Resources/SHUU_CustomFilePathsAsset");

                return _pathsAsset;
            }
        }


        private static ScriptableObjectLoader _scriptableObjectLoader = null;
        public static ScriptableObjectLoader scriptableObjectLoader
        {
            get
            {
                if (_pathsAsset == null) _scriptableObjectLoader = Resources.Load<ScriptableObjectLoader>("SHUU/InnerWorkings/ScriptableObjectLoader_Asset");

                return _scriptableObjectLoader;
            }
        }
        #endregion




        #region Logic
        public static string GetPath(string id, string endPoint = null) => pathsAsset.GetPath(id, endPoint, true);

        
        #if UNITY_EDITOR
        public static void TrackScriptableObject(ScriptableObject obj) => scriptableObjectLoader.Track(obj);
        #endif
        #endregion
    }
}
