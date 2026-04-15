/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System;
using System.Collections.Generic;
using UnityEngine;

using static SHUU.InnerWorkings.SHUU_PackageUtils;

namespace SHUU.Utils.Developer.Console
{
    public class SavedConsoleVariables : MonoBehaviour
    {
        #region Variables
        private static string filePath => PackageFilePath("DevConsole", "saved_console_variables" + ".json");

    

        private static Dictionary<string, List<string>> variables = new();

        private static bool loaded = false;
        #endregion




        #region Main
        private void Awake()
        {
            if (!loaded)
            {
                loaded = true;
                
                Load();
            }
        }

        private void OnApplicationQuit() => Save();
        #endregion



        #region API
        public static void Set(string name, List<string> value) => variables[name.ToLower()] = value;
        public static bool TryGet(string name, out List<string> value)
            => variables.TryGetValue(name.ToLower(), out value);

        public static bool Exists(string name) => variables.ContainsKey(name.ToLower());
        public static Dictionary<string, List<string>> GetAll() => new(variables);

        public static void Remove(string name) => variables.Remove(name.ToLower());
        public static void Clear() => variables.Clear();


        #region Parse
        public static string ParseVariable(string input)
        {
            string[] parts = input.Split(' ');

            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].StartsWith("$"))
                {
                    parts[i] = parts[i].Replace("$", "");
                    continue;
                }

                var (name, start, count) = ParseVariableToken(parts[i]);

                if (!TryGet(name, out var value))
                {
                    parts[i] = parts[i].Replace("$", "");
                    continue;
                }

                int actualStart = 0;

                // >X
                if (start.HasValue && value.Count > start.Value) actualStart = start.Value;

                int available = value.Count - actualStart;

                // <Y
                int actualCount = count.HasValue ? Math.Min(count.Value, available) : available;

                if (actualStart < 0) actualStart = 0;
                if (actualStart > value.Count) actualStart = value.Count;
                if (actualCount < 0) actualCount = 0;

                var sliced = value.GetRange(actualStart, actualCount);

                parts[i] = string.Join(" ", sliced);
            }

            return string.Join(" ", parts);
        }
        public static string ParseVariable(string input, out CommandReturn error)
        {
            error = null;

            string[] parts = input.Split(' ');

            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].StartsWith("$")) continue;

                var (name, start, count) = ParseVariableToken(parts[i]);

                if (!TryGet(name, out var value))
                {
                    error = new CommandReturn(Color.red, $"Variable '{name}' not found");
                    continue;
                }

                int actualStart = 0;

                // >X
                if (start.HasValue && value.Count > start.Value) actualStart = start.Value;

                int available = value.Count - actualStart;

                // <Y
                int actualCount = count.HasValue ? Math.Min(count.Value, available) : available;

                if (actualStart < 0) actualStart = 0;
                if (actualStart > value.Count) actualStart = value.Count;
                if (actualCount < 0) actualCount = 0;

                var sliced = value.GetRange(actualStart, actualCount);

                parts[i] = string.Join(" ", sliced);
            }

            return string.Join(" ", parts);
        }

        private static (string name, int? start, int? end) ParseVariableToken(string token)
        {
            int? start = null;
            int? end = null;

            // extract name
            int varEnd = token.Length;

            for (int i = 0; i < token.Length; i++)
            {
                if (token[i] == '<' || token[i] == '>')
                {
                    varEnd = i;
                    break;
                }
            }

            string name = token.Substring(1, varEnd - 1); // skip $

            string modifiers = token.Substring(varEnd);

            int iMod = 0;
            while (iMod < modifiers.Length)
            {
                char c = modifiers[iMod];

                if (c == '<')
                {
                    iMod++;
                    int val = 0;
                    while (iMod < modifiers.Length && char.IsDigit(modifiers[iMod]))
                    {
                        val = val * 10 + (modifiers[iMod] - '0');
                        iMod++;
                    }
                    end = val-1;
                }
                else if (c == '>')
                {
                    iMod++;
                    int val = 0;
                    while (iMod < modifiers.Length && char.IsDigit(modifiers[iMod]))
                    {
                        val = val * 10 + (modifiers[iMod] - '0');
                        iMod++;
                    }
                    start = val;
                }
                else iMod++;
            }

            return (name, start, end);
        }
        #endregion
        #endregion



        #region Saving/Loading
        private void Save()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(variables, Newtonsoft.Json.Formatting.Indented);

                var dir = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex) { Debug.LogError($"Failed to save console variables: {ex}"); }
        }


        private void Load()
        {
            try
            {
                if (!System.IO.File.Exists(filePath)) return;


                string json = System.IO.File.ReadAllText(filePath);

                variables = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)
                            ?? new Dictionary<string, List<string>>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load console variables: {ex}");

                variables = new Dictionary<string, List<string>>();
            }
        }
        #endregion
    }
}
