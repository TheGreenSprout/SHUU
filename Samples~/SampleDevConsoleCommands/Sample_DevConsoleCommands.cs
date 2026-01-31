using SHUU.UserSide.Commons;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.SettingsSytem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    [DevConsoleCommand("help", "Lists all commands", "Information")]
    public static (string[], Color?) Help()
    {
        var cmds = DevCommandRegistry.AllCommands().ToList();
        if (!cmds.Any()) return (new[] { "No commands registered." }, Color.red);

        List<string> tagOrder = DevConsoleManager.instance.tagList;

        int fallbackIndex = tagOrder.Count - 2;

        var grouped = new Dictionary<int, Dictionary<string, List<string>>>();

        foreach (var (name, info) in cmds)
        {
            int tagIndex = tagOrder.IndexOf(info.Tag);
            if (tagIndex == -1)
                tagIndex = fallbackIndex;

            if (!grouped.TryGetValue(tagIndex, out var tagGroups))
            {
                tagGroups = new Dictionary<string, List<string>>();
                grouped[tagIndex] = tagGroups;
            }

            if (!tagGroups.TryGetValue(info.Tag, out var list))
            {
                list = new List<string>();
                tagGroups[info.Tag] = list;
            }

            var parameters = info.Method.GetParameters();
            string paramString = parameters.Length == 0
                ? ""
                : " (" + string.Join(", ", parameters.Select(p =>
                {
                    bool isParams = Attribute.IsDefined(p, typeof(ParamArrayAttribute));
                    if (isParams)
                    {
                        Type elemType = p.ParameterType.GetElementType();
                        return $"params {ParseParameter(elemType)}[]";
                    }
                    return ParseParameter(p.ParameterType);
                })) + ")";

            list.Add($"{name}{paramString} - {info.Description}");
        }

        var output = new List<string>();

        foreach (var index in grouped.Keys.OrderBy(i => i))
        {
            foreach (var tagGroup in grouped[index])
            {
                output.Add(tagGroup.Key);

                foreach (var cmd in tagGroup.Value.OrderBy(c => c))
                    output.Add("  " + cmd);

                output.Add(string.Empty);
            }
        }

        return (output.ToArray(), null);
    }

    private static string ParseParameter(Type t)
    {
        // Detect OptionalParameter<T>
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OptionalParameter<>))
        {
            Type inner = t.GetGenericArguments()[0];
            return "~" + ParseParameter(inner);
        }


        // Primitive C# types
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



    [DevConsoleCommand("colliders", "Toggles the visibility of all colliders in the game", "Debug")]
    public static (string[], Color?) ShowColliders()
    {
        bool? visible = SHUU_Debug.Toggle_DebugColliders();

        if (visible == null) return (new string[] { "Local debug disabled on this scene" }, null);


        return (new string[] { "Debug collider visibility " + (visible.Value ? "enabled" : "disabled") }, null);
    }

    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static (string[], Color?) ToggleScreenLogs()
    {
        bool? visible = SHUU_Debug.Toggle_ScreenLogs();

        if (visible == null) return (new string[] { "Something went wrong, missing reference exception" }, null);


        return (new string[] { "Screen logs " + (visible.Value ? "enabled" : "disabled") }, null);
    }



    [DevConsoleCommand("loadscene", "Changes the scene to the specified scene name", "Debug")]
    public static (string[], Color?) Save(string sceneName)
    {
        SHUU_GlobalsProxy.generalManager.GoToScene(sceneName);


        return (new string[] { $"Scene changed to {sceneName}" }, null);
    }



    [DevConsoleCommand("save", "Saves all data temporarily", "Debug")]
    public static (string[], Color?) Save()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfo();


        return (new string[] { "Data saved successfully" }, null);
    }

    [DevConsoleCommand("filesave", "Saves all data to json files", "Debug")]
    public static (string[], Color?) SaveFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfoToFile();


        return (new string[] { "Data saved to file successfully" }, null);
    }


    [DevConsoleCommand("load", "Loads all temporary data", "Debug")]
    public static (string[], Color?) Load()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfo();


        return (new string[] { "Data loaded successfully" }, null);
    }

    [DevConsoleCommand("fileload", "Loads all data from json files", "Debug")]
    public static (string[], Color?) LoadFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfoFromFile();


        return (new string[] { "Data loaded from file successfully" }, null);
    }


    [DevConsoleCommand("saveprefs", "Saves all player prefs", "Debug")]
    public static (string[], Color?) SavePlayerPrefs()
    {
        PlayerPrefs.Save();


        return (new string[] { "PlayerPrefs saved successfully" }, null);
    }

    [DevConsoleCommand("deleteprefs", "Deletes all player prefs", "Debug")]
    public static (string[], Color?) DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();

        return (new string[] { "PlayerPrefs deleted successfully" }, null);
    }



    [DevConsoleCommand("timescale", "Sets the game's timescale to the specified value", "Utilities")]
    public static (string[], Color?) SetTimeScale(float timeScale)
    {
        TimeController.SetTimeScale(timeScale);


        return (new string[] { $"Timescale set to {timeScale}" }, null);
    }

    [DevConsoleCommand("pause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static (string[], Color?) Pause()
    {
        TimeController.TogglePause();


        return (new string[] { "Timescale paused/unpaused (toggle)" }, null);
    }



    [DevConsoleCommand("setsettings", "Sets a field on the global settings data", "Utilities")]
    public static (string[], Color?) TrySetSettingsValue(string settingsDataName, string fieldName, MutableParameter value)
    {
        (string[], Color?) returnVal = (null, null);


        if (value.TryGetValue(out bool b)) returnVal = TrySetFieldValue(settingsDataName, fieldName, b);
        else if (value.TryGetValue(out int i)) returnVal = TrySetFieldValue(settingsDataName, fieldName, i);
        else if (value.TryGetValue(out float f)) returnVal = TrySetFieldValue(settingsDataName, fieldName, f);
        else if (value.TryGetValue(out string s)) returnVal = TrySetFieldValue(settingsDataName, fieldName, s);
        else if (value.TryGetValue(out char c)) returnVal = TrySetFieldValue(settingsDataName, fieldName, c);
        else returnVal = (new string[] { "Unsupported value type" }, Color.red);


        return returnVal;
    }


    public static (string[], Color?) TrySetFieldValue(string name, string fieldName, object value)
    {
        SettingsData target = SettingsTracker.GetSettingsData(name);

        if (!target.SetField(fieldName, value)) return (new string[] { $"Field '{fieldName}' not found on {target.name} or value {value} invalid for such field." }, Color.red);


        return (null, null);
    }



    [DevConsoleCommand("screenshot", "Takes a screenshot", "Utilities")]
    public static (string[], Color?) Screenshot(OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        
        if (!_showScreenshot.TryGetValue(out bool showScreenshot))
        {
            ScreenCaptureHelper.Capture(prefix, customDir, false, new GameObject[] { DevConsoleManager.instance.gameObject });

            return (new string[] { "Capturing screenshot..." }, null);
        }
        else
        {
            ScreenCaptureHelper.Capture(prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });

            return (new string[] { "Capturing screenshot...", "Opening screenshot..." }, null);
        }
    }

    [DevConsoleCommand("scaleshot", "Takes a scaled screenshot", "Utilities")]
    public static (string[], Color?) ScaledScreenshot(int scale, OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, false, new GameObject[] { DevConsoleManager.instance.gameObject });
        else ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });


        return (new string[] { "Capturing scaled screenshot..." }, null);
    }


    [DevConsoleCommand("openshot", "Opens the last saved screenshot in the file browser", "Utilities")]
    public static (string[], Color?) OpenLastShot()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return (new string[] { "No screenshot has been taken yet"}, Color.red);


        ScreenCaptureHelper.OpenLastScreenshot();
        

        return (new string[] { "Opening last screenshot..." }, null);
    }


    [DevConsoleCommand("shotlocation", "Shows the last saved screenshot's file location", "Information")]
    public static (string[], Color?) LastShotLocation()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return (new string[] { "No screenshot has been taken yet"}, Color.red);


        return (new string[] { $"Last screenshot saved at: {ScreenCaptureHelper.lastPath}" }, null);
    }



    [DevConsoleCommand("pools", "Displays all object pool information", "Information")]
    public static (string[], Color?) ShowPools()
    {
        if (ObjectPooling.pools.Count == 0) return (new string[] { "No object pools found" }, Color.yellow);


        string[] lines = new string[ObjectPooling.pools.Count + 1];
        lines[0] = "Object Pools:";

        int i = 1;
        foreach (var pool in ObjectPooling.pools)
        {
            lines[i] = $"- Name: {pool.name}, Type: {pool.GetItemType().Name}, Total: {pool.totalCount}, In Pool: {pool.poolCount}";
            i++;
        }


        return (lines, null);
    }



    [DevConsoleCommand("bindcommand", "Binds a Dev Console command to a keycode or mouse button", "Classic Input")]
    public static (string[], Color?) BindCommand(string _input, params string[] commandData)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return (new string[] { $"Invalid input '{_input}'" }, Color.red);


        BoundCommands.BindCommand(input, commandData);


        return (new string[] { $"Command bound to {_input} successfully" }, null);
    }

    [DevConsoleCommand("unbindcommands", "Unbinds all Dev Console commands previously bound to a keycode or mouse button", "Classic Input")]
    public static (string[], Color?) UnBindCommands(string _input)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return (new string[] { $"Invalid input '{_input}'" }, Color.red);


        BoundCommands.UnBindCommands(input);


        return (new string[] { $"All commands unbound from {_input} successfully" }, null);
    }
}
