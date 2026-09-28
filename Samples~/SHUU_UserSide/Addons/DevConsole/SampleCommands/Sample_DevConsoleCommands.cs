using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

using SHUU.InnerWorkings;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.SettingsSystem;
using SHUU.Utils.SceneManagement;
using SHUU.UserSide.Addons.CameraShakeSystem;

public class Sample_DevConsoleCommands : MonoBehaviour
{
    #region Information

    #region Help
    [DevConsoleCommand("help", "Lists all commands", "Information")]
    public static CommandReturn Help(OptionalParameter<string> command) => command.TryGetValue(out var c) ? Help_Single(c) : Help_All();

    #region Help Internal
    private static CommandReturn HelpRet = null;
    private static Dictionary<string, CommandReturn> HelpCommandRet = new();
    
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

            string display = $"{name}{paramString}";

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
    #endregion


    [DevConsoleCommand("find", "Searches the names and descriptions of all commands", "Information")]
    public static CommandReturn Find(params string[] text)
    {
        string query = string.Join(" ", text).Trim();
        if (query.Length == 0) return CommandReturn.Red("Give some text to search for.");


        List<string> lines = new List<string>();

        foreach (var (name, info) in DevCommandRegistry.AllCommands().OrderBy(c => c.Item1))
        {
            bool nameMatches = name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
            bool descriptionMatches = !string.IsNullOrEmpty(info.Description) && info.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

            if (!nameMatches && !descriptionMatches) continue;


            var parameters = info.Method.GetParameters();
            string paramString = parameters.Length == 0 ? "" : " (" + string.Join(", ", parameters.Select(FormatParameter)) + ")";

            lines.Add($"{name}{paramString}");
        }

        if (lines.Count == 0) return CommandReturn.Yellow($"No commands match '{query}'.");

        return new CommandReturn(lines.ToArray());
    }


    [DevConsoleCommand("explain", "Explains how a part of the console works: vars, query, exec or quotes (with no topic it lists them)", "Information")]
    public static CommandReturn Explain(OptionalParameter<ExplainTopic> topic)
    {
        if (!topic.TryGetValue(out ExplainTopic chosen))
            return new CommandReturn("Topics: " + string.Join(", ", Enum.GetNames(typeof(ExplainTopic)).Select(name => name.ToLowerInvariant())), "Use  explain <topic>");

        switch (chosen)
        {
            case ExplainTopic.Vars: return ExplainVars();
            case ExplainTopic.Query: return ExplainQuery();
            case ExplainTopic.Quotes: return ExplainQuotes();

            default: return ExplainExec();
        }
    }

    #region Explain internal
    public enum ExplainTopic { Vars, Query, Exec, Quotes }

    private static CommandReturn ExplainQuotes() => new CommandReturn(
        "Quotes: keep words that have spaces in them together.",
        "",
        "Writing them",
        "  Words are separated by spaces. Put a word with spaces in double quotes and it counts as one:",
        "    echo \"Hello  world\"                               prints: Hello  world",
        "    set Transform gameObject.name=\"Main Camera\" position 0,5,0",
        "  Quotes can start in the middle of a word (name=\"Main Camera\"). They're removed before the command gets the word.",
        "  \"\" is a word with nothing in it.",
        "  To write a quote inside quotes, put a backslash before it:  \"say \\\"hi\\\"\"",
        "  A backslash only matters right before a quote, so paths like C:\\Games\\Saves work as they are.",
        "",
        "With variables",
        "  A variable keeps its words apart, so a word with spaces stays one word when it's put in a command:",
        "    setvar target \"Main Camera\"",
        "    set Transform gameObject.name=$target position 0,5,0     ->  gameObject.name=\"Main Camera\"",
        "  Inside quotes the variable's words are just joined:  echo \"hello $target\"  ->  hello Main Camera",
        "  var and allvars show the words with quotes where they're needed.",
        "",
        "Good to know",
        "  Bound commands (bindcommand...) keep their quotes too.",
        "  A quote that's never closed is an error, and the command doesn't run.");

    private static CommandReturn ExplainVars() => new CommandReturn(
        "Variables: text you can reuse in any command.",
        "",
        "Making them",
        "  setvar <name> <words...>   e.g.  setvar levels Menu Level1 Level2   (a word with spaces goes in quotes, see: explain quotes)",
        "  var <name> shows one   allvars lists them all   delvar <name> removes one   clearvars removes all",
        "",
        "Using them",
        "  Write $name in a command and it's replaced by the words before the command runs:",
        "    setvar speed 0.5",
        "    timescale $speed        ->  timescale 0.5",
        "  It works in the middle of a word too, next to anything that isn't a letter, number or _:",
        "    setvar health 50",
        "    setvar ammunition 30",
        "    query Enemy hp<$health,ammo=$ammunition        ->  query Enemy hp<50,ammo=30",
        "  Names start with a letter or _, then use letters, numbers and _. They aren't case sensitive.",
        "  A $ that isn't followed by a name (like cost$5, or a lone $) is left alone.",
        "  A variable with several words becomes several words, so it can fill several parameters. A word that has spaces in it stays one word.",
        "  A bound command (bindcommand...) has its variables replaced when you bind it, so it's stored with the values in it.",
        "",
        "Picking some of the words",
        "  $name>X      skips the first X words          $levels>1     ->  Level1 Level2",
        "  $name<Y      keeps only the first Y words     $levels<2     ->  Menu Level1",
        "  $name>X<Y    skips X, then keeps Y            $levels>1<1   ->  Level1",
        "    loadscene $levels>1<1        ->  loadscene Level1",
        "  Write them right after the name. A < or > only counts when a number follows it.",
        "",
        "Good to know",
        "  setvar doesn't expand variables in its own line: 'setvar b $a' stores the text $a,",
        "    so b follows a whenever it's used (up to 16 levels deep).",
        "  Using a variable that doesn't exist prints an error and the command doesn't run.",
        "  A line can be up to 10000 characters once the variables are replaced (this stops a variable that contains itself).",
        "  Variables are saved when the scene changes or the game closes, and loaded next time.");

    private static CommandReturn ExplainExec() => new CommandReturn(
        "Scripts: text files of console commands, run with exec.",
        "",
        "Writing one",
        "  One command per line, exactly as you'd type it (variables and queries work).",
        "  Blank lines and lines starting with // or # are ignored.",
        "    // debug setup",
        "    timescale 0.5",
        "    setvar levels Menu Level1 Level2",
        "",
        "Printing and waiting",
        "  echo <text>      prints text (rich text tags work), so a script can say what it's doing",
        "  wait <seconds>   pauses the script for that long (real time, so timescale doesn't change it)",
        "    echo Loading the level...",
        "    loadscene Level1",
        "    wait 2",
        "    echo Loaded.",
        "  While a script waits the console is free to use, but you can't start another script until it ends.",
        "  execstop cancels a script that's running or waiting.",
        "",
        "Running one",
        "  exec                           runs autoexec.txt (see below)",
        "  exec name                      runs name.txt from the console's data folder",
        "  exec folder/name.txt           a path starts from that folder too",
        "  exec C:/some/place/name.txt    a full path runs that file wherever it is",
        "  It stops at the first line that fails and says which one. A script can run another one (8 levels deep at most).",
        "",
        "Autoexec",
        "  autoexec.txt is the default script: exec with nothing after it runs it. It never runs by itself.",
        "  The console looks for it when the game starts and creates it, with a few comment lines, if it isn't there.",
        "  It's read each time it runs, so you can edit it while the game is running.",
        "",
        "Data folder:  " + ConsoleScripts.DataFolder);

    private static CommandReturn ExplainQuery() => new CommandReturn(
        "Queries: filters that pick things by their values. A command that takes a 'query' applies it to whatever it works on.",
        "  query <type> [query]                    lists the matching objects",
        "  inspect <type> [query] [name piece]     shows their fields and properties with the values",
        "  set <type> <query> <field> <value>      changes a field or property on all of them",
        "  destroy <type> <query>                  destroys their game objects",
        "  (set and destroy always need a query, so you can't change or wipe a whole type by accident: * matches all)",
        "",
        "Writing one",
        "  Conditions separated by commas, no spaces:  field<operator>value",
        "    hp<50                  one condition",
        "    hp<50,team=red         every condition has to match",
        "    *  (or -)              matches everything",
        "",
        "Operators",
        "  =   equals               !=  is different",
        "  <  <=  >  >=             compare numbers",
        "  ~   contains (text)",
        "",
        "Fields",
        "  Public fields and properties, not case sensitive. Use dots to go deeper:",
        "    transform.position.y>5      gameObject.name~Enemy      target.hp<=10",
        "",
        "Values",
        "  Text and enums ignore case (team=red, rarity=epic). Booleans are true or false.",
        "  Numbers use a dot (0.5). null checks for nothing:  target=null   target!=null",
        "  Values with spaces go in quotes:  gameObject.name=\"Main Camera\"  (see: explain quotes). Values can't contain commas.",
        "",
        "Try it",
        "  query Rigidbody mass>5,useGravity=false",
        "  query Light intensity>=1,type=Point",
        "  inspect Light type=Point range",
        "  set Light type=Point range 20          numbers use a dot; vectors are 1,2,3; colors are #ff0000 or red",
        "  set Transform gameObject.name=Player position 0,5,0",
        "  destroy Light intensity<0.1");
    #endregion


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
        if (t == typeof(QueryParameter)) return "query";
        if (t.IsEnum) return string.Join("|", Enum.GetNames(t)).ToLowerInvariant();

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

    [DevConsoleCommand("listsettings", "Lists the fields of a settings atlas (name and type), across all its maps or just one, e.g. 'listsettings Settings Audio'", "Utilities")]
    public static CommandReturn ListSettings(string atlasName, OptionalParameter<string> mapName)
    {
        SettingsAtlas atlas = SettingsAtlas.GetSettingsAtlas(atlasName);
        if (atlas == null) return CommandReturn.Red($"Atlas '{atlasName}' not found.");

        List<SettingMap> maps;

        if (mapName.TryGetValue(out string wanted))
        {
            if (!atlas.TryGetMap(wanted, out SettingMap map)) return CommandReturn.Red($"Map '{wanted}' not found on '{atlas.name}'.");

            maps = new List<SettingMap> { map };
        }
        else maps = atlas.maps;

        if (maps.Count == 0) return CommandReturn.Yellow($"'{atlas.name}' has no maps.");


        const int maxFieldsPerMap = 60;

        List<string> lines = new List<string>();

        foreach (SettingMap map in maps)
        {
            List<(string key, SettingField field)> all = map.fields.Select(f => (f.key, f)).ToList();

            foreach (SettingGroup group in map.groups)
                all.AddRange(group.fields.Select(f => (f.key, f)));

            lines.Add($"{map.mapName} ({all.Count} field(s)):");

            foreach (var (key, field) in all.Take(maxFieldsPerMap)) lines.Add($"  {key} : {DescribeSettingType(field)}");

            if (all.Count > maxFieldsPerMap) lines.Add($"  ... and {all.Count - maxFieldsPerMap} more.");
            else if (all.Count == 0) lines.Add("  (no fields)");
        }

        return new CommandReturn(lines.ToArray());
    }
    private static string DescribeSettingType(SettingField field)
    {
        if (field.type != SettingType.Enum) return field.type.ToString().ToLowerInvariant();

        Type resolved = field.Type();

        return resolved != null ? $"enum ({resolved.Name})" : $"enum (unresolved: {field.enumTypeName})";
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


    [DevConsoleCommand("stats", "Shows the FPS, memory usage and system info", "Information")]
    public static CommandReturn ShowStats()
    {
        float fps = Stats.Fps > 0f ? Stats.Fps : (Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f);
        float frameMs = fps > 0f ? 1000f / fps : 0f;

        const float bytesPerMb = 1024f * 1024f;

        return new CommandReturn(
            $"FPS: {fps:F1} ({frameMs:F2} ms) | Refresh rate: {Stats.Refreshrate:F0} Hz",
            $"Managed memory: {Stats.Monoused / bytesPerMb:F1} MB used / {Stats.Monoheap / bytesPerMb:F1} MB heap",
            $"Total memory: {Stats.Totalallocated / bytesPerMb:F1} MB allocated / {Stats.Totalreserved / bytesPerMb:F1} MB reserved",
            $"CPU: {Stats.Cpu} ({Stats.Cpucores} cores)",
            $"GPU: {Stats.Gpu} ({Stats.Gpumemory} MB)",
            $"OS: {Stats.Os}");
    }


    [DevConsoleCommand("scenes", "Lists the scenes in the build's Scene List (the ones loadscene can load)", "Information")]
    public static CommandReturn Scenes()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count == 0) return CommandReturn.Yellow("There are no scenes in the Scene List.");

        string current = SceneLoader.GetCurrentSceneName();

        string[] lines = new string[count];
        for (int i = 0; i < count; i++)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

            lines[i] = name == current ? $"{name} (current)" : name;
        }

        return new CommandReturn(lines);
    }
    #endregion



    [DevConsoleCommand("clear", "Clears the Developer Console's output", "Information")]
    public static CommandReturn Clear()
    {
        if (DevConsoleManager.Instance == null || DevConsoleManager.Instance.devConsoleUI == null)
            return CommandReturn.Red("DevConsole or DevConsoleUI missing from scene.");

        DevConsoleManager.Instance.devConsoleUI.Clear();

        return new CommandReturn(new string[0]);
    }

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
    [DevConsoleCommand("screenlogs", "Toggles whether Debug.Logs are displayed on screen", "Debug")]
    public static CommandReturn ScreenLogs()
    {
        bool? visible = SHUU_Debug.ScreenLogs_Toggle();

        if (visible == null) return CommandReturn.Red("Local screen logs not present in this scene.");


        return CommandReturn.Green("Screen logs " + (visible.Value ? "enabled." : "disabled."));
    }

    [DevConsoleCommand("screenlogslistener", "Toggles whether Debug.Logs are displayed on screen", "Debug")]
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
    public static CommandReturn LoadScene(string sceneName, OptionalParameter<bool> fade, OptionalParameter<bool> loadingScreen)
    {
        bool f = true;
        if (!fade.TryGetValue(out bool _fade)) f = false;
        bool ls = true;
        if (!loadingScreen.TryGetValue(out bool _loadingScreen)) ls = false;
        
        SHUU_General.GoToScene(sceneName, f ? _fade : null, ls ? _loadingScreen : null);

        return CommandReturn.Green($"Scene changed to {sceneName}.");
    }

    [DevConsoleCommand("loadscenedirect", "Changes the scene to the specified scene name", "Debug")]
    public static CommandReturn LoadSceneDirect(string sceneName, OptionalParameter<bool> loadingScreen)
    {
        if (loadingScreen.TryGetValue(out bool ls)) SceneLoader.Load(sceneName, ls);
        else SceneLoader.Load(sceneName);

        return CommandReturn.Green($"Scene changed to {sceneName}.");
    }

    [DevConsoleCommand("reloadscene", "Reloads the current scene (optionally fading out first)", "Debug")]
    public static CommandReturn ReloadScene(OptionalParameter<bool> fade)
    {
        if (SHUU_General.Instance == null) return CommandReturn.Red("There's no SHUU_General in the scene.");

        string scene = SceneLoader.GetCurrentSceneName();

        if (fade.TryGetValue(out bool useFade)) SHUU_General.GoToScene(scene, useFade);
        else SHUU_General.GoToScene(scene);

        return CommandReturn.Green($"Reloading {scene}.");
    }
    #endregion



    #region Query
    private static readonly Dictionary<string, Type> QueryTypeCache = new(StringComparer.OrdinalIgnoreCase);

    [DevConsoleCommand("query", "Lists the objects of a type in the scene that match a query, e.g. 'query Rigidbody mass>5,gameObject.name~Enemy' (no spaces; = != < <= > >= ~ ; dots reach into fields; * or - matches all)", "Debug")]
    public static CommandReturn QueryObjects(string typeName, OptionalParameter<QueryParameter> query)
    {
        CommandReturn error = FindMatches(typeName, query.TryGetValue(out QueryParameter q) ? q : null, out Type type, out int total, out List<UnityEngine.Object> matches);
        if (error != null) return error;


        if (matches.Count == 0) return CommandReturn.Yellow($"None of the {total} {type.Name} match.");

        const int maxLines = 40;

        List<string> lines = new List<string> { $"{matches.Count} of {total} {type.Name}:" };

        foreach (UnityEngine.Object match in matches.Take(maxLines)) lines.Add("  " + DescribeObject(match));

        if (matches.Count > maxLines) lines.Add($"  ... and {matches.Count - maxLines} more.");

        return new CommandReturn(lines.ToArray());
    }

    [DevConsoleCommand("destroy", "Destroys the game objects of the objects of a type that match a query, e.g. 'destroy Enemy hp<=0' (the query is required, * matches all; see: explain query)", "Debug")]
    public static CommandReturn DestroyObjects(string typeName, QueryParameter query)
    {
        CommandReturn error = FindMatches(typeName, query, out Type type, out int total, out List<UnityEngine.Object> matches);
        if (error != null) return error;

        if (type != typeof(GameObject) && !typeof(Component).IsAssignableFrom(type)) return CommandReturn.Red($"{type.Name} isn't a component or a game object, so there's nothing in the scene to destroy.");

        if (matches.Count == 0) return CommandReturn.Yellow($"None of the {total} {type.Name} match.");


        HashSet<GameObject> targets = new HashSet<GameObject>();
        int kept = 0;

        foreach (UnityEngine.Object match in matches)
        {
            GameObject target = match is Component component ? component.gameObject : (GameObject)match;

            if (DevConsoleManager.Instance.transform.IsChildOf(target.transform)) kept++;
            else targets.Add(target);
        }

        if (targets.Count == 0) return CommandReturn.Red("Every match has the console inside it, so nothing was destroyed.");


        const int maxLines = 15;

        List<string> lines = new List<string> { $"Destroyed {targets.Count} game object(s) ({matches.Count} {type.Name} matched):" };

        foreach (GameObject target in targets.Take(maxLines)) lines.Add("  " + DescribeObject(target.transform));

        if (targets.Count > maxLines) lines.Add($"  ... and {targets.Count - maxLines} more.");

        if (kept > 0) lines.Add($"Left {kept} alone because the console is inside them.");


        foreach (GameObject target in targets) Destroy(target);

        return new CommandReturn(Color.green, lines.ToArray());
    }


    [DevConsoleCommand("inspect", "Shows the fields and properties, with their values, of the objects of a type that match a query, e.g. 'inspect Rigidbody gameObject.name=Player' (optionally a piece of a name to show only some: 'inspect Rigidbody - mass')", "Debug")]
    public static CommandReturn Inspect(string typeName, OptionalParameter<QueryParameter> query, OptionalParameter<string> filter)
    {
        CommandReturn error = FindMatches(typeName, query.TryGetValue(out QueryParameter q) ? q : null, out Type type, out int total, out List<UnityEngine.Object> matches);
        if (error != null) return error;


        if (matches.Count == 0) return CommandReturn.Yellow($"None of the {total} {type.Name} match.");

        const int maxObjects = 3;
        const int maxMembers = 80;

        string wanted = filter.TryGetValue(out string piece) ? piece : null;

        List<string> lines = new List<string>();

        foreach (UnityEngine.Object match in matches.Take(maxObjects))
        {
            lines.Add($"{DescribeObject(match)} ({match.GetType().Name})");

            if (match is GameObject gameObject) lines.Add("  components: " + string.Join(", ", gameObject.GetComponents<Component>().Select(component => component == null ? "(missing script)" : component.GetType().Name)));

            List<MemberInfo> members = ObjectEditing.GetMembers(match.GetType());

            if (!string.IsNullOrEmpty(wanted)) members = members.Where(member => member.Name.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            foreach (MemberInfo member in members.Take(maxMembers)) lines.Add($"  {member.Name} = {ObjectEditing.ReadValue(member, match)}");

            if (members.Count == 0) lines.Add("  (no field or property has that in its name)");
            else if (members.Count > maxMembers) lines.Add($"  ... and {members.Count - maxMembers} more (give a piece of a name to narrow it down).");
        }

        if (matches.Count > maxObjects) lines.Add($"... and {matches.Count - maxObjects} more {type.Name} (narrow it with a query).");

        return new CommandReturn(lines.ToArray());
    }

    [DevConsoleCommand("set", "Sets a field or property on the objects of a type that match a query, e.g. 'set Rigidbody gameObject.name=Player mass 5' (the query is required, * matches all; vectors are 1,2,3 and colors are #ff0000 or red)", "Debug")]
    public static CommandReturn SetMembers(string typeName, QueryParameter query, string field, params string[] value)
    {
        string text = string.Join(" ", value);

        if (text.Length == 0) return CommandReturn.Red("Give the value to set.");


        CommandReturn error = FindMatches(typeName, query, out Type type, out int total, out List<UnityEngine.Object> matches);
        if (error != null) return error;

        if (matches.Count == 0) return CommandReturn.Yellow($"None of the {total} {type.Name} match.");


        List<(UnityEngine.Object match, ObjectEditing.SetEdit edit)> edits = new List<(UnityEngine.Object, ObjectEditing.SetEdit)>();

        foreach (UnityEngine.Object match in matches)
        {
            if (!ObjectEditing.TryPrepareSet(match, field, text, out ObjectEditing.SetEdit edit, out string problem)) return CommandReturn.Red($"{DescribeObject(match)}: {problem}", "Nothing was changed.");

            edits.Add((match, edit));
        }


        const int maxLines = 10;

        List<string> lines = new List<string>();
        List<string> failures = new List<string>();

        foreach ((UnityEngine.Object match, ObjectEditing.SetEdit edit) in edits)
        {
            string failure = edit.Apply();

            if (failure != null) failures.Add($"{DescribeObject(match)}: {failure}");
            else if (lines.Count < maxLines) lines.Add($"  {DescribeObject(match)}: {edit.oldText} -> {edit.newText}");
        }

        int changed = edits.Count - failures.Count;

        lines.Insert(0, $"Set {field} to {text} on {changed} of {edits.Count} {type.Name}:");

        if (changed > maxLines) lines.Add($"  ... and {changed - maxLines} more.");

        lines.AddRange(failures.Take(maxLines).Select(failure => "Couldn't change " + failure));

        return new CommandReturn(failures.Count == 0 ? Color.green : Color.yellow, lines.ToArray());
    }


    #region Helpers
    private static CommandReturn FindMatches(string typeName, QueryParameter query, out Type type, out int total, out List<UnityEngine.Object> matches)
    {
        matches = null;
        total = 0;

        type = FindObjectType(typeName);
        if (type == null) return CommandReturn.Red($"No Unity object type called '{typeName}' was found.");


        UnityEngine.Object[] found = FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);

        total = found.Length;

        try { matches = query != null ? query.Filter(found) : new List<UnityEngine.Object>(found); }
        catch (QueryException e) { return CommandReturn.Red(e.Message); }

        return null;
    }

    
    private static string DescribeObject(UnityEngine.Object obj)
    {
        if (obj is GameObject gameObject) obj = gameObject.transform;

        if (obj is not Component component) return obj.name;

        string path = component.name;

        for (Transform parent = component.transform.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;

        return component.gameObject.activeInHierarchy ? path : path + " (inactive)";
    }

    private static Type FindObjectType(string name)
    {
        if (QueryTypeCache.TryGetValue(name, out Type cached)) return cached;


        Type found = null;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            foreach (Type type in types)
            {
                if (!typeof(UnityEngine.Object).IsAssignableFrom(type)) continue;

                if (!string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase) && !string.Equals(type.FullName, name, StringComparison.OrdinalIgnoreCase)) continue;

                found = type;
                break;
            }

            if (found != null) break;
        }

        QueryTypeCache[name] = found;

        return found;
    }
    #endregion

    #endregion

    #endregion




    #region Utilities

    #region Custom Console Logic
    [DevConsoleCommand("setvar", "Set a variable", "Utilities")]
    public static CommandReturn SetVar(string name, params string[] values)
    {
        if (!SavedConsoleVariables.IsValidName(name)) return CommandReturn.Red($"'{name}' can't be a variable name: it has to start with a letter or _, then use only letters, numbers and _.");


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


    [DevConsoleCommand("exec", "Runs the commands in a text file, one per line: 'exec' alone runs autoexec.txt, 'exec name' runs name.txt from the console's data folder, 'exec some/folder/name.txt' runs that file (see: explain exec)", "Utilities")]
    public static CommandReturn Exec(params string[] file) => ConsoleScripts.Run(string.Join(" ", file));

    [DevConsoleCommand("execstop", "Stops the script that's running or waiting (see: explain exec)", "Utilities")]
    public static CommandReturn ExecStop() => ConsoleScripts.Cancel() ? CommandReturn.Green("Script stopped.") : CommandReturn.Yellow("No script is running.");

    [DevConsoleCommand("wait", "Inside a script: waits this many seconds (real time) before the next line (see: explain exec)", "Utilities")]
    public static CommandReturn Wait(float seconds) => ConsoleScripts.Wait(seconds);

    [DevConsoleCommand("echo", "Prints the text you give it, handy in scripts (rich text tags work in it)", "Utilities")]
    public static CommandReturn Echo(params string[] text) => new CommandReturn(string.Join(" ", text));


    [DevConsoleCommand("clearbinds", "Clears all bound commands across all input types", "Utilities")]
    public static CommandReturn ClearBinds()
    {
        BoundCommands.ClearAllBinds();

        return CommandReturn.Green("All bound commands cleared.");
    }
    #endregion



    #region Time
    [DevConsoleCommand("timescale", "Sets or gets the game's timescale to the specified value", "Utilities")]
    public static CommandReturn TimeScale(OptionalParameter<float> timeScale)
    {
        if (!timeScale.TryGetValue(out var tS)) return CommandReturn.Green($"Timescale: {SHUU_Time.CurrentTimeScale}.");

        SHUU_Time.SetTimeScale(tS);

        return CommandReturn.Green($"Timescale set to {timeScale}.");
    }


    [DevConsoleCommand("pause", "Pauses or resumes the game with the console's own pause (other things that pause the game aren't affected)", "Utilities")]
    public static CommandReturn Pause(bool toggle)
    {
        bool result;

        if (toggle) result = SHUU_Time.Pause();
        else result = SHUU_Time.Resume();

        if (!toggle && SHUU_Time.Paused)
            return CommandReturn.Yellow(result ? "The console's pause was removed, but the game is still paused by something else (see 'pauses')." : "The console wasn't pausing, and the game is paused by something else (see 'pauses').");

        if (result) return CommandReturn.Green("Timescale " + (toggle ? "paused." : "resumed."));
        else return CommandReturn.Green("Timescale was already " + (toggle ? "paused." : "resumed."));
    }

    [DevConsoleCommand("pauses", "Lists everything that is currently pausing the game", "Utilities")]
    public static CommandReturn Pauses()
    {
        string[] owners = SHUU_Time.GetPauseOwners();

        if (owners.Length == 0) return CommandReturn.Yellow("Nothing is pausing the game.");

        return new CommandReturn(owners);
    }

    [DevConsoleCommand("resumeall", "Removes every pause at once (for when something forgot to resume)", "Utilities")]
    public static CommandReturn ResumeAll()
    {
        int count = SHUU_Time.GetPauseOwners().Length;

        SHUU_Time.ResumeAll();

        return CommandReturn.Green(count == 0 ? "Nothing was pausing the game." : $"Removed {count} pause(s).");
    }

    [DevConsoleCommand("togglepause", "Toggles the game's timescale between paused and unpaused states", "Utilities")]
    public static CommandReturn TogglePause()
    {
        bool toggle = SHUU_Time.TogglePause();

        return CommandReturn.Green("Timescale " + (toggle ? "paused." : "unpaused."));
    }

    [DevConsoleCommand("step", "Advances the game by a single frame while it's paused", "Utilities")]
    public static CommandReturn Step()
    {
        if (SHUU_Time.Instance == null) return CommandReturn.Red("There's no SHUU_Time in the scene.");

        if (!SHUU_Time.StepFrame()) return CommandReturn.Yellow("The game isn't paused (use 'pause true' or 'togglepause' first).");

        return CommandReturn.Green("Stepped one frame.");
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



    #region Fades
    public enum FadeType { In, Out, PingPong }

    [DevConsoleCommand("fade", "Plays a fade: in, out or pingpong (optional duration in seconds, otherwise the defaults are used)", "Utilities")]
    public static CommandReturn Fade(FadeType type, OptionalParameter<float> duration)
    {
        if (SHUU_Fades.Instance == null) return CommandReturn.Red("There's no SHUU_Fades in the scene.");

        float? seconds = duration.TryGetValue(out float d) ? d : null;
        if (seconds < 0f) return CommandReturn.Red("Duration can't be negative.");

        try
        {
            switch (type)
            {
                case FadeType.In: SHUU_Fades.CreateFade_In(new FadeOptions { duration = seconds }); break;
                case FadeType.Out: SHUU_Fades.CreateFade_Out(new FadeOptions { duration = seconds }); break;

                default: SHUU_Fades.CreateFade_PingPong(new PingPong_FadeOptions { firstFade_duration = seconds, secondFade_duration = seconds }); break;
            }
        }
        catch (Exception e) { return CommandReturn.Red($"Couldn't start the fade: {e.Message}"); }

        return CommandReturn.Green($"Fade {type.ToString().ToLowerInvariant()} started.");
    }
    #endregion



    #region Camera Shake
    [DevConsoleCommand("shake", "Shakes every camera that has a CameraShake (intensity 0-1, duration in seconds).", "Debug")]
    public static CommandReturn Shake(OptionalParameter<float> intensity, OptionalParameter<float> duration)
    {
        if (SHUU_CameraShake.Shakes.Count == 0) return CommandReturn.Red("There's no active CameraShake in the scene.");

        if (!intensity.TryGetValue(out float amount)) amount = 0.6f;
        if (!duration.TryGetValue(out float seconds)) seconds = 0.5f;

        if (amount <= 0f) return CommandReturn.Red("Intensity must be above 0.");
        if (seconds <= 0f) return CommandReturn.Red("Duration must be above 0.");


        SHUU_CameraShake.Shake(amount, seconds);

        return CommandReturn.Green($"Shaking {SHUU_CameraShake.Shakes.Count} camera(s) (intensity {amount}, {seconds}s).");
    }

    [DevConsoleCommand("shaketrauma", "Adds trauma to every camera that has a CameraShake (0-1, stacks up to 1, fades at each camera's decay rate).", "Debug")]
    public static CommandReturn Trauma(OptionalParameter<float> amount)
    {
        if (SHUU_CameraShake.Shakes.Count == 0) return CommandReturn.Red("There's no active CameraShake in the scene.");

        if (!amount.TryGetValue(out float value)) value = 0.3f;

        if (value <= 0f) return CommandReturn.Red("Amount must be above 0.");


        SHUU_CameraShake.AddTrauma(value);

        float current = SHUU_CameraShake.Shakes.Max(s => s.Trauma);

        return CommandReturn.Green($"Added {value} trauma to {SHUU_CameraShake.Shakes.Count} camera(s) (now at {current:0.##}).");
    }

    [DevConsoleCommand("stopshake", "Stops the shake on every camera that has a CameraShake.", "Debug")]
    public static CommandReturn StopShake()
    {
        if (SHUU_CameraShake.Shakes.Count == 0) return CommandReturn.Red("There's no active CameraShake in the scene.");

        SHUU_CameraShake.Stop();

        return CommandReturn.Green("Shake stopped.");
    }
    #endregion



    #region Application
    [DevConsoleCommand("quit","Quits the application (stops play mode in the editor)", "Utilities")]
    public static CommandReturn Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif

        return CommandReturn.Green("Quitting...");
    }
    #endregion

    #endregion




    #region Classic Input
    
#if ENABLE_LEGACY_INPUT_MANAGER
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
                                                                                        ? $"Command '{ConsoleTokenizer.Join(commandData)}' not found on '{_actionPath}'."
                                                                                        : $"No commands bound to '{_actionPath}'.");

        return CommandReturn.Green(specific
            ? $"Command '{ConsoleTokenizer.Join(commandData)}' unbound from '{_actionPath}'."
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
#endif

    #endregion
}
