using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using SHUU.InnerWorkings;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.SettingsSytem;
using SHUU.Utils.SceneManagement;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    #region Information

    #region Help
    private static CommandReturn helpRet = null;

    [DevConsoleCommand("help", "Lists all commands", "Information")]
    public static CommandReturn Help()
    {
        if (helpRet != null) return helpRet;


        var cmds = DevCommandRegistry.AllCommands().ToList();
        if (!cmds.Any()) return new CommandReturn(Color.red, "No commands registered.");

        List<string> tagOrder = DevConsoleManager.instance.tagList;
        int fallbackIndex = tagOrder.Count - 2;


        var grouped = new Dictionary<int, Dictionary<string, List<(int order, string text)>>>();

        foreach (var (name, info) in cmds)
        {
            int tagIndex = tagOrder.IndexOf(info.Tag);
            if (tagIndex == -1) tagIndex = fallbackIndex;

            if (!grouped.TryGetValue(tagIndex, out var tagGroups))
            {
                tagGroups = new Dictionary<string, List<(int, string)>>();
                grouped[tagIndex] = tagGroups;
            }

            if (!tagGroups.TryGetValue(info.Tag, out var list))
            {
                list = new List<(int, string)>();
                tagGroups[info.Tag] = list;
            }

            var parameters = info.Method.GetParameters();

            string paramString = parameters.Length == 0 ? "" : " (" + string.Join(", ", parameters.Select(p => {
                    bool isParams = Attribute.IsDefined(p, typeof(ParamArrayAttribute));

                    if (isParams)
                    {
                        Type elemType = p.ParameterType.GetElementType();
                        return $"params {ParseParameter(elemType)}[]";
                    }

                    return ParseParameter(p.ParameterType);
                })) + ")";

            string display = $"{name}{paramString} - {info.Description}";

            list.Add((info.Order, display));
        }

        
        var output = new List<string>();

        foreach (var index in grouped.Keys.OrderBy(i => i))
        {
            foreach (var tagGroup in grouped[index])
            {
                output.Add(tagGroup.Key);

                foreach (var cmd in tagGroup.Value.OrderBy(x => x.order))
                    output.Add("  " + cmd.text);

                output.Add("\n");
            }
        }


        return new CommandReturn(output.ToArray());
    }


    private static string ParseParameter(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OptionalParameter<>))
        {
            Type inner = t.GetGenericArguments()[0];
            
            return "~" + ParseParameter(inner);
        }


        if (t == typeof(bool))       return "bool";
        if (t == typeof(byte))       return "byte";
        if (t == typeof(sbyte))      return "sbyte";
        if (t == typeof(short))      return "short";
        if (t == typeof(ushort))     return "ushort";
        if (t == typeof(int))        return "int";
        if (t == typeof(uint))       return "uint";
        if (t == typeof(long))       return "long";
        if (t == typeof(ulong))      return "ulong";
        if (t == typeof(float))      return "float";
        if (t == typeof(double))     return "double";
        if (t == typeof(decimal))    return "decimal";
        if (t == typeof(char))       return "char";
        if (t == typeof(string))     return "string";
        if (t == typeof(MutableParameter))     return "mutable";

        else return null;
    }
    #endregion



    #region Display
    [DevConsoleCommand("allvars", "List all variables", "Information")]
    public static CommandReturn AllVars()
    {
        var vars = SavedConsoleVariables.GetAll();
        
        if (vars.Count == 0) return new CommandReturn("No variables set.");

        List<CommandReturn> returns = new();


        foreach (var key in vars.Keys)
            returns.Add(Var(key));


        return new CommandReturn(returns.ToArray());
    }

    [DevConsoleCommand("var", "List a specific variable", "Information")]
    public static CommandReturn Var(string name)
    {
        string ret = SavedConsoleVariables.ParseVariable("$"+name, out CommandReturn error);
        ret.Replace(" ", ", ");

        if (error != null) return error;


        return new CommandReturn(Color.green, $"{name} = {ret}");
    }


    [DevConsoleCommand("shotlocation", "Shows the last saved screenshot's file location", "Information")]
    public static CommandReturn Shotlocation()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return new CommandReturn(Color.red, "No screenshot has been taken yet");

        return new CommandReturn($"Last screenshot saved at: {ScreenCaptureHelper.lastPath}");
    }


    [DevConsoleCommand("pools", "Displays all object pool information", "Information")]
    public static CommandReturn Pools()
    {
        if (ObjectPooling.pools.Count == 0) return new CommandReturn(Color.yellow, "No object pools found");


        string[] lines = new string[ObjectPooling.pools.Count + 1];
        lines[0] = "Object Pools:";

        int i = 1;
        foreach (var pool in ObjectPooling.pools)
        {
            lines[i] = $"- Name: {pool.name}, Type: {pool.GetItemType().Name}, Total: {pool.totalCount}, In Pool: {pool.poolCount}";
            i++;
        }


        return new CommandReturn(lines);
    }


    [DevConsoleCommand("applicationpath", "Displays the application's persistent data path", "Information")]
    public static CommandReturn ApplicationPath() => new CommandReturn($"Application persistent data path: {Application.persistentDataPath}");

    [DevConsoleCommand("shuupath", "Displays the SHUU's persistent data path", "Information")]
    public static CommandReturn SHUUPath() => new CommandReturn($"SHUU persistent data path: {SHUU_PackageUtils.GetPath("General")}");
    #endregion

    #endregion




    #region Debug

    #region Visual
    [DevConsoleCommand("colliders", "Toggles the visibility of all colliders in the game", "Debug")]
    public static CommandReturn Colliders()
    {
        bool? visible = SHUU_Debug.Toggle_DebugColliders();

        if (visible == null) return new CommandReturn(Color.red, "Local debug disabled on this scene.");


        return new CommandReturn(Color.green, "Debug collider visibility " + (visible.Value ? "enabled." : "disabled."));
    }
    

    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static CommandReturn ScreenLogs()
    {
        bool? visible = SHUU_Debug.Toggle_ScreenLogs();

        if (visible == null) return new CommandReturn(Color.red, "Something went wrong, missing reference exception.");


        return new CommandReturn(Color.green, "Screen logs " + (visible.Value ? "enabled." : "disabled."));
    }
    #endregion



    #region Functional
    [DevConsoleCommand("loadscene", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn LoadScene(string sceneName, OptionalParameter<bool> loadingScreen)
    {
        if (loadingScreen.TryGetValue(out bool ls)) SHUU_General.GoToScene(sceneName, ls);
        else SHUU_General.GoToScene(sceneName, ls);

        return new CommandReturn(Color.green, $"Scene changed to {sceneName}.");
    }

    [DevConsoleCommand("loadscenedirect", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn LoadSceneDirect(string sceneName, OptionalParameter<bool> loadingScreen)
    {
        if (loadingScreen.TryGetValue(out bool ls)) SceneLoader.Load(sceneName, ls);
        else SceneLoader.Load(sceneName, ls);

        return new CommandReturn(Color.green, $"Scene changed to {sceneName}.");
    }
    #endregion



    #region Saving/Loading
    [DevConsoleCommand("save", "Saves all data temporarily", "Debug")]
    public static CommandReturn Save()
    {
        SHUU_Saving.SaveInfo();

        return new CommandReturn(Color.green, "Data saved successfully.");
    }

    [DevConsoleCommand("filesave", "Saves all data to json files", "Debug")]
    public static CommandReturn FileSave()
    {
        SHUU_Saving.SaveInfoToFile();

        return new CommandReturn(Color.green, "Data saved to file successfully.");
    }


    [DevConsoleCommand("load", "Loads all temporary data", "Debug")]
    public static CommandReturn Load()
    {
        SHUU_Saving.LoadInfo();

        return new CommandReturn(Color.green, "Data loaded successfully.");
    }

    [DevConsoleCommand("fileload", "Loads all data from json files", "Debug")]
    public static CommandReturn FileLoad()
    {
        SHUU_Saving.LoadInfoFromFile();

        return new CommandReturn(Color.green, "Data loaded from file successfully.");
    }


    [DevConsoleCommand("backup", "Backs up the save data on the save file (not local)", "Debug")]
    public static CommandReturn Backup()
    {
        SHUU_Saving.Backup();

        return new CommandReturn(Color.green, "Data backed up from file successfully.");
    }

    [DevConsoleCommand("restorebackup", "Restores backed up info into the save file", "Debug")]
    public static CommandReturn RestoreBackup(OptionalParameter<int> index)
    {
        int i = index.TryGetValue(out int o) ? o : 0;
        
        SHUU_Saving.RestoreBackup(i);

        return new CommandReturn(Color.green, "Data restored into file successfully.");
    }


    [DevConsoleCommand("saveprefs", "Saves all player prefs", "Debug")]
    public static CommandReturn SavePrefs()
    {
        PlayerPrefs.Save();

        return new CommandReturn(Color.green, "PlayerPrefs saved successfully.");
    }

    [DevConsoleCommand("deleteprefs", "Deletes all player prefs", "Debug")]
    public static CommandReturn DeletePrefs()
    {
        PlayerPrefs.DeleteAll();

        return new CommandReturn(Color.green, "PlayerPrefs deleted successfully.");
    }
    #endregion

    #endregion




    #region Utilities

    #region Variables
    [DevConsoleCommand("setvar", "Set a variable", "Utilities")]
    public static CommandReturn SetVar(string name, params string[] values)
    {
        SavedConsoleVariables.Set(name, new List<string>(values));

        return new CommandReturn(Color.green, $"Variable '{name}' set.");
    }


    [DevConsoleCommand("delvar", "Delete variable", "Utilities")]
    public static CommandReturn DelVar(string name)
    {
        if (!SavedConsoleVariables.Exists(name)) return new CommandReturn(Color.red, $"Variable '{name}' not found.");
        

        SavedConsoleVariables.Remove(name);

        return new CommandReturn(Color.green, $"Variable '{name}' removed.");
    }

    [DevConsoleCommand("clearvars", "Clear all variables", "Utilities")]
    public static CommandReturn ClearVars()
    {
        SavedConsoleVariables.Clear();

        return new CommandReturn(Color.green, "All variables cleared.");
    }
    #endregion



    #region Time
    [DevConsoleCommand("timescale", "Sets the game's timescale to the specified value", "Utilities")]
    public static CommandReturn TimeScale(float timeScale)
    {
        SHUU_Time.SetTimeScale(timeScale);

        return new CommandReturn(Color.green, $"Timescale set to {timeScale}.");
    }


    [DevConsoleCommand("pause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn Pause(bool toggle)
    {
        bool result;

        if (toggle) result = SHUU_Time.Pause();
        else result = SHUU_Time.Resume(); 

        if (result) return new CommandReturn(Color.green, "Timescale " + (toggle ? "paused." : "resumed."));
        else return new CommandReturn(Color.green, "Timescale was already " + (toggle ? "paused." : "resumed."));
    }

    [DevConsoleCommand("togglepause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn TogglePause()
    {
        bool toggle = SHUU_Time.TogglePause();

        return new CommandReturn(Color.green, "Timescale " + (toggle ? "paused." : "unpaused."));
    }
    #endregion



    #region Settings
    [DevConsoleCommand("setsettings", "Sets a field on the global settings data", "Utilities")]
    public static CommandReturn SetSettings(string settingsDataName, string fieldName, MutableParameter value)
    {
        if (value.TryGetValue(out bool b)) return TrySetFieldValue(settingsDataName, fieldName, b);
        else if (value.TryGetValue(out int i)) return TrySetFieldValue(settingsDataName, fieldName, i);
        else if (value.TryGetValue(out float f)) return TrySetFieldValue(settingsDataName, fieldName, f);
        else if (value.TryGetValue(out string s)) return TrySetFieldValue(settingsDataName, fieldName, s);
        else if (value.TryGetValue(out char c)) return TrySetFieldValue(settingsDataName, fieldName, c);

        return new CommandReturn(Color.red, "Unsupported value type.");
    }


    public static CommandReturn TrySetFieldValue(string name, string fieldName, object value)
    {
        SettingsData target = SettingsData.GetSettingsData(name);

        if (!target.SetField(fieldName, value)) return new CommandReturn(Color.red, $"Field '{fieldName}' not found on {target.name} or value {value} invalid for such field.");

        return new CommandReturn(Color.green, $"Field '{fieldName}' on {target.name} set to {value}.");
    }
    #endregion



    #region Screenshot
    [DevConsoleCommand("screenshot", "Takes a screenshot", "Utilities")]
    public static CommandReturn Screenshot(OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) showScreenshot = false;


        ScreenCaptureHelper.Capture(prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });

        if (showScreenshot) return new CommandReturn(Color.green, "Capturing screenshot...", "Opening screenshot...");
        else return new CommandReturn(Color.green, "Capturing screenshot...");
    }

    [DevConsoleCommand("scaleshot", "Takes a scaled screenshot", "Utilities")]
    public static CommandReturn Scaleshot(int scale, OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) showScreenshot = false;


        ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });

        if (showScreenshot) return new CommandReturn(Color.green, "Capturing scaled screenshot...", "Opening scaled screenshot...");
        else return new CommandReturn(Color.green, "Capturing scaled screenshot...");
    }


    [DevConsoleCommand("openshot", "Opens the last saved screenshot in the file browser", "Utilities")]
    public static CommandReturn OpenShot()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return new CommandReturn(Color.red, "No screenshot has been taken yet");

        ScreenCaptureHelper.OpenLastScreenshot();

        return new CommandReturn(Color.green, "Opening last screenshot...");
    }
    #endregion

    #endregion




    #region Classic Input
    [DevConsoleCommand("bindcommand", "Binds a Dev Console command to a keycode or mouse button", "Classic Input")]
    public static CommandReturn BindCommand(string _input, params string[] commandData)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return new CommandReturn(Color.red, $"Invalid input '{_input}'");

        BoundCommands.BindCommand(input, commandData);

        return new CommandReturn(Color.green, $"Command bound to {_input} successfully.");
    }


    [DevConsoleCommand("unbindcommands", "Unbinds all Dev Console commands previously bound to a keycode or mouse button", "Classic Input")]
    public static CommandReturn UnBindCommands(string _input)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return new CommandReturn(Color.red, $"Invalid input '{_input}'");

        BoundCommands.UnBindCommands(input);

        return new CommandReturn(Color.green, $"All commands unbound from {_input} successfully.");
    }
    #endregion
}
