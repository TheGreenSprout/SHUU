/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using System;
using UnityEngine;

using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using static SHUU.InnerWorkings.SHUU_PackageUtils;

namespace SHUU.Utils.Developer.Console
{
    [RequireComponent(typeof(DevConsoleManager))]
    public class BoundCommands : AutoSave_Json_MonoBehaviour<BoundCommands_SaveData>
    {
        #region Variables
        private static Dictionary<(KeyCode?, int?, string), string[]> boundCommands = new();
        private static Dictionary<(InputBindingMap, string), string[]> is_boundCommands = new();


        private DevConsoleManager devConsoleManager;
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            devConsoleManager = GetComponent<DevConsoleManager>();
        }


        private void Update()
        {
            if (devConsoleManager.devConsoleUI.gameObject.activeInHierarchy && devConsoleManager.inputFieldActive) return;


            foreach (var kvp in boundCommands)
            {
                if (GetInputDown(kvp.Key)) devConsoleManager.ProcessInput(string.Join(" ", kvp.Value));
            }

            foreach (var kvp in is_boundCommands)
            {
                if (SHUU_Input.GetInputDown(kvp.Key.Item1, kvp.Key.Item2)) devConsoleManager.ProcessInput(string.Join(" ", kvp.Value));
            }
        }
        #endregion



        #region API
        public static void BindCommand((KeyCode?, int?, string) input, string[] commandData) => boundCommands.Add(input, commandData);
        public static void UnBindCommands((KeyCode?, int?, string) input) => boundCommands.Remove(input);

        public static void BindCommand((InputBindingMap, string) binding, string[] commandData) => is_boundCommands.Add(binding, commandData);
        public static void UnBindCommands((InputBindingMap, string) binding) => is_boundCommands.Remove(binding);


        private bool GetInputDown((KeyCode?, int?, string) input)
        {
            if (input.Item1 != null) return Input.GetKeyDown(input.Item1.Value);
            else if (input.Item2 != null) return Input.GetMouseButtonDown(input.Item2.Value);
            else if (input.Item3 != null) return Input.GetAxisRaw(input.Item3) > 0;

            return false;
        }
        #endregion



        #region Saving/Loading
        protected override string FileAddress() => GetPath("DevConsole", "bound_commands" + ".json");


        protected override BoundCommands_SaveData SaveData() => new BoundCommands_SaveData(boundCommands, is_boundCommands);

        protected override void LoadData(BoundCommands_SaveData data)
        {
            boundCommands = new(data.boundCommands);
            is_boundCommands = new(data.is_boundCommands);
        }
        #endregion
    }




    #region Save data class
    [Serializable]
    public class BoundCommands_SaveData
    {
        public Dictionary<(KeyCode?, int?, string), string[]> boundCommands = new();
        public Dictionary<(InputBindingMap, string), string[]> is_boundCommands = new();


        public BoundCommands_SaveData(Dictionary<(KeyCode?, int?, string), string[]> data, Dictionary<(InputBindingMap, string), string[]> is_data)
        {
            if (data == null) boundCommands = new();
            else boundCommands = new(data);

            if (is_data == null) is_boundCommands = new();
            else is_boundCommands = new(is_data);
        }
    }
    #endregion
}
