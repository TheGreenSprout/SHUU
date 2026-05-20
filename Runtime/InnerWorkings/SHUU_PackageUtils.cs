using UnityEngine;

using SHUU.Utils.Helpers.ScriptableObjects;

namespace SHUU.InnerWorkings
{
    public static class SHUU_PackageUtils
    {
        #region Variables
        private const string resourcesPath = "SHUU_Runtime_Resources/SHUU_CustomFilePathsAsset";



        private static CustomFilePathsAsset _pathsAsset = null;

        public static CustomFilePathsAsset pathsAsset
        {
            get
            {
                if (_pathsAsset == null) _pathsAsset = Resources.Load<CustomFilePathsAsset>(resourcesPath);

                return _pathsAsset;
            }
        }
        #endregion




        #region Logic
        public static string GetPath(string id, string endPoint = null) => pathsAsset.GetPath(id, endPoint, true);
        #endregion
    }
}
