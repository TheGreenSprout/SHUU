using SHUU.UserSide.Commons;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.SettingsSytem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    [DevConsoleCommand("help", "Lists all commands", "Information")]
    public static CommandReturn Help()
    {
        var cmds = DevCommandRegistry.AllCommands().ToList();
        if (!cmds.Any()) return new CommandReturn(Color.red, "No commands registered.");

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

        return new CommandReturn(output.ToArray());
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
    public static CommandReturn ShowColliders()
    {
        bool? visible = SHUU_Debug.Toggle_DebugColliders();

        if (visible == null) return new CommandReturn("Local debug disabled on this scene.");


        return new CommandReturn("Debug collider visibility " + (visible.Value ? "enabled." : "disabled."));
    }

    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static CommandReturn ToggleScreenLogs()
    {
        bool? visible = SHUU_Debug.Toggle_ScreenLogs();

        if (visible == null) return new CommandReturn("Something went wrong, missing reference exception.");


        return new CommandReturn("Screen logs " + (visible.Value ? "enabled." : "disabled."));
    }



    [DevConsoleCommand("loadscene", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn Save(string sceneName)
    {
        SHUU_GlobalsProxy.generalManager.GoToScene(sceneName);


        return new CommandReturn($"Scene changed to {sceneName}");
    }



    [DevConsoleCommand("save", "Saves all data temporarily", "Debug")]
    public static CommandReturn Save()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfo();


        return new CommandReturn("Data saved successfully");
    }

    [DevConsoleCommand("filesave", "Saves all data to json files", "Debug")]
    public static CommandReturn SaveFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfoToFile();


        return new CommandReturn("Data saved to file successfully");
    }


    [DevConsoleCommand("load", "Loads all temporary data", "Debug")]
    public static CommandReturn Load()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfo();


        return new CommandReturn("Data loaded successfully");
    }

    [DevConsoleCommand("fileload", "Loads all data from json files", "Debug")]
    public static CommandReturn LoadFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfoFromFile();


        return new CommandReturn("Data loaded from file successfully");
    }


    [DevConsoleCommand("saveprefs", "Saves all player prefs", "Debug")]
    public static CommandReturn SavePlayerPrefs()
    {
        PlayerPrefs.Save();


        return new CommandReturn("PlayerPrefs saved successfully");
    }

    [DevConsoleCommand("deleteprefs", "Deletes all player prefs", "Debug")]
    public static CommandReturn DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();

        return new CommandReturn("PlayerPrefs deleted successfully");
    }



    [DevConsoleCommand("timescale", "Sets the game's timescale to the specified value", "Utilities")]
    public static CommandReturn SetTimeScale(float timeScale)
    {
        TimeController.SetTimeScale(timeScale);


        return new CommandReturn($"Timescale set to {timeScale}");
    }

    [DevConsoleCommand("pause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn Pause()
    {
        TimeController.TogglePause();


        return new CommandReturn("Timescale paused/unpaused (toggle)");
    }



    [DevConsoleCommand("setsettings", "Sets a field on the global settings data", "Utilities")]
    public static CommandReturn TrySetSettingsValue(string settingsDataName, string fieldName, MutableParameter value)
    {
        CommandReturn returnVal = null;


        if (value.TryGetValue(out bool b)) returnVal = TrySetFieldValue(settingsDataName, fieldName, b);
        else if (value.TryGetValue(out int i)) returnVal = TrySetFieldValue(settingsDataName, fieldName, i);
        else if (value.TryGetValue(out float f)) returnVal = TrySetFieldValue(settingsDataName, fieldName, f);
        else if (value.TryGetValue(out string s)) returnVal = TrySetFieldValue(settingsDataName, fieldName, s);
        else if (value.TryGetValue(out char c)) returnVal = TrySetFieldValue(settingsDataName, fieldName, c);
        else returnVal = new CommandReturn(Color.red, "Unsupported value type");


        return returnVal;
    }


    public static CommandReturn TrySetFieldValue(string name, string fieldName, object value)
    {
        SettingsData target = SettingsTracker.GetSettingsData(name);

        if (!target.SetField(fieldName, value)) return new CommandReturn(Color.red, $"Field '{fieldName}' not found on {target.name} or value {value} invalid for such field.");


        return null;
    }



    [DevConsoleCommand("screenshot", "Takes a screenshot", "Utilities")]
    public static CommandReturn Screenshot(OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        
        if (!_showScreenshot.TryGetValue(out bool showScreenshot))
        {
            ScreenCaptureHelper.Capture(prefix, customDir, false, new GameObject[] { DevConsoleManager.instance.gameObject });

            return new CommandReturn("Capturing screenshot...");
        }
        else
        {
            ScreenCaptureHelper.Capture(prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });

            return new CommandReturn("Capturing screenshot...", "Opening screenshot...");
        }
    }

    [DevConsoleCommand("scaleshot", "Takes a scaled screenshot", "Utilities")]
    public static CommandReturn ScaledScreenshot(int scale, OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, false, new GameObject[] { DevConsoleManager.instance.gameObject });
        else ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });


        return new CommandReturn("Capturing scaled screenshot...");
    }


    [DevConsoleCommand("openshot", "Opens the last saved screenshot in the file browser", "Utilities")]
    public static CommandReturn OpenLastShot()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return new CommandReturn(Color.red, "No screenshot has been taken yet");


        ScreenCaptureHelper.OpenLastScreenshot();
        

        return new CommandReturn("Opening last screenshot...");
    }


    [DevConsoleCommand("shotlocation", "Shows the last saved screenshot's file location", "Information")]
    public static CommandReturn LastShotLocation()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return new CommandReturn(Color.red, "No screenshot has been taken yet");


        return new CommandReturn($"Last screenshot saved at: {ScreenCaptureHelper.lastPath}");
    }



    [DevConsoleCommand("pools", "Displays all object pool information", "Information")]
    public static CommandReturn ShowPools()
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



    [DevConsoleCommand("bindcommand", "Binds a Dev Console command to a keycode or mouse button", "Classic Input")]
    public static CommandReturn BindCommand(string _input, params string[] commandData)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return new CommandReturn(Color.red, $"Invalid input '{_input}'");


        BoundCommands.BindCommand(input, commandData);


        return new CommandReturn($"Command bound to {_input} successfully");
    }

    [DevConsoleCommand("unbindcommands", "Unbinds all Dev Console commands previously bound to a keycode or mouse button", "Classic Input")]
    public static CommandReturn UnBindCommands(string _input)
    {
        (KeyCode?, int?, string) input = InputParser.ParseInput(_input);
        if (input == (null, null, null)) return new CommandReturn(Color.red, $"Invalid input '{_input}'");


        BoundCommands.UnBindCommands(input);


        return new CommandReturn($"All commands unbound from {_input} successfully");
    }
}
