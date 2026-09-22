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
using System.Reflection;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    #region Information

    #region Help
    private static CommandReturn HelpRet = null;
    private static Dictionary<string, CommandReturn> HelpCommandRet = new();

    [DevConsoleCommand("help", "Lists all commands", "Information")]
    public static CommandReturn Help(OptionalParameter<string> command) => command.TryGetValue(out var c) ? Help_Single(c) : Help_All();

    private static CommandReturn Help_Single(string command)
    {
        string key = command.ToLower();
        if (HelpCommandRet.TryGetValue(key, out var cached)) return cached;

        if (!DevCommandRegistry.TryGet(key, out var info)) return CommandReturn.Red($"Unknown command '{command}'. Use 'help' to list all commands.");


        var parameters = info.Method.GetParameters();

        string usage = key + string.Concat(parameters.Select(p =>
        {
            if (Attribute.IsDefined(p, typeof(ParamArrayAttribute))) return $" <{p.Name}...>";
            return IsOptional(p.ParameterType) ? $" [{p.Name}]" : $" <{p.Name}>";
        }));

        var lines = new List<string>
        {
            key,
            $"  Description: {(string.IsNullOrEmpty(info.Description) ? "-" : info.Description)}",
            $"  Tag: {info.Tag}",
            $"  Usage: {usage}",
        };

        if (parameters.Length > 0)
        {
            lines.Add("  Parameters:");

            foreach (var p in parameters)
            {
                string flags = "";
                if (Attribute.IsDefined(p, typeof(ParamArrayAttribute))) flags = " (multiple)";
                else if (IsOptional(p.ParameterType)) flags = " (optional)";

                lines.Add($"    {p.Name} : {FormatParameter(p)}{flags}");
            }
        }
        else lines.Add("  Parameters: none");

        var ret = new CommandReturn(lines.ToArray());
        HelpCommandRet[key] = ret;
        return ret;
    }

    private static CommandReturn Help_All()
    {
        if (HelpRet != null) return HelpRet;


        var cmds = DevCommandRegistry.AllCommands().ToList();
        if (!cmds.Any()) return CommandReturn.Red("No commands registered.");

        List<string> tagOrder = DevConsoleManager.Instance.tagList;
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

            string paramString = parameters.Length == 0 ? "" : " (" + string.Join(", ", parameters.Select(FormatParameter)) + ")";

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


    #region Helpers
    private static string ParseParameter(Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OptionalParameter<>))
        {
            Type inner = t.GetGenericArguments()[0];
            
            return "~" + ParseParameter(inner);
        }


        if (t == typeof(bool)) return "bool";
        if (t == typeof(byte)) return "byte";
        if (t == typeof(sbyte)) return "sbyte";
        if (t == typeof(short)) return "short";
        if (t == typeof(ushort)) return "ushort";
        if (t == typeof(int)) return "int";
        if (t == typeof(uint)) return "uint";
        if (t == typeof(long)) return "long";
        if (t == typeof(ulong)) return "ulong";
        if (t == typeof(float)) return "float";
        if (t == typeof(double)) return "double";
        if (t == typeof(decimal)) return "decimal";
        if (t == typeof(char)) return "char";
        if (t == typeof(string)) return "string";
        if (t == typeof(MutableParameter)) return "mutable";

        return t.Name;
    }


    private static bool IsOptional(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OptionalParameter<>);

    private static string FormatParameter(ParameterInfo p)
    {
        if (Attribute.IsDefined(p, typeof(ParamArrayAttribute))) return $"params {ParseParameter(p.ParameterType.GetElementType())}[]";

        return ParseParameter(p.ParameterType);
    }
    #endregion

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
        if (target == null) return CommandReturn.Red($"Atlas '{atlasName}' not found.");

        SettingField field = target.GetField(mapName, fieldName);
        if (field == null) return CommandReturn.Red($"Field '{fieldName}' not found on '{target.name}'.");

        string value = field.type switch
        {
            SettingType.Bool => field.boolValue.ToString(),
            SettingType.Int => field.intValue.ToString(),
            SettingType.Float => field.floatValue.ToString(),
            SettingType.String => field.stringValue,
            SettingType.Enum => field.Type() is Type t && t.IsEnum ? Enum.GetName(t, field.enumValue) ?? field.enumValue.ToString() : field.enumValue.ToString(),
            
            _ => null
        };

        if (value == null) return CommandReturn.Red($"Field '{fieldName}' has an unsupported type ({field.type}).");

        return new CommandReturn($"'{fieldName}' on '{target.name}' ({field.type}) = {value}");
    }


    [DevConsoleCommand("shotlocation", "Shows the last saved screenshot's file location", "Information")]
    public static CommandReturn Shotlocation()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.LastPath)) return CommandReturn.Red("No screenshot has been taken yet");

        return new CommandReturn($"Last screenshot saved at: {ScreenCaptureHelper.LastPath}");
    }


    [DevConsoleCommand("pools", "Displays all object pool information", "Information")]
    public static CommandReturn Pools()
    {
        if (ObjectPooling.Pools.Count == 0) return CommandReturn.Yellow("No object pools found");


        string[] lines = new string[ObjectPooling.Pools.Count + 1];
        lines[0] = "Object Pools:";

        int i = 1;
        foreach (var pool in ObjectPooling.Pools)
        {
            lines[i] = $"- Name: {pool.name}, Type: {pool.GetItemType().Name}, Total: {pool.totalCount}, In Pool: {pool.poolCount}";
            i++;
        }


        return new CommandReturn(lines);
    }


    [DevConsoleCommand("allbinds", "Lists all bound commands across all input types", "Information")]
    public static CommandReturn AllBinds()
    {
        BoundCommands.GetAllBinds(out var actions, out var direct, out var classicKeys, out var classicMouse);

        bool any = actions.Count > 0 || direct.Count > 0 || classicKeys.Count > 0 || classicMouse.Count > 0;
        if (!any) return CommandReturn.Yellow("No commands bound.");

        var lines = new List<string>();

        if (actions.Count > 0)
        {
            lines.Add("Action:");
            foreach (var kvp in actions)
                foreach (var cmd in kvp.Value)
                    lines.Add($"  {kvp.Key} → {cmd}");
        }

        if (direct.Count > 0)
        {
            lines.Add("Direct:");
            foreach (var kvp in direct)
                foreach (var cmd in kvp.Value)
                    lines.Add($"  {kvp.Key} → {cmd}");
        }

        if (classicKeys.Count > 0)
        {
            lines.Add("Classic Key:");
            foreach (var kvp in classicKeys)
                foreach (var cmd in kvp.Value)
                    lines.Add($"  {kvp.Key} → {cmd}");
        }

        if (classicMouse.Count > 0)
        {
            lines.Add("Classic Mouse:");
            foreach (var kvp in classicMouse)
                foreach (var cmd in kvp.Value)
                    lines.Add($"  Mouse{kvp.Key} → {cmd}");
        }

        return new CommandReturn(lines.ToArray());
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
        if (DevConsoleManager.Instance == null || DevConsoleManager.Instance.devConsoleUI == null)
            return CommandReturn.Red("DevConsole or DevConsoleUI missing from scene.");

        DevConsoleManager.Instance.devConsoleUI.scrollSensitivity = scroll;

        return new CommandReturn($"Developer Console scroll sensitivity set to: {scroll}");
    }

    [DevConsoleCommand("consoletypewriter", "Toggles the Developer Console's typewriter effect", "Information")]
    public static CommandReturn ConsoleTypewriter(OptionalParameter<bool> toggle)
    {
        if (DevConsoleManager.Instance == null || DevConsoleManager.Instance.devConsoleUI == null)
            return CommandReturn.Red("DevConsole or DevConsoleUI missing from scene.");

        if (toggle.TryGetValue(out bool t)) DevConsoleManager.Instance.typewriterLines = t;
        else DevConsoleManager.Instance.typewriterLines = !DevConsoleManager.Instance.typewriterLines;

        return new CommandReturn($"Developer Console typewriter effect set to: {DevConsoleManager.Instance.typewriterLines}");
    }

    [DevConsoleCommand("consoletypewriterdelay", "Changes the delay for the Developer Console's typewriter effect", "Information")]
    public static CommandReturn ConsoleTypewriterDelay(float delay)
    {
        if (DevConsoleManager.Instance == null || DevConsoleManager.Instance.devConsoleUI == null)
            return CommandReturn.Red("DevConsole or DevConsoleUI missing from scene.");
        
        DevConsoleManager.Instance.typewriterDelay = delay;

        return new CommandReturn($"Developer Console typewriter delay set to: {delay}");
    }

    [DevConsoleCommand("consoletypewritersfx", "Toggles the Developer Console's typewriter sound effect", "Information")]
    public static CommandReturn ConsoleTypewriterSFX(OptionalParameter<bool> toggle)
    {
        if (DevConsoleManager.Instance == null || DevConsoleManager.Instance.devConsoleUI == null)
            return CommandReturn.Red("DevConsole or DevConsoleUI missing from scene.");

        if (toggle.TryGetValue(out bool t)) DevConsoleManager.Instance.playTypewriterSFX = t;
        else DevConsoleManager.Instance.playTypewriterSFX = !DevConsoleManager.Instance.playTypewriterSFX;
        
        return new CommandReturn($"Developer Console typewriter sound effect set to: {DevConsoleManager.Instance.playTypewriterSFX}");
    }
    #endregion



    #region Screen Logs
    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static CommandReturn ScreenLogs()
    {
        bool? visible = SHUU_Debug.ScreenLogs_Toggle();

        if (visible == null) return CommandReturn.Red("Local screen logs not present in this scene.");


        return CommandReturn.Green("Screen logs " + (visible.Value ? "enabled." : "disabled."));
    }

    [DevConsoleCommand("screenlogslistener", "Toggles whether Debug.Logs are displayed on screen.", "Debug")]
    public static CommandReturn ScreenLogsListener()
    {
        bool? visible = SHUU_Debug.ScreenLogsListener_Toggle();

        if (visible == null) return CommandReturn.Red("Local screen logs disabled on this scene.");


        return CommandReturn.Green("Screen logs listener " + (visible.Value ? "enabled." : "disabled."));
    }
    #endregion



    #region Debug Colliders
    [DevConsoleCommand("colliders", "Toggles the visibility of all colliders in the game", "Debug")]
    public static CommandReturn Colliders(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_Toggle(t) : SHUU_Debug.DebugColliders_Toggle();

        if (visible == null) return CommandReturn.Red("Local debug disabled on this scene.");


        return CommandReturn.Green("Debug collider visibility " + (visible.Value ? "enabled." : "disabled."));
    }
    
    [DevConsoleCommand("colliderswire", "Toggles whether the wire of all colliders in the game render on top of everything or not", "Debug")]
    public static CommandReturn CollidersWire(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_ToggleWireRender(t) : SHUU_Debug.DebugColliders_ToggleWireRender();

        if (visible == null) return CommandReturn.Red("Local debug disabled on this scene.");


        return CommandReturn.Green("Debug collider wire render on top " + (visible.Value ? "enabled." : "disabled."));
    }
    [DevConsoleCommand("collidersfill", "Toggles whether the fill of all colliders in the game render on top of everything or not", "Debug")]
    public static CommandReturn CollidersFill(OptionalParameter<bool> toggle)
    {
        bool? visible = toggle.TryGetValue(out bool t) ? SHUU_Debug.DebugColliders_ToggleFillRender(t) : SHUU_Debug.DebugColliders_ToggleFillRender();

        if (visible == null) return CommandReturn.Red("Local debug disabled on this scene.");


        return CommandReturn.Green("Debug collider fill render on top " + (visible.Value ? "enabled." : "disabled."));
    }

    [DevConsoleCommand("collidersreload", "Reloads the debug collider cache", "Debug")]
    public static CommandReturn CollidersReload()
    {
        if (!SHUU_Debug.DebugColliders_CacheReload()) return CommandReturn.Red("Something went wrong.");

        return CommandReturn.Green("Debug collider: Cache reloaded.");
    }
    [DevConsoleCommand("colliderscache", "Caches the debug colliders", "Debug")]
    public static CommandReturn CollidersCache()
    {
        if (!SHUU_Debug.DebugColliders_CacheColliders()) return CommandReturn.Red("Something went wrong.");

        return CommandReturn.Green("Debug collider: Colliders cached.");
    }
    [DevConsoleCommand("collidersrebuild", "Rebuilds the debug collider cache", "Debug")]
    public static CommandReturn CollidersRebuild()
    {
        if (!SHUU_Debug.DebugColliders_RebuildCache()) return CommandReturn.Red("Something went wrong.");

        return CommandReturn.Green("Debug collider: Cache rebuilt.");
    }
    #endregion



    #region Functional
    [DevConsoleCommand("loadscene", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn LoadScene(string sceneName, OptionalParameter<bool> loadingScreen)
    {
        if (loadingScreen.TryGetValue(out bool ls)) SHUU_General.GoToScene(sceneName, ls);
        else SHUU_General.GoToScene(sceneName, ls);

        return CommandReturn.Green($"Scene changed to {sceneName}.");
    }

    [DevConsoleCommand("loadscenedirect", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn LoadSceneDirect(string sceneName, OptionalParameter<bool> loadingScreen)
    {
        if (loadingScreen.TryGetValue(out bool ls)) SceneLoader.Load(sceneName, ls);
        else SceneLoader.Load(sceneName, ls);

        return CommandReturn.Green($"Scene changed to {sceneName}.");
    }
    #endregion

    #endregion




    #region Utilities

    #region Variables
    [DevConsoleCommand("setvar", "Set a variable", "Utilities")]
    public static CommandReturn SetVar(string name, params string[] values)
    {
        SavedConsoleVariables.Set(name, new List<string>(values));

        return CommandReturn.Green($"Variable '{name}' set.");
    }


    [DevConsoleCommand("delvar", "Delete variable", "Utilities")]
    public static CommandReturn DelVar(string name)
    {
        if (!SavedConsoleVariables.Exists(name)) return CommandReturn.Red($"Variable '{name}' not found.");
        

        SavedConsoleVariables.Remove(name);

        return CommandReturn.Green($"Variable '{name}' removed.");
    }

    [DevConsoleCommand("clearvars", "Clear all variables", "Utilities")]
    public static CommandReturn ClearVars()
    {
        SavedConsoleVariables.Clear();

        return CommandReturn.Green("All variables cleared.");
    }


    [DevConsoleCommand("clearbinds", "Clears all bound commands across all input types", "Utilities")]
    public static CommandReturn ClearBinds()
    {
        BoundCommands.ClearAllBinds();

        return CommandReturn.Green("All bound commands cleared.");
    }
    #endregion



    #region Time
    [DevConsoleCommand("timescale", "Sets the game's timescale to the specified value", "Utilities")]
    public static CommandReturn TimeScale(float timeScale)
    {
        SHUU_Time.SetTimeScale(timeScale);

        return CommandReturn.Green($"Timescale set to {timeScale}.");
    }


    [DevConsoleCommand("pause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn Pause(bool toggle)
    {
        bool result;

        if (toggle) result = SHUU_Time.Pause();
        else result = SHUU_Time.Resume(); 

        if (result) return CommandReturn.Green("Timescale " + (toggle ? "paused." : "resumed."));
        else return CommandReturn.Green("Timescale was already " + (toggle ? "paused." : "resumed."));
    }

    [DevConsoleCommand("togglepause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn TogglePause()
    {
        bool toggle = SHUU_Time.TogglePause();

        return CommandReturn.Green("Timescale " + (toggle ? "paused." : "unpaused."));
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

        return CommandReturn.Red("Unsupported value type.");
    }

    private static CommandReturn TrySetFieldValue(string atlasName, string mapName, string fieldName, object value)
    {
        SettingsAtlas target = SettingsAtlas.GetSettingsAtlas(atlasName);

        if (!target.SetField(mapName, fieldName, value))
            return CommandReturn.Red($"Field '{fieldName}' not found on {target.name} or value {value} invalid for such field.");

        return CommandReturn.Green($"Field '{fieldName}' on {target.name} set to {value}.");
    }
    #endregion



    #region Screenshot
    [DevConsoleCommand("screenshot", "Takes a screenshot", "Utilities")]
    public static CommandReturn Screenshot(OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) showScreenshot = false;


        ScreenCaptureHelper.Capture(prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.Instance.gameObject });

        if (showScreenshot) return CommandReturn.Green("Capturing screenshot...", "Opening screenshot...");
        else return CommandReturn.Green("Capturing screenshot...");
    }

    [DevConsoleCommand("scaleshot", "Takes a scaled screenshot", "Utilities")]
    public static CommandReturn Scaleshot(int scale, OptionalParameter<bool> _showScreenshot, OptionalParameter<string> _prefix, OptionalParameter<string> _customDir)
    {
        if (!_prefix.TryGetValue(out string prefix)) prefix = null;
        if (!_customDir.TryGetValue(out string customDir)) customDir = null;

        if (!_showScreenshot.TryGetValue(out bool showScreenshot)) showScreenshot = false;


        ScreenCaptureHelper.CaptureScaled(scale, prefix, customDir, showScreenshot, new GameObject[] { DevConsoleManager.Instance.gameObject });

        if (showScreenshot) return CommandReturn.Green("Capturing scaled screenshot...", "Opening scaled screenshot...");
        else return CommandReturn.Green("Capturing scaled screenshot...");
    }


    [DevConsoleCommand("openshot", "Opens the last saved screenshot in the file browser", "Utilities")]
    public static CommandReturn OpenShot()
    {
        if (string.IsNullOrEmpty(ScreenCaptureHelper.LastPath)) return CommandReturn.Red("No screenshot has been taken yet");

        ScreenCaptureHelper.OpenLastScreenshot();

        return CommandReturn.Green("Opening last screenshot...");
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

        return CommandReturn.Green($"Command bound to '{controlId}' successfully.");
    }

    [DevConsoleCommand("unbindcommandsclassic", "Unbinds commands bound to an input action (all or one)", "Classic Input")]
    public static CommandReturn UnBindCommandsClassic(string _actionPath, params string[] commandData)
    {
        bool specific = commandData != null && commandData.Length > 0;

        if (!BoundCommands.UnBindCommands(_actionPath, specific ? commandData : null)) return CommandReturn.Red(specific
                                                                                        ? $"Command '{string.Join(" ", commandData)}' not found on '{_actionPath}'."
                                                                                        : $"No commands bound to '{_actionPath}'.");

        return CommandReturn.Green(specific
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
                error = CommandReturn.Red($"Mouse button index {mouseButton} out of range (0-6).");
                return false;
            }

            controlId = $"mouse:{mouseButton}";
            return true;
        }

        if (param.TryGetValue(out string keyName))
        {
            if (!Enum.TryParse(keyName, true, out KeyCode _))
            {
                error = CommandReturn.Red($"'{keyName}' is not a valid KeyCode.");
                return false;
            }

            controlId = keyName;
            return true;
        }

        error = CommandReturn.Red("Argument must be a KeyCode name (e.g. Space) or mouse button index (e.g. 0).");
        return false;
    }

    #endregion
}
