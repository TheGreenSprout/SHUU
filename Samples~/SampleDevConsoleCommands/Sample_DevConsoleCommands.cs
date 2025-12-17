using SHUU.UserSide.Commons;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using SHUU.Utils.PersistantInfo.General;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    [DevConsoleCommand("help", "Lists all commands")]
    public static (string[], Color?) Help()
    {
        var cmds = DevCommandRegistry.AllCommands();
        if (!cmds.Any()) return (new string[] { "No commands registered." }, Color.red);


        string[] commandList = new string[cmds.Count()];
        int i = 0;

        foreach (var (name, info) in cmds)
        {
            var parameters = info.Method.GetParameters();
            
            string paramString = parameters.Length == 0
                ? ""
                : " (" + string.Join(", ", parameters.Select((p, index) =>
                {
                    bool isParams = Attribute.IsDefined(p, typeof(ParamArrayAttribute));

                    if (isParams)
                    {
                        Type elemType = p.ParameterType.GetElementType();
                        return $"params {ParseParameter(elemType)}[]";
                    }

                    return ParseParameter(p.ParameterType);

                })) + ")";

            commandList[i] = $"{name}{paramString} - {info.Description}";
            i++;
        }

        return (commandList, null);
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



    /*[DevConsoleCommand("bindcommand", "Binds a Dev Console command to a keycode")]
    public static (string[], Color?) BindCommand(string key, params string[] commandInfo)
    {
        
    }*/



    [DevConsoleCommand("loadscene", "Changes the scene to the specified scene name")]
    public static (string[], Color?) Save(string sceneName)
    {
        SHUU_GlobalsProxy.generalManager.GoToScene(sceneName);


        return (new string[] { $"Scene changed to {sceneName}" }, null);
    }



    [DevConsoleCommand("save", "Saves all data temporarily")]
    public static (string[], Color?) Save()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfo();


        return (new string[] { "Data saved successfully" }, null);
    }

    [DevConsoleCommand("filesave", "Saves all data to json files")]
    public static (string[], Color?) SaveFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfoToFile();


        return (new string[] { "Data saved to file successfully" }, null);
    }


    [DevConsoleCommand("load", "Loads all temporary data")]
    public static (string[], Color?) Load()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfo();


        return (new string[] { "Data loaded successfully" }, null);
    }

    [DevConsoleCommand("fileload", "Loads all data from json files")]
    public static (string[], Color?) LoadFile()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfoFromFile();


        return (new string[] { "Data loaded from file successfully" }, null);
    }


    [DevConsoleCommand("saveprefs", "Saves all player prefs")]
    public static (string[], Color?) SavePlayerPrefs()
    {
        PlayerPrefs.Save();


        return (new string[] { "PlayerPrefs saved successfully" }, null);
    }

    [DevConsoleCommand("deleteprefs", "Deletes all player prefs")]
    public static (string[], Color?) DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();

        return (new string[] { "PlayerPrefs deleted successfully" }, null);
    }



    [DevConsoleCommand("timescale", "Sets the game's timescale to the specified value")]
    public static (string[], Color?) SetTimeScale(float timeScale)
    {
        TimeController.SetTimeScale(timeScale);


        return (new string[] { $"Timescale set to {timeScale}" }, null);
    }

    [DevConsoleCommand("pause", "Toggles the game's timescale between paused and unpaused states")]
    public static (string[], Color?) Pause()
    {
        TimeController.TogglePause();


        return (new string[] { "Timescale paused/unpaused (toggle)" }, null);
    }



    [DevConsoleCommand("setsettings", "Sets a field on the global settings data")]
    public static (string[], Color?) TrySetSettingsValue(string fieldName, MutableParameter value)
    {
        (string[], Color?) returnVal = (null, null);


        if (value.TryGetValue<bool>(out bool b)) returnVal = TrySetFieldValue<bool>(fieldName, b);
        else if (value.TryGetValue<int>(out int i)) returnVal = TrySetFieldValue<int>(fieldName, i);
        else if (value.TryGetValue<float>(out float f)) returnVal = TrySetFieldValue<float>(fieldName, f);
        else if (value.TryGetValue<string>(out string s)) returnVal = TrySetFieldValue<string>(fieldName, s);
        else if (value.TryGetValue<char>(out char c)) returnVal = TrySetFieldValue<char>(fieldName, c);
        else returnVal = (new string[] { "Unsupported value type" }, Color.red);


        return returnVal;
    }


    public static (string[], Color?) TrySetFieldValue<T>(string fieldName, T value)
    {
        SettingsData target = SettingsTracker.settings;

        FieldInfo field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public
        );

        if (field == null)
        {
            return (new string[] { $"Field '{fieldName}' not found on {target.name}" }, Color.red);
        }

        if (!field.FieldType.IsAssignableFrom(typeof(T)))
        {
            return (new string[] { $"Type mismatch. Field '{fieldName}' is {field.FieldType}, tried to assign {typeof(T)}" }, Color.red);
        }

        field.SetValue(target, value);
        return (new string[] { $"Field '{fieldName}' set successfully to {value}" }, null);
    }



    [DevConsoleCommand("screenshot", "Takes a screenshot")]
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

    [DevConsoleCommand("scaleshot", "Takes a scaled screenshot")]
    public static (string[], Color?) ScaledScreenshot(int scale, OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, false, new GameObject[] { DevConsoleManager.instance.gameObject });
        else ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.instance.gameObject });


        return (new string[] { "Capturing scaled screenshot..." }, null);
    }


    [DevConsoleCommand("openshot", "Opens the last saved screenshot in the file browser")]
    public static (string[], Color?) OpenLastShot()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return (new string[] { "No screenshot has been taken yet"}, Color.red);


        ScreenCaptureHelper.OpenLastScreenshot();
        

        return (new string[] { "Opening last screenshot..." }, null);
    }


    [DevConsoleCommand("shotlocation", "Shows the last saved screenshot's file location")]
    public static (string[], Color?) LastShotLocation()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.lastPath)) return (new string[] { "No screenshot has been taken yet"}, Color.red);


        return (new string[] { $"Last screenshot saved at: {ScreenCaptureHelper.lastPath}" }, null);
    }



    [DevConsoleCommand("pools", "Displays all object pool information")]
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



    [DevConsoleCommand("bindcommand", "Binds a Dev Console command to a keycode or mouse button")]
    public static (string[], Color?) BindCommand(string _input, params string[] commandData)
    {
        (KeyCode?, int?) input = InputParser.ParseInput(_input);
        if (input == (null, null)) return (new string[] { $"Invalid input '{_input}'" }, Color.red);


        BoundCommands.BindCommand(input, commandData);


        return (new string[] { $"Command bound to {_input} successfully" }, null);
    }

    [DevConsoleCommand("unbindcommands", "Unbinds all Dev Console commands previously bound to a keycode or mouse button")]
    public static (string[], Color?) UnBindCommands(string _input)
    {
        (KeyCode?, int?) input = InputParser.ParseInput(_input);
        if (input == (null, null)) return (new string[] { $"Invalid input '{_input}'" }, Color.red);


        BoundCommands.UnBindCommands(input);


        return (new string[] { $"All commands unbound from {_input} successfully" }, null);
    }
}
