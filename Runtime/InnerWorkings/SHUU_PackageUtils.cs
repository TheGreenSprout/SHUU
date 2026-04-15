using UnityEngine;

namespace SHUU.InnerWorkings
{
    public static class SHUU_PackageUtils
    {
        public static string packageFilePath => Application.persistentDataPath + "/SHUU";
        
        public static string PackageFilePath(params string[] additionalPaths)
        {
            string path = packageFilePath;

            foreach (var additional in additionalPaths) path = path + "/" + additional;

            return path;
        }
    }
}
