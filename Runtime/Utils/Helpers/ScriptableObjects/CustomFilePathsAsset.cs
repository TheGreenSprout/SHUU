using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SHUU.Utils.Helpers.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CustomFilePathsAsset", menuName = "SHUU/Utils/Helpers/CustomFilePathsAsset")]
    public class CustomFilePathsAsset : ScriptableObject
    {
        #region Variables
        public List<CustomFilePathData> data = new();

        private Dictionary<string, CustomFilePathData> lookup;
        #endregion




        #region Main
        private void OnEnable()
        {
            if (data == null) return;


            lookup = new();

            foreach (var item in data)
            {
                if (item == null || string.IsNullOrEmpty(item.id)) continue;

                if (lookup.ContainsKey(item.id)) Debug.LogError($"Duplicate path ID: {item.id}");


                lookup[item.id] = item;
            }
        }
        #endregion



        #region Logic
        public string GetPath(string id, string endPoint = null, bool ensureExists = false, bool? application_persistentDataPath_override = null, HashSet<string> visited = null)
        {
            if (lookup == null) OnEnable();


            visited ??= new HashSet<string>();

            if (!visited.Add(id))
            {
                Debug.LogError($"Cyclic path reference detected: {id}");
                
                return null;
            }


            if (!lookup.TryGetValue(id, out var item))
            {
                Debug.LogError($"Path ID not found: {id}");

                return null;
            }


            string path = item.GetPath(this, endPoint, application_persistentDataPath_override, visited);

            if (!string.IsNullOrEmpty(path) && ensureExists)
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            }


            visited.Remove(id);


            return path;
        }
        #endregion
    }



    
    #region Data class
    [Serializable]
    public class CustomFilePathData
    {
        public string id;


        public string path;

        public bool application_persistentDataPath;

        public string[] startPoints;




        public CustomFilePathData() => application_persistentDataPath = true;



        public string GetPath(CustomFilePathsAsset holder, string endPoint = null, bool? application_persistentDataPath_override = null, HashSet<string> visited = null)
        {
            if (endPoint == null) return GetPath_Internal(holder, application_persistentDataPath_override, visited);
            else return GetPath_Internal(holder, endPoint, application_persistentDataPath_override, visited);
        }


        private string GetPath_Internal(CustomFilePathsAsset holder, bool? application_persistentDataPath_override = null, HashSet<string> visited = null)
        {
            string cleanPath = path.TrimStart('/', '\\');

            string startPoint = GetStartPoint(holder, visited);
            if (!string.IsNullOrWhiteSpace(startPoint)) cleanPath = Path.Combine(startPoint, cleanPath);

            if (application_persistentDataPath_override != null)
                return application_persistentDataPath_override.Value ? Path.Combine(Application.persistentDataPath, cleanPath) : cleanPath;
            else return application_persistentDataPath ? Path.Combine(Application.persistentDataPath, cleanPath) : cleanPath;
        }
        private string GetPath_Internal(CustomFilePathsAsset holder, string endPoint, bool? application_persistentDataPath_override = null, HashSet<string> visited = null)
        {
            endPoint = endPoint.TrimStart('/', '\\');

            return Path.Combine(GetPath_Internal(holder, application_persistentDataPath_override, visited), endPoint);
        }

        private string GetStartPoint(CustomFilePathsAsset holder, HashSet<string> visited = null)
        {
            if (startPoints == null || startPoints.Length == 0) return null;


            List<string> startPointList = new(startPoints);

            for (int i = 0; i < startPointList.Count; i++)
                startPointList[i] = holder.GetPath(startPointList[i], null, false, false, visited);


            return Path.Combine(startPointList.ToArray());
        }
    }
    #endregion
}
