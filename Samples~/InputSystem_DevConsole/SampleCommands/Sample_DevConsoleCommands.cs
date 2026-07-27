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
using SHUU.Utils.SettingsSystem;
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

        var sortedIndices = grouped.Keys.OrderBy(i => i).ToList();

        for (int i = 0; i < sortedIndices.Count; i++)
        {
            foreach (var tagGroup in grouped[sortedIndices[i]])
            {
                output.Add(tagGroup.Key);

                foreach (var cmd in tagGroup.Value.OrderBy(x => x.order))
                    output.Add("  " + cmd.text);
            }

            if (i < sortedIndices.Count - 1) output.Add("");
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


        return new CommandReturn($"{name} = {ret}");
    }


    [DevConsoleCommand("getsettings", "Gets the value of a field on the global settings data", "Utilities")]
    public static CommandReturn GetSettings(string atlasName, string mapName, string fieldName)
    {
        SettingsAtlas target = SettingsAtlas.GetSettingsAtlas(atlasName);
        if (target == null) return new CommandReturn(Color.red, $"Atlas '{atlasName}' not found.");

        SettingField field = target.GetField(mapName, fieldName);
        if (field == null) return new CommandReturn(Color.red, $"Field '{fieldName}' not found on '{target.name}'.");

        string value = field.type switch
        {
            SettingType.Bool => field.boolValue.ToString(),
            SettingType.Int => field.intValue.ToString(),
            SettingType.Float => field.floatValue.ToString(),
            SettingType.String => field.stringValue,
            SettingType.Enum => field.Type() is Type t && t.IsEnum ? Enum.GetName(t, field.enumValue) ?? field.enumValue.ToString() : field.enumValue.ToString(),
            
            _ => null
        };

        if (value == null) return new CommandReturn(Color.red, $"Field '{fieldName}' has an unsupported type ({field.type}).");

        return new CommandReturn($"'{fieldName}' on '{target.name}' ({field.type}) = {value}");
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

    #region Dev Console
    [DevConsoleCommand("consolescroll", "Changes the Developer Console's scroll sensitivity", "Information")]
    public static CommandReturn ConsoleScroll(float scroll)
    {
        if (DevConsoleManager.instance == null || DevConsoleManager.instance.devConsoleUI == null)
            return new CommandReturn(Color.red, "DevConsole or DevConsoleUI missing from scene.");

        DevConsoleManager.instance.devConsoleUI.scrollSensitivity = scroll;

        return new CommandReturn($"Developer Console scroll sensitivity set to: {scroll}");
    }

    [DevConsoleCommand("consoletypewriter", "Toggles the Developer Console's typewriter effect", "Information")]
    public static CommandReturn ConsoleTypewriter(OptionalParameter<bool> toggle)
    {
        if (DevConsoleManager.instance == null || DevConsoleManager.instance.devConsoleUI == null)
            return new CommandReturn(Color.red, "DevConsole or DevConsoleUI missing from scene.");

        if (toggle.TryGetValue(out bool t)) DevConsoleManager.instance.typewriterLines = t;
        else DevConsoleManager.instance.typewriterLines = !DevConsoleManager.instance.typewriterLines;

        return new CommandReturn($"Developer Console typewriter effect set to: {DevConsoleManager.instance.typewriterLines}");
    }

    [DevConsoleCommand("consoletypewriterdelay", "Changes the delay for the Developer Console's typewriter effect", "Information")]
    public static CommandReturn ConsoleTypewriterDelay(float delay)
    {
        if (DevConsoleManager.instance == null || DevConsoleManager.instance.devConsoleUI == null)
            return new CommandReturn(Color.red, "DevConsole or DevConsoleUI missing from scene.");
        
        DevConsoleManager.instance.typewriterDelay = delay;

        return new CommandReturn($"Developer Console typewriter delay set to: {delay}");
    }

    [DevConsoleCommand("consoletypewritersfx", "Toggles the Developer Console's typewriter sound effect", "Information")]
    public static CommandReturn ConsoleTypewriterSFX(OptionalParameter<bool> toggle)
    {
        if (DevConsoleManager.instance == null || DevConsoleManager.instance.devConsoleUI == null)
            return new CommandReturn(Color.red, "DevConsole or DevConsoleUI missing from scene.");

        if (toggle.TryGetValue(out bool t)) DevConsoleManager.instance.playTypewriterSFX = t;
        else DevConsoleManager.instance.playTypewriterSFX = !DevConsoleManager.instance.playTypewriterSFX;
        
        return new CommandReturn($"Developer Console typewriter sound effect set to: {DevConsoleManager.instance.playTypewriterSFX}");
    }
    #endregion



    #region Screen Logs
    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static CommandReturn ScreenLogs()
    {
        bool? visible = SHUU_Debug.ScreenLogs_Toggle();

        if (visible == null) return new CommandReturn(Color.red, "Something went wrong, missing reference exception.");


        return new CommandReturn(Color.green, "Screen logs " + (visible.Value ? "enabled." : "disabled."));
    }
    #endregion



    #region Debug Colliders
    [DevConsoleCommand("colliders", "Toggles the visibility of all colliders in the game", "Debug")]
    public static CommandReturn Colliders(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_Toggle(t) : SHUU_Debug.DebugColliders_Toggle();

        if (visible == null) return new CommandReturn(Color.red, "Local debug disabled on this scene.");


        return new CommandReturn(Color.green, "Debug collider visibility " + (visible.Value ? "enabled." : "disabled."));
    }
    
    [DevConsoleCommand("colliderswire", "Toggles whether the wire of all colliders in the game render on top of everything or not", "Debug")]
    public static CommandReturn CollidersWire(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_ToggleWireRender(t) : SHUU_Debug.DebugColliders_ToggleWireRender();

        if (visible == null) return new CommandReturn(Color.red, "Local debug disabled on this scene.");


        return new CommandReturn(Color.green, "Debug collider wire render on top " + (visible.Value ? "enabled." : "disabled."));
    }
    [DevConsoleCommand("collidersfill", "Toggles whether the fill of all colliders in the game render on top of everything or not", "Debug")]
    public static CommandReturn CollidersFill(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_ToggleFillRender(t) : SHUU_Debug.DebugColliders_ToggleFillRender();

        if (visible == null) return new CommandReturn(Color.red, "Local debug disabled on this scene.");


        return new CommandReturn(Color.green, "Debug collider fill render on top " + (visible.Value ? "enabled." : "disabled."));
    }

    [DevConsoleCommand("collidersreload", "Reloads the debug collider cache", "Debug")]
    public static CommandReturn CollidersReload()
    {
        if (!SHUU_Debug.DebugColliders_CacheReload()) return new CommandReturn(Color.red, "Something went wrong.");

        return new CommandReturn(Color.green, "Debug collider: Cache reloaded.");
    }
    [DevConsoleCommand("colliderscache", "Caches the debug colliders", "Debug")]
    public static CommandReturn CollidersCache()
    {
        if (!SHUU_Debug.DebugColliders_CacheColliders()) return new CommandReturn(Color.red, "Something went wrong.");

        return new CommandReturn(Color.green, "Debug collider: Colliders cached.");
    }
    [DevConsoleCommand("collidersrebuild", "Rebuilds the debug collider cache", "Debug")]
    public static CommandReturn CollidersRebuild()
    {
        if (!SHUU_Debug.DebugColliders_RebuildCache()) return new CommandReturn(Color.red, "Something went wrong.");

        return new CommandReturn(Color.green, "Debug collider: Cache rebuilt.");
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
    public static CommandReturn SetSettings(string atlasName, string mapName, string fieldName, MutableParameter value)
    {
        if (value.TryGetValue(out bool b)) return TrySetFieldValue(atlasName, mapName, fieldName, b);
        else if (value.TryGetValue(out int i)) return TrySetFieldValue(atlasName, mapName, fieldName, i);
        else if (value.TryGetValue(out float f)) return TrySetFieldValue(atlasName, mapName, fieldName, f);
        else if (value.TryGetValue(out string s)) return TrySetFieldValue(atlasName, mapName, fieldName, s);
        else if (value.TryGetValue(out char c)) return TrySetFieldValue(atlasName, mapName, fieldName, c);

        return new CommandReturn(Color.red, "Unsupported value type.");
    }

    private static CommandReturn TrySetFieldValue(string atlasName, string mapName, string fieldName, object value)
    {
        SettingsAtlas target = SettingsAtlas.GetSettingsAtlas(atlasName);

        if (!target.SetField(mapName, fieldName, value))
            return new CommandReturn(Color.red, $"Field '{fieldName}' not found on {target.name} or value {value} invalid for such field.");

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

    [DevConsoleCommand("bindcommandclassic", "Binds a command to an input (KeyCode or int)", "Classic Input")]
    public static CommandReturn BindCommandClassic(MutableParameter _key, params string[] commandData)
    {
        if (!ResolveClassicInput(_key, out string controlId, out CommandReturn error)) return error;

        if (_key.TryGetValue(out int mouseButton)) BoundCommands.BindClassicCommand(mouseButton, commandData);
        else BoundCommands.BindClassicCommand((KeyCode)Enum.Parse(typeof(KeyCode), controlId, true), commandData);

        return new CommandReturn(Color.green, $"Command bound to '{controlId}' successfully.");
    }

    [DevConsoleCommand("unbindcommandsclassic", "Unbinds commands bound to an input action (all or one)", "Classic Input")]
    public static CommandReturn UnBindCommandsClassic(string _actionPath, params string[] commandData)
    {
        bool specific = commandData != null && commandData.Length > 0;

        if (!BoundCommands.UnBindCommands(_actionPath, specific ? commandData : null)) return new CommandReturn(Color.red, specific
                                                                                        ? $"Command '{string.Join(" ", commandData)}' not found on '{_actionPath}'."
                                                                                        : $"No commands bound to '{_actionPath}'.");

        return new CommandReturn(Color.green, specific
            ? $"Command '{string.Join(" ", commandData)}' unbound from '{_actionPath}'."
            : $"All commands unbound from '{_actionPath}'.");
    }


    private static bool ResolveClassicInput(MutableParameter param, out string controlId, out CommandReturn error)
    {
        error = null;
        controlId = null;

        if (param.TryGetValue(out int mouseButton))
        {
            if (mouseButton < 0 || mouseButton > 6)
            {
                error = new CommandReturn(Color.red, $"Mouse button index {mouseButton} out of range (0-6).");
                return false;
            }

            controlId = $"mouse:{mouseButton}";
            return true;
        }

        if (param.TryGetValue(out string keyName))
        {
            if (!Enum.TryParse(keyName, true, out KeyCode _))
            {
                error = new CommandReturn(Color.red, $"'{keyName}' is not a valid KeyCode.");
                return false;
            }

            controlId = keyName;
            return true;
        }

        error = new CommandReturn(Color.red, "Argument must be a KeyCode name (e.g. Space) or mouse button index (e.g. 0).");
        return false;
    }

    #endregion
}
