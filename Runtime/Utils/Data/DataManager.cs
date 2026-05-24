using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using SFB;

using SHUU.Utils.BaseScripts;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.Data
{
    #region XML doc
    /// <summary>
    /// Manages all sorts of things relating to data manipulation, such as saving or managing file addresses.
    /// </summary>
    #endregion
    public static class DataManager
    {
        #region Variables
        private const string SAVE_fileInfoSeparator = "#SAVED_INFO#";



        private static bool debugLogEmission => SHUU_Preferences.instance.dataManager_debugLogEmission;
        private static bool warningLogEmission => SHUU_Preferences.instance.dataManager_warningLogEmission;
        private static bool errorLogEmission => SHUU_Preferences.instance.dataManager_debugLogEmission;
        #endregion




        #region Logic

        #region General
        #region XML doc
        /// <summary>
        /// Gets a file address from the user.
        /// </summary>
        /// <param name="newFile">Whether the file address being fetched is from a pre-existing file or from a new file that must be created.</param>
        /// <returns>Returns the file address.</returns>
        #endregion
        public static string GetFileAddress(bool newFile, string displayName, params string[] extensions)
        {
            if (extensions == null || extensions.Length == 0) extensions = new[] { "*" };

            if (string.IsNullOrEmpty(displayName)) displayName = "Files";


            var filters = new[]
            {
                new ExtensionFilter(displayName, extensions),
                new ExtensionFilter("All files", "*")
            };

            BrowserProperties browseProperties = new BrowserProperties
            {
                filters = filters,
                defaultExtension = extensions[0] != "*" ? extensions[0] : "",
                defaultName = "NewFile"
            };


            string address = FileExplorer.GetFileAddress(browseProperties, newFile);

            if (address == null)
            {
                if (errorLogEmission) Debug.LogError("Error getting file address");
                return null;
            }


            if (newFile && extensions[0] != "*" && !address.EndsWith("." + extensions[0])) address += "." + extensions[0];

            return address;
        }


        #region XML doc
        /// <summary>
        /// Splits a string into all the info it contains, as a list.
        /// </summary>
        /// <param name="data">The string you want to split.</param>
        /// <returns>Returns a string list with all the individual info.</returns>
        #endregion
        public static string[] GetDataListFromString(string data) => data.Split(new[] { SAVE_fileInfoSeparator }, StringSplitOptions.None);
        
        #region XML doc
        /// <summary>
        /// Converts a list of information into one single string.
        /// </summary>
        /// <param name="dataList">The list you want to merge.</param>
        /// <returns>Returns a string with all the info from the list.</returns>
        #endregion
        public static string GetStringFromDataList(IEnumerable<string> dataList) => string.Join(SAVE_fileInfoSeparator, dataList);
        #endregion
        


        #region Manage file
        #region XML doc
        /// <summary>
        /// Checks for the existance of a file.
        /// </summary>
        /// <param name="address">The address of the file you want to check for.</param>
        /// <returns>Returns true if the file exists, false if it doesn't.</returns>
        #endregion
        public static bool FileExists(string address) => !string.IsNullOrEmpty(address) && File.Exists(address);


        public static bool DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try { File.Delete(filePath); }
                catch { return false; }

                return true;
            }
            else return false;
        }


        public static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path)) return;


            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }
        #endregion



        #region Write
        public static void WriteText_ToFile(string path, string contents)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(contents)) return;

            EnsureDirectoryExists(path);
            File.WriteAllText(path, contents);
        }

        public static void WriteText_ToFile(string path, IEnumerable<string> contents)
        {
            if (string.IsNullOrEmpty(path) || contents == null) return;

            EnsureDirectoryExists(path);
            File.WriteAllText(path, GetStringFromDataList(contents));
        }


        public static void AppendText_ToFile(string path, string contents)
        {
            if (string.IsNullOrEmpty(path) || contents == null) return;

            EnsureDirectoryExists(path);
            File.AppendAllText(path, contents);
        }

        public static void AppendText_ToFile(string path, IEnumerable<string> contents)
        {
            if (string.IsNullOrEmpty(path) || contents == null) return;

            EnsureDirectoryExists(path);
            File.AppendAllText(path, GetStringFromDataList(contents));
        }
        #endregion



        #region Read
        public static string ReadText_FromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !FileExists(path)) return null;

            return File.ReadAllText(path);
        }

        public static IEnumerable<string> ReadTextArray_FromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !FileExists(path)) return null;

            return GetDataListFromString(File.ReadAllText(path));
        }


        public static bool TryReadText_FromFile(string path, out string output)
        {
            output = null;

            if (string.IsNullOrEmpty(path) || !FileExists(path)) return false;

            output = File.ReadAllText(path);
            return true;
        }
        
        public static bool TryReadTextArray_FromFile(string path, out IEnumerable<string> output)
        {
            output = null;
            
            if (string.IsNullOrEmpty(path) || !FileExists(path)) return false;

            output = GetDataListFromString(File.ReadAllText(path));
            return true;
        }
        #endregion



        #region Json
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };


        public static string SerializeJson<T>(T data, JsonSerializerSettings settings = null) => JsonConvert.SerializeObject(data, settings ?? jsonSettings);
        public static string SerializeJson_Basic<T>(T data) => JsonUtility.ToJson(data);

        public static T DeserializeJson<T>(string json, JsonSerializerSettings settings = null) => JsonConvert.DeserializeObject<T>(json, settings ?? jsonSettings);
        public static T DeserializeJson_Basic<T>(string json) => JsonUtility.FromJson<T>(json);
        #endregion

        #endregion



        // NOT RECOMMENDED FOR IMPORTANT DATA, TEXT FILES CAN BE EASILY MODIFIED
        // IF SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
        #region Saving/Loading info to/from txt files
        #region Save
        #region XML doc
        /// <summary>
        /// Saves a text file in the user's PC.
        /// </summary>
        /// <param name="infoList">The list of info you want to save.</param>
        /// <param name="customLocationFileName">The location you want the file to be saved in. If this param is null, the user will be prompted to pick an address themselves.</param>
        #endregion
        public static void SaveTxtFile(IEnumerable<string> data, string customLocationFileName, bool persistentDataPath = true)
        {
            string address;
            if (customLocationFileName == null) address = GetFileAddress(true, "Text files", "txt");
            else address = persistentDataPath ? Application.persistentDataPath + customLocationFileName : customLocationFileName;
            

            string saveStr = GetStringFromDataList(data);


            if (address == null) return;


            if (!address.EndsWith(".txt")) address += ".txt";

            WriteText_ToFile(address, saveStr);


            if (debugLogEmission) Debug.Log("Saved TXT: " + saveStr);
        }
        #endregion


        #region Load
        #region XML doc
        /// <summary>
        /// Loads a text file from the user's PC.
        /// </summary>
        /// <param name="address">The location of the file you want to load. If this param is null, the user will be prompted to pick an address themselves.</param>
        /// <returns>A simple string array with all the info that was saved.</returns>
        #endregion
        public static IEnumerable<string> LoadTxtFile(string address, bool persistentDataPath = true)
        {
            if (address == null) address = GetFileAddress(false, "Text files", "txt");
            else if (persistentDataPath) address = Application.persistentDataPath + address;
            
            if (FileExists(address))
            {
                if (!TryReadText_FromFile(address, out string data))
                {
                    if (errorLogEmission) Debug.LogError("Failed load");

                    return null;
                }

                if (debugLogEmission) Debug.Log("Loaded TXT: " + data);

                return GetDataListFromString(data);
            }
            else
            {
                if (errorLogEmission) Debug.LogError("Failed load");

                return null;
            }
        }

        public static bool TryLoadTxtFile(string address, out IEnumerable<string> output, bool persistentDataPath = true)
        {
            output = null;


            if (address == null) address = GetFileAddress(false, "Text files", "txt");
            else if (persistentDataPath) address = Application.persistentDataPath + address;
            
            if (!FileExists(address) || !TryReadText_FromFile(address, out string data)) return false;
            

            if (debugLogEmission) Debug.Log("Loaded TXT: " + data);

            output = GetDataListFromString(data);
            return true;
        }
        public static bool TryLoadTxtFile(string address, out string output, bool persistentDataPath = true)
        {
            output = null;

            
            if (address == null) address = GetFileAddress(false, "Text files", "txt");
            else if (persistentDataPath) address = Application.persistentDataPath + address;
            
            if (!FileExists(address) || !TryReadText_FromFile(address, out string data)) return false;
            

            if (debugLogEmission) Debug.Log("Loaded TXT: " + data);

            output = data;
            return true;
        }
        #endregion
        #endregion


        // RECOMMENDED FOR SAVING IN GENERAL, INFO IS DECENTLY SECURE.
        // IF FURTHER SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
        #region Saving/Loading info to/from json files
        #region Save
        public static void SaveJsonFile<T>(T data, string customLocationFileName, bool persistentDataPath = true)
        {
            string address;
            if (customLocationFileName == null) address = GetFileAddress(true, "Json files", "json");
            else address = persistentDataPath ? Application.persistentDataPath + customLocationFileName : customLocationFileName;
            

            if (address == null) return;


            if (!address.EndsWith(".json")) address += ".json";


            string jsonString = SerializeJson(data);

            WriteText_ToFile(address, jsonString);

            if (debugLogEmission) Debug.Log("Saved JSON: " + jsonString);
        }
        public static void SaveJsonFile_Basic<T>(T data, string customLocationFileName, bool persistentDataPath = true, bool prettyPrint = false)
        {
            string address;
            if (customLocationFileName == null) address = GetFileAddress(true, "Json files", "json");
            else address = persistentDataPath ? Application.persistentDataPath + customLocationFileName : customLocationFileName;
            

            if (address == null) return;


            if (!address.EndsWith(".json")) address += ".json";


            string jsonString = SerializeJson_Basic(data);

            WriteText_ToFile(address, jsonString);

            if (debugLogEmission) Debug.Log("Saved JSON: " + jsonString);
        }
        #endregion


        #region Load
        public static T LoadJsonFile<T>(string address, bool persistentDataPath = true)
        {
            if (address == null) address = GetFileAddress(false, "Json files", "json");
            else if (persistentDataPath) address = Application.persistentDataPath + address;

            if (!FileExists(address)) return default;


            string json = File.ReadAllText(address);
            return DeserializeJson<T>(json);
        }
        public static T LoadJsonFile_Basic<T>(string address, bool persistentDataPath = true)
        {
            if (address == null) address = GetFileAddress(false, "Json files", "json");
            else if (persistentDataPath) address = Application.persistentDataPath + address;

            if (!FileExists(address)) return default;


            string json = File.ReadAllText(address);
            return DeserializeJson_Basic<T>(json);
        }

        public static bool TryLoadJsonFile<T>(string address, out T data, bool persistentDataPath = true)
        {
            data = default;


            if (address == null) address = GetFileAddress(false, "Json files", "json");
            else if (persistentDataPath) address = Application.persistentDataPath + address;

            if (!FileExists(address)) return false;


            string json = File.ReadAllText(address);
            data = DeserializeJson<T>(json);

            return true;
        }
        public static bool TryLoadJsonFile_Basic<T>(string address, out T data, bool persistentDataPath = true)
        {
            data = default;


            if (address == null) address = GetFileAddress(false, "Json files", "json");
            else if (persistentDataPath) address = Application.persistentDataPath + address;

            if (!FileExists(address)) return false;


            string json = File.ReadAllText(address);
            data = DeserializeJson_Basic<T>(json);

            return true;
        }
        #endregion
        #endregion


        // NOT RECOMMENDED FOR IMPORTANT DATA, PREFABS CAN BE EASILY MODIFIED
        // IF SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
        #region Saving/Loading info to/from player prefabs
        #region General
        public static bool HasPlayerPref(string key) => PlayerPrefs.HasKey(key);

        public static void DeletePlayerPref(string key)
        {
            if (HasPlayerPref(key))
            {
                PlayerPrefs.DeleteKey(key);
                if (debugLogEmission) Debug.Log($"Deleted PlayerPref [{key}]");
            }
        }
        public static void ClearAllPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            if (debugLogEmission) Debug.Log("All PlayerPrefs cleared");
        }
        #endregion


        #region API
        public enum PlayerPrefMode
        {
            Automatic,
            Native,
            Json
        }

        private static bool IsNativePlayerPrefType<T>()
        {
            Type t = typeof(T);

            return
                t == typeof(string) ||
                t == typeof(bool) ||
                t == typeof(int) ||
                t == typeof(float) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Color) ||
                t.IsEnum;
        }


        public static void SavePlayerPref<T>(string key, T data, bool saveImmediately = true, PlayerPrefMode mode = PlayerPrefMode.Automatic)
        {
            switch (mode)
            {
                case PlayerPrefMode.Automatic:
                    if (IsNativePlayerPrefType<T>()) Native_SavePlayerPref(key, data);
                    else Json_SavePlayerPref(key, data);
                    break;

                case PlayerPrefMode.Native:
                    Native_SavePlayerPref(key, data);
                    break;

                case PlayerPrefMode.Json:
                    Json_SavePlayerPref(key, data);
                    break;
            }
            
            if (saveImmediately) PlayerPrefs.Save();
        }

        public static T LoadPlayerPref<T>(string key, T defaultValue = default, PlayerPrefMode mode = PlayerPrefMode.Automatic)
        {
            if (!HasPlayerPref(key))
            {
                if (warningLogEmission) Debug.LogWarning($"PlayerPref key not found: {key}");
                return defaultValue;
            }

            switch (mode)
            {
                case PlayerPrefMode.Automatic:
                    if (IsNativePlayerPrefType<T>()) return Native_LoadPlayerPref(key, defaultValue);
                    else return Json_LoadPlayerPref<T>(key);

                case PlayerPrefMode.Native:
                    return Native_LoadPlayerPref(key, defaultValue);

                case PlayerPrefMode.Json:
                    return Json_LoadPlayerPref<T>(key);

                default:
                    if (errorLogEmission) Debug.LogError($"Unsupported PlayerPrefMode: {mode}");
                    return defaultValue;
            }
        }
        #endregion


        #region Save
        public static void Json_SavePlayerPref<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (errorLogEmission) Debug.LogError("PlayerPref key is null or empty");
                return;
            }


            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);

            if (debugLogEmission) Debug.Log($"Saved PlayerPref [{key}]: {json}");
        }
        public static void Json_SavePlayerPref_Default<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (errorLogEmission) Debug.LogError("PlayerPref key is null or empty");
                return;
            }


            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(key, json);

            if (debugLogEmission) Debug.Log($"Saved PlayerPref [{key}]: {json}");
        }
        #endregion


        #region Load
        public static T Json_LoadPlayerPref<T>(string key)
        {
            if (!HasPlayerPref(key))
            {
                if (warningLogEmission) Debug.LogWarning($"PlayerPref key not found: {key}");
                return default;
            }


            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                if (errorLogEmission) Debug.LogError($"Error loading PlayerPref: {key}");
                return default;
            }
            T data = JsonConvert.DeserializeObject<T>(json);

            if (debugLogEmission) Debug.Log($"Loaded PlayerPref [{key}]: {json}");

            return data;
        }
        public static T Json_LoadPlayerPref_Default<T>(string key)
        {
            if (!HasPlayerPref(key))
            {
                if (warningLogEmission) Debug.LogWarning($"PlayerPref key not found: {key}");
                return default;
            }


            string json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                if (errorLogEmission) Debug.LogError($"Error loading PlayerPref: {key}");
                return default;
            }
            T data = JsonUtility.FromJson<T>(json);

            if (debugLogEmission) Debug.Log($"Loaded PlayerPref [{key}]: {json}");

            return data;
        }
        #endregion


        #region Direct
        #region XML doc
        /// <summary>
        /// Saves an PlayerPref.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        /// <param name="value">The value to save.</param>
        #endregion
        public static void Native_SavePlayerPref<T>(string key, T value)
        {
            if (value is string s) PlayerPrefs.SetString(key, s);
            else if (value is bool b)
            {
                string bool_str = b ? bool.TrueString : bool.FalseString;
                PlayerPrefs.SetString(key, bool_str);
            }
            else if (value is int i) PlayerPrefs.SetInt(key, i);
            else if (value is float f) PlayerPrefs.SetFloat(key, f);
            else if (value is Vector2 v2)
            {
                Native_SavePlayerPref(key + "_x", v2.x);
                Native_SavePlayerPref(key + "_y", v2.y);
            }
            else if (value is Vector3 v3)
            {
                Native_SavePlayerPref(key + "_x", v3.x);
                Native_SavePlayerPref(key + "_y", v3.y);
                Native_SavePlayerPref(key + "_z", v3.z);
            }
            else if (value is Color c)
            {
                string hex = ColorUtility.ToHtmlStringRGBA(c);

                Native_SavePlayerPref(key, hex);
            }
            else if (typeof(T).IsEnum) Native_SavePlayerPref(key, value.ToString());
            else
            {
                try
                {
                    string json = JsonConvert.SerializeObject(value);
                    Native_SavePlayerPref(key, json);
                }
                catch { throw new NotSupportedException($"Type {typeof(T)} is not supported by SetPlayerPref and isn't Serializable."); }
            }
        }

        #region XML doc
        /// <summary>
        /// Retrieves an PlayerPref's value.
        /// </summary>
        /// <param name="key">The PlayerPref's key (aka their "name").</param>
        /// <param name="defaultValue">The default value of this PlayerPref.</param>
        /// <returns>Returns the value of the PlayerPref.</returns>
        #endregion
        public static T Native_LoadPlayerPref<T>(string key, T defaultValue = default)
        {
            if (!HasPlayerPref(key)) return defaultValue;


            if (typeof(T) == typeof(string)) return (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
            else if (typeof(T) == typeof(bool))
            {
                string bool_defaultVal_str = (bool)(object)defaultValue ? bool.TrueString : bool.FalseString;
                string bool_str = PlayerPrefs.GetString(key, bool_defaultVal_str);

                bool bool_val = bool.TryParse(bool_str, out bool result) && result;

                return (T)(object)bool_val;
            }
            else if (typeof(T) == typeof(int)) return (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
            else if (typeof(T) == typeof(float)) return (T)(object)PlayerPrefs.GetFloat(key, (float)(object)defaultValue);
            else if (typeof(T) == typeof(Vector2))
            {
                float x = Native_LoadPlayerPref(key + "_x", ((Vector2)(object)defaultValue).x);
                float y = Native_LoadPlayerPref(key + "_y", ((Vector2)(object)defaultValue).y);
                return (T)(object)new Vector2(x, y);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                float x = Native_LoadPlayerPref(key + "_x", ((Vector3)(object)defaultValue).x);
                float y = Native_LoadPlayerPref(key + "_y", ((Vector3)(object)defaultValue).y);
                float z = Native_LoadPlayerPref(key + "_z", ((Vector3)(object)defaultValue).z);
                return (T)(object)new Vector3(x, y, z);
            }
            else if (typeof(T) == typeof(Color))
            {
                string hex = Native_LoadPlayerPref(key, ColorUtility.ToHtmlStringRGBA((Color)(object)defaultValue));

                if (ColorUtility.TryParseHtmlString("#" + hex, out var color)) return (T)(object)color;

                return defaultValue;
            }
            else if (typeof(T).IsEnum)
            {
                string str = Native_LoadPlayerPref(key, defaultValue.ToString());

                try { return (T)Enum.Parse(typeof(T), str); }
                catch { return defaultValue; }
            }
            else
            {
                try
                {
                    string json = Native_LoadPlayerPref(key, "");
                    if (string.IsNullOrEmpty(json)) return defaultValue;

                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch { throw new NotSupportedException($"Type {typeof(T)} is not supported by GetPlayerPref and isn't Serializable."); }
            }
        }
        #endregion
        #endregion
    }
}
