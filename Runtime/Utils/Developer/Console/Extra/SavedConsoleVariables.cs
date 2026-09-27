using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using SHUU.Utils.Helpers;

using static SHUU.InnerWorkings.SHUU_PackageUtils;

namespace SHUU.Utils.Developer.Console
{
    public class SavedConsoleVariables : AutoSave_Json_MonoBehaviour<SavedConsoleVariables_SaveData>
    {
        #region Variables
        private static Dictionary<string, List<string>> Variables = new();
        #endregion




        #region API
        public static void Set(string name, List<string> value) => Variables[name.ToLower()] = value;
        public static bool TryGet(string name, out List<string> value) => Variables.TryGetValue(name.ToLower(), out value);

        public static bool Exists(string name) => Variables.ContainsKey(name.ToLower());
        public static Dictionary<string, List<string>> GetAll() => new(Variables);

        public static void Remove(string name) => Variables.Remove(name.ToLower());
        public static void Clear() => Variables.Clear();


        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This code snippet was written with the assistance of AI.
        */
        #region Parse
        #region XML doc
        /// <summary>
        /// Variable names start with a letter or _ and go on with letters, numbers and _, so they can be told apart from the text around them.
        /// </summary>
        #endregion
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name) || !IsNameStart(name[0])) return false;

            for (int i = 1; i < name.Length; i++)
                if (!IsNameCharacter(name[i])) return false;

            return true;
        }

        private static bool IsNameStart(char c) => char.IsLetter(c) || c == '_';
        private static bool IsNameCharacter(char c) => char.IsLetterOrDigit(c) || c == '_';
        private static bool IsDigit(char c) => c >= '0' && c <= '9';


        #region XML doc
        /// <summary>
        /// Replaces every $variable in the text, wherever it is in a word (hp&lt;$health,ammo=$ammo). One that doesn't exist loses its $.
        /// </summary>
        #endregion
        public static string ParseVariable(string input) => Expand(input, int.MaxValue, false, out _);

        #region XML doc
        /// <summary>
        /// Same, but a variable that doesn't exist is an error, and so is the result getting longer than maxLength. On an error the text comes back unchanged.
        /// </summary>
        #endregion
        public static string ParseVariable(string input, out CommandReturn error, int maxLength = int.MaxValue)
        {
            string result = Expand(input, maxLength, true, out error);

            return error == null ? result : input;
        }


        private static string Expand(string input, int maxLength, bool reportMissing, out CommandReturn error)
        {
            error = null;

            if (input.IndexOf('$') < 0) return input;


            StringBuilder result = new StringBuilder(input.Length);
            List<string> missing = new List<string>();

            int i = 0;

            // Where the text is inside quotes (the same way the console reads them), because a variable is put in differently there.
            bool inQuotes = false;
            int backslashes = 0;

            while (i < input.Length)
            {
                // A '$' that isn't followed by a name (like the one in "cost$5") is just text.
                if (input[i] != '$' || i + 1 >= input.Length || !IsNameStart(input[i + 1]))
                {
                    if (input[i] == '\\') backslashes++;
                    else
                    {
                        if (input[i] == '"' && backslashes % 2 == 0) inQuotes = !inQuotes;

                        backslashes = 0;
                    }

                    result.Append(input[i]);
                    i++;

                    continue;
                }


                int nameStart = i + 1;
                int nameEnd = nameStart + 1;

                while (nameEnd < input.Length && IsNameCharacter(input[nameEnd])) nameEnd++;

                int end = nameEnd;
                ReadSlice(input, ref end, out int? start, out int? count);

                string name = input.Substring(nameStart, nameEnd - nameStart);

                if (TryGet(name, out var value))
                {
                    List<string> words = Slice(value, start, count);

                    // Outside quotes every word that has a space in it gets its own, so it stays one word. Inside quotes they just join the quoted text.
                    result.Append(inQuotes ? ConsoleTokenizer.EscapeForQuotes(string.Join(" ", words), end < input.Length && input[end] == '"') : ConsoleTokenizer.Join(words));
                }
                else if (reportMissing) { if (!missing.Contains(name)) missing.Add(name); }
                else result.Append(input, nameStart, end - nameStart);

                backslashes = 0;
                i = end;


                // Checked as it grows, so a variable that contains itself never gets to build a huge line.
                if (result.Length > maxLength)
                {
                    error = CommandReturn.Red($"The line got longer than {maxLength} characters once the variables were replaced (does a variable contain itself?).");

                    return input;
                }
            }


            if (missing.Count > 0)
            {
                error = CommandReturn.Red(missing.Count == 1 ? $"Variable '{missing[0]}' not found" : $"Variables '{string.Join("', '", missing)}' not found");

                return input;
            }

            return result.ToString();
        }

        // >X skips the first X words, <Y keeps only Y of them (after skipping). Going past the end gives fewer words, or none.
        private static List<string> Slice(List<string> words, int? start, int? count)
        {
            int from = Math.Clamp(start ?? 0, 0, words.Count);
            int take = Math.Clamp(count ?? words.Count - from, 0, words.Count - from);

            return words.GetRange(from, take);
        }

        // Reads the >X and <Y written right after a name. They only count when a number follows, so in "$max<$min" the < stays as text.
        private static void ReadSlice(string text, ref int index, out int? start, out int? count)
        {
            start = null;
            count = null;

            while (index + 1 < text.Length && (text[index] == '<' || text[index] == '>') && IsDigit(text[index + 1]))
            {
                bool isCount = text[index] == '<';
                index++;

                long number = 0;

                while (index < text.Length && IsDigit(text[index]))
                {
                    number = Math.Min(number * 10 + (text[index] - '0'), int.MaxValue);
                    index++;
                }

                if (isCount) count = (int)number;
                else start = (int)number;
            }
        }
        #endregion
        
        #endregion



        #region Saving/Loading
        protected override string FileAddress() => GetPath("DevConsole", "saved_console_variables" + ".json");


        protected override SavedConsoleVariables_SaveData SaveData() => new SavedConsoleVariables_SaveData(Variables);

        protected override void LoadData(SavedConsoleVariables_SaveData data)
        {
            if (data == null) return;
            
            Variables = new(data.variables);
        }
        #endregion
    }




    #region Save data class
    [Serializable]
    public class SavedConsoleVariables_SaveData
    {
        public Dictionary<string, List<string>> variables = new();


        public SavedConsoleVariables_SaveData(Dictionary<string, List<string>> data)
        {
            if (data == null) variables = new();
            else variables = new(data);
        }
    }
    #endregion
}
