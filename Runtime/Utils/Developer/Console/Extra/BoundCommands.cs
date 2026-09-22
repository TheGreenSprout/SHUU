using System.Collections.Generic;
using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

using SHUU.Utils.InputSystem;
using SHUU.Utils.Helpers;

using static SHUU.InnerWorkings.SHUU_PackageUtils;

namespace SHUU.Utils.Developer.Console
{
    [RequireComponent(typeof(DevConsoleManager))]
    public class BoundCommands : AutoSave_Json_MonoBehaviour<BoundCommands_SaveData>
    {
        #region Variables
        private static Dictionary<string, List<string>> BindCommands = new();
        private static Dictionary<string, List<string>> Direct_BindCommands = new();

        private static Dictionary<KeyCode, List<string>> Classic_BindCommands_key = new();
        private static Dictionary<int, List<string>> Classic_BindCommands_mouse = new();


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


            foreach (var kvp in BindCommands)
                if (SHUU_Input.GetInputDown(kvp.Key))
                {
                    foreach (var cmd in kvp.Value)
                        devConsoleManager.ProcessInput(cmd);
                }

            foreach (var kvp in Direct_BindCommands)
                if (GetDirectInputDown(kvp.Key))
                {
                    foreach (var cmd in kvp.Value)
                        devConsoleManager.ProcessInput(cmd);
                }


            foreach (var kvp in Classic_BindCommands_key)
                if (Input.GetKeyDown(kvp.Key))
                {
                    foreach (var cmd in kvp.Value)
                        devConsoleManager.ProcessInput(cmd);
                }

            foreach (var kvp in Classic_BindCommands_mouse)
                if (Input.GetMouseButtonDown(kvp.Key))
                {
                    foreach (var cmd in kvp.Value)
                        devConsoleManager.ProcessInput(cmd);
                }
        }
        #endregion



        #region API

        #region Input System

        #region Action
        public static void BindCommand(string actionPath, string[] commandData) => Bind(BindCommands, actionPath, commandData);
        public static bool UnBindCommands(string actionPath, string[] commandData = null) => Unbind(BindCommands, actionPath, commandData);
        #endregion


        #region Direct
        public static void BindDirectCommand(string controlPath, string[] commandData) => Bind(Direct_BindCommands, controlPath, commandData);
        public static bool UnBindDirectCommands(string controlPath, string[] commandData = null) => Unbind(Direct_BindCommands, controlPath, commandData);

        private static bool GetDirectInputDown(string controlPath)
        {
            #if ENABLE_INPUT_SYSTEM
            InputControl control = UnityEngine.InputSystem.InputSystem.FindControl(controlPath);
            if (control == null) return false;

            if (control is ButtonControl button) return button.wasPressedThisFrame;
            if (control is AxisControl axis) return axis.ReadValue() > 0f;
            #endif

            return false;
        }
        #endregion
        
        #endregion



        #region Classic Input

        #region Classic key
        public static void BindClassicCommand(KeyCode key, string[] commandData) => Bind(Classic_BindCommands_key, key, commandData);
        public static bool UnBindClassicCommands(KeyCode key, string[] commandData = null) => Unbind(Classic_BindCommands_key, key, commandData);
        #endregion


        #region Classic mouse
        public static void BindClassicCommand(int mouseButton, string[] commandData) => Bind(Classic_BindCommands_mouse, mouseButton, commandData);
        public static bool UnBindClassicCommands(int mouseButton, string[] commandData = null) => Unbind(Classic_BindCommands_mouse, mouseButton, commandData);
        #endregion
        
        #endregion



        #region General
        public static void GetAllBinds(
            out Dictionary<string, List<string>> actions,
            out Dictionary<string, List<string>> direct,
            out Dictionary<KeyCode, List<string>> classicKeys,
            out Dictionary<int, List<string>> classicMouse)
        {
            actions = BindCommands;
            direct = Direct_BindCommands;
            classicKeys = Classic_BindCommands_key;
            classicMouse = Classic_BindCommands_mouse;
        }
        

        public static void ClearAllBinds()
        {
            BindCommands.Clear();
            Direct_BindCommands.Clear();
            Classic_BindCommands_key.Clear();
            Classic_BindCommands_mouse.Clear();
        }
        #endregion



        #region Helpers
        private static void Bind<TKey>(Dictionary<TKey, List<string>> dict, TKey key, string[] commandData)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<string>();
                dict[key] = list;
            }

            list.Add(string.Join(" ", commandData));
        }

        private static bool Unbind<TKey>(Dictionary<TKey, List<string>> dict, TKey key, string[] commandData = null)
        {
            if (!dict.TryGetValue(key, out var list)) return false;

            if (commandData == null || commandData.Length == 0)
            {
                dict.Remove(key);
                return true;
            }

            string joined = string.Join(" ", commandData);
            int removed = list.RemoveAll(c => c == joined);
            if (list.Count == 0) dict.Remove(key);

            return removed > 0;
        }
        #endregion

        #endregion



        #region Saving/Loading
        protected override string FileAddress() => GetPath("DevConsole", "bound_commands.json");


        protected override BoundCommands_SaveData SaveData()
            => new BoundCommands_SaveData(BindCommands, Direct_BindCommands, Classic_BindCommands_key, Classic_BindCommands_mouse);

        protected override void LoadData(BoundCommands_SaveData data)
        {
            if (data == null) return;

            
            BindCommands = new(data.boundCommands);
            Direct_BindCommands = new(data.direct_boundCommands);

            Classic_BindCommands_key = new(data.classic_boundCommands_key);
            Classic_BindCommands_mouse = new(data.classic_boundCommands_mouse);
        }
        #endregion
    }




    #region Save data class
    [Serializable]
    public class BoundCommands_SaveData
    {
        public Dictionary<string, List<string>> boundCommands = new();
        public Dictionary<string, List<string>> direct_boundCommands = new();

        public Dictionary<KeyCode, List<string>> classic_boundCommands_key = new();
        public Dictionary<int, List<string>> classic_boundCommands_mouse = new();


        public BoundCommands_SaveData(
            Dictionary<string, List<string>> data,
            Dictionary<string, List<string>> directData,
            Dictionary<KeyCode, List<string>> keyData,
            Dictionary<int, List<string>> mouseData)
        {
            boundCommands = data == null ? new() : new(data);
            direct_boundCommands = directData == null ? new() : new(directData);
            classic_boundCommands_key = keyData == null ? new() : new(keyData);
            classic_boundCommands_mouse = mouseData == null ? new() : new(mouseData);
        }
    }
    #endregion
}