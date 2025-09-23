using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class AddingInputsHelper : EditorWindow
{
    private InputActionAsset inputAsset;
    private TextAsset customInputManagerScript;

    private Vector2 scroll;
    private List<ActionOptions> actionOptions = new();
    private List<(string mapName, string actionName)> existingOrder = new();

    private bool showLegend = true;
    private bool includeMapNameInVariables = true;

    private const string PrefKey_InputAsset = "AddingInputsHelper_InputAsset";
    private const string PrefKey_CustomScript = "AddingInputsHelper_CustomScript";
    private const string PrefKey_IncludeMapNames = "AddingInputsHelper_IncludeMapNames";

    [MenuItem("Tools/Input Sync Helper")]
    public static void ShowWindow()
    {
        var window = GetWindow<AddingInputsHelper>("Input Sync Helper");
        window.LoadPrefs();
    }

    private void OnEnable() => LoadPrefs();
    private void OnDisable() => SavePrefs();

    private void LoadPrefs()
    {
        string projectKey = Application.dataPath.GetHashCode().ToString();
        if (EditorPrefs.HasKey(projectKey + PrefKey_InputAsset))
        {
            string guid = EditorPrefs.GetString(projectKey + PrefKey_InputAsset);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        if (EditorPrefs.HasKey(projectKey + PrefKey_CustomScript))
        {
            string guid = EditorPrefs.GetString(projectKey + PrefKey_CustomScript);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            customInputManagerScript = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        includeMapNameInVariables = EditorPrefs.GetBool(projectKey + PrefKey_IncludeMapNames, true);
    }

    private void SavePrefs()
    {
        string projectKey = Application.dataPath.GetHashCode().ToString();
        if (inputAsset != null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(inputAsset));
            EditorPrefs.SetString(projectKey + PrefKey_InputAsset, guid);
        }
        if (customInputManagerScript != null)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(customInputManagerScript));
            EditorPrefs.SetString(projectKey + PrefKey_CustomScript, guid);
        }
        EditorPrefs.SetBool(projectKey + PrefKey_IncludeMapNames, includeMapNameInVariables);
    }

    private void OnGUI()
    {
        GUILayout.Label("Input Action Asset", EditorStyles.boldLabel);
        inputAsset = (InputActionAsset)EditorGUILayout.ObjectField("Asset", inputAsset, typeof(InputActionAsset), false);

        GUILayout.Label("CustomInputManager.cs", EditorStyles.boldLabel);
        customInputManagerScript = (TextAsset)EditorGUILayout.ObjectField("Script", customInputManagerScript, typeof(TextAsset), false);

        includeMapNameInVariables = EditorGUILayout.Toggle("Include Map Name in Variable Names", includeMapNameInVariables);

        showLegend = EditorGUILayout.Foldout(showLegend, "Legend");
        if (showLegend)
        {
            EditorGUILayout.HelpBox(
                "iv_ : Input Value (Vector2, float...)\n" +
                "it_ : Input Toggle (boolean)\n" +
                "icb_ : Input Callback [Pressed/Released] (Action)\n" +
                "iref_ : Input Reference (InputAction)",
                MessageType.None
            );
        }

        if (inputAsset == null || customInputManagerScript == null)
        {
            EditorGUILayout.HelpBox("Assign both InputActionAsset and CustomInputManager.cs", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Scan Inputs")) ScanInputs();

        if (actionOptions.Count > 0)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var opt in actionOptions)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"{opt.mapName}/{opt.actionName} ({opt.controlType})", EditorStyles.boldLabel);

                if (opt.alreadyExists)
                {
                    //EditorGUILayout.HelpBox("Previously defined in CustomInputManager.cs", MessageType.Info);
                }

                opt.variableName = EditorGUILayout.TextField("Variable Name", opt.variableName);
                opt.generateValue = EditorGUILayout.Toggle("Generate Value", opt.generateValue);
                opt.generateBool = EditorGUILayout.Toggle("Generate Bool (Down state)", opt.generateBool);
                opt.generatePressed = EditorGUILayout.Toggle("Generate Pressed Event", opt.generatePressed);
                opt.generateReleased = EditorGUILayout.Toggle("Generate Released Event", opt.generateReleased);
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Generate Code")) GenerateCode();
        }
    }

    private void ScanInputs()
    {
        actionOptions.Clear();
        existingOrder.Clear();

        string path = AssetDatabase.GetAssetPath(customInputManagerScript);
        string source = File.ReadAllText(path);

        var existingOptions = ParseExistingOptions(source);
        existingOrder = existingOptions.Select(e => (e.mapName, e.actionName)).ToList();

        foreach (var map in inputAsset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                var existing = existingOptions.FirstOrDefault(e => e.actionName == action.name && e.mapName == map.name);
                if (existing != null)
                {
                    existing.controlType = !string.IsNullOrEmpty(action.expectedControlType)
                        ? action.expectedControlType
                        : GetFallbackControlType(action);
                    actionOptions.Add(existing);
                }
                else
                {
                    actionOptions.Add(new ActionOptions
                    {
                        actionName = action.name,
                        variableName = SafeName(action.name),
                        mapName = map.name,
                        controlType = !string.IsNullOrEmpty(action.expectedControlType)
                            ? action.expectedControlType
                            : GetFallbackControlType(action),
                        generateValue = false,
                        generateBool = false,
                        generatePressed = false,
                        generateReleased = false,
                        alreadyExists = false
                    });
                }
            }
        }

        actionOptions = actionOptions
            .OrderBy(opt =>
            {
                int index = existingOrder.IndexOf((opt.mapName, opt.actionName));
                return index == -1 ? int.MaxValue : index;
            })
            .ToList();
    }

    private void GenerateCode()
    {
        string path = AssetDatabase.GetAssetPath(customInputManagerScript);
        string source = File.ReadAllText(path);

        string fields = GenerateFields();
        string awake = GenerateAwake();
        string onEnable = GenerateOnEnable();
        string onDisable = GenerateOnDisable();
        string callbacks = GenerateCallbackHandling();
        string update = GenerateUpdate();
        string summary = GenerateSummary();

        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_FIELDS", fields);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_AWAKE", awake);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_ONENABLE", onEnable);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_ONDISABLE", onDisable);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_CALLBACK_HANDLING", callbacks);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_UPDATE", update);
        source = ReplaceRegion(source, "AUTO_GENERATED_INPUT_SUMMARY", summary);

        File.WriteAllText(path, source, Encoding.UTF8);
        AssetDatabase.Refresh();
    }


    private string GenerateFields()
    {
        StringBuilder sb = new();
        sb.AppendLine("        /* Auto-generated Input fields.");
        sb.AppendLine("        This block is managed by the Input Action Sync tool.");
        sb.AppendLine("        DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        int i = 0;
        foreach (var mapGroup in actionOptions.GroupBy(a => a.mapName))
        {
            if (i != 0) { sb.AppendLine(); sb.AppendLine(); }
            i++;
            sb.AppendLine($"    // {mapGroup.Key} Map");

            var values = mapGroup.Where(o => o.generateValue).ToList();
            if (values.Count > 0)
            {
                sb.AppendLine("        // Values");
                foreach (var opt in values)
                    sb.AppendLine($"        public {GetCSharpType(opt.controlType)} iv_{GetVariableName(opt)} {{ get; private set; }}");
                sb.AppendLine();
            }

            var bools = mapGroup.Where(o => o.generateBool).ToList();
            if (bools.Count > 0)
            {
                sb.AppendLine("        // States");
                foreach (var opt in bools)
                    sb.AppendLine($"        public static bool it_{GetVariableName(opt)}Down {{ get; private set; }}");
                sb.AppendLine();
            }

            var events = mapGroup.Where(o => o.generatePressed || o.generateReleased).ToList();
            if (events.Count > 0)
            {
                sb.AppendLine("        // Events");
                foreach (var opt in events)
                {
                    if (opt.generatePressed)
                        sb.AppendLine($"        public static event Action icb_{GetVariableName(opt)}PressedAction;");
                    if (opt.generateReleased)
                        sb.AppendLine($"        public static event Action icb_{GetVariableName(opt)}ReleasedAction;");
                }
                sb.AppendLine();
            }

            sb.AppendLine("        // References");
            foreach (var opt in mapGroup)
                sb.AppendLine($"        private InputAction iref_{GetVariableName(opt)}InputAction;");
        }
        return sb.ToString();
    }

    private string GenerateAwake()
    {
        StringBuilder sb = new();
        sb.AppendLine("            /* Auto-generated Awake bindings.");
        sb.AppendLine("            This block is managed by the Input Action Sync tool.");
        sb.AppendLine("            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        int i = 0;
        foreach (var mapGroup in actionOptions.GroupBy(a => a.mapName))
        {
            if (i != 0) { sb.AppendLine(); sb.AppendLine(); }
            i++;
            sb.AppendLine($"        // {mapGroup.Key} Map");
            foreach (var opt in mapGroup)
                sb.AppendLine($"            iref_{GetVariableName(opt)}InputAction = InputSystem.actions.FindAction(\"{opt.actionName}\");");
        }
        return sb.ToString();
    }

    private string GenerateOnEnable()
    {
        StringBuilder sb = new();
        sb.AppendLine("            /* Auto-generated OnEnable: enable all action maps.");
        sb.AppendLine("            This block is managed by the Input Action Sync tool.");
        sb.AppendLine("            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        foreach (var map in inputAsset.actionMaps)
        {
            sb.AppendLine($"        // {map.name} Map");
            sb.AppendLine($"            input.FindActionMap(\"{map.name}\").Enable();");
            if (!Equals(map, inputAsset.actionMaps.Last())) { sb.AppendLine(); }
        }
        return sb.ToString();
    }

    private string GenerateOnDisable()
    {
        StringBuilder sb = new();
        sb.AppendLine("            /* Auto-generated OnDisable: disable all action maps.");
        sb.AppendLine("            This block is managed by the Input Action Sync tool.");
        sb.AppendLine("            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        foreach (var map in inputAsset.actionMaps)
        {
            sb.AppendLine($"        // {map.name} Map");
            sb.AppendLine($"            input.FindActionMap(\"{map.name}\").Disable();");
            if (!Equals(map, inputAsset.actionMaps.Last())) { sb.AppendLine(); }
        }
        return sb.ToString();
    }

    private string GenerateCallbackHandling()
    {
        StringBuilder sb = new();
        sb.AppendLine("        /* Auto-generated Callback Handling: easy way to add and remove methods to the \"icb_\"s.");
        sb.AppendLine("        This block is managed by the Input Action Sync tool.");
        sb.AppendLine("        DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        int i = 0;
        foreach (var mapGroup in actionOptions.GroupBy(a => a.mapName))
        {
            if (i != 0)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine();
            }
            i++;

            sb.AppendLine($"    // {mapGroup.Key} Map");
            int j = 0;
            foreach (var opt in mapGroup)
            {
                if (j != 0)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
                j++;

                string varName = GetVariableName(opt);

                bool k = false;
                if (opt.generatePressed)
                {
                    sb.AppendLine($"        public static void Add{opt.variableName}PressedCallback(Action callback)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            icb_{varName}PressedAction += callback;");
                    sb.AppendLine("        }");
                    sb.AppendLine($"        public static void Remove{opt.variableName}PressedCallback(Action callback)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            icb_{varName}PressedAction -= callback;");
                    sb.AppendLine("        }");

                    k = true;
                }

                if (opt.generateReleased)
                {
                    if (k)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine($"        public static void Add{opt.variableName}ReleasedCallback(Action callback)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            icb_{varName}ReleasedAction += callback;");
                    sb.AppendLine("        }");
                    sb.AppendLine($"        public static void Remove{opt.variableName}ReleasedCallback(Action callback)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            icb_{varName}ReleasedAction -= callback;");
                    sb.AppendLine("        }");

                    k = true;
                }

                if (!k)
                {
                    j--;
                }
            }
        }
        return sb.ToString();
    }

    private string GenerateUpdate()
    {
        StringBuilder sb = new();
        sb.AppendLine("            /* Auto-generated Update input handling.");
        sb.AppendLine("            This block is managed by the Input Action Sync tool.");
        sb.AppendLine("            DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        int i = 0;
        var grouped = actionOptions.GroupBy(a => a.mapName).ToList();
        foreach (var mapGroup in grouped)
        {
            if (i != 0) { sb.AppendLine(); sb.AppendLine(); }
            i++;
            sb.AppendLine($"        // {mapGroup.Key} Map");

            var values = mapGroup.Where(o => o.generateValue).ToList();
            if (values.Count > 0)
            {
                sb.AppendLine("            // Values");
                foreach (var opt in values)
                    sb.AppendLine($"            iv_{GetVariableName(opt)} = iref_{GetVariableName(opt)}InputAction.ReadValue<{GetCSharpType(opt.controlType)}>();");
                if (!Equals(mapGroup, grouped.Last())) sb.AppendLine();
            }

            var bools = mapGroup.Where(o => o.generateBool).ToList();
            if (bools.Count > 0)
            {
                sb.AppendLine("            // States");
                foreach (var opt in bools)
                    sb.AppendLine($"            it_{GetVariableName(opt)}Down = iref_{GetVariableName(opt)}InputAction.IsPressed();");
                if (!Equals(mapGroup, grouped.Last())) sb.AppendLine();
            }

            var events = mapGroup.Where(o => o.generatePressed || o.generateReleased).ToList();
            if (events.Count > 0)
            {
                sb.AppendLine("            // Events");
                foreach (var opt in events)
                {
                    if (opt.generatePressed)
                    {
                        sb.AppendLine($"            if (iref_{GetVariableName(opt)}InputAction.WasPressedThisFrame())");
                        sb.AppendLine("            {");
                        sb.AppendLine($"                icb_{GetVariableName(opt)}PressedAction?.Invoke();");
                        sb.AppendLine("            }");
                        sb.AppendLine();
                    }
                    if (opt.generateReleased)
                    {
                        sb.AppendLine($"            if (iref_{GetVariableName(opt)}InputAction.WasReleasedThisFrame())");
                        sb.AppendLine("            {");
                        sb.AppendLine($"                icb_{GetVariableName(opt)}ReleasedAction?.Invoke();");
                        sb.AppendLine("            }");
                        sb.AppendLine();
                    }
                }
            }
        }
        return sb.ToString();
    }

    private string GenerateSummary()
    {
        StringBuilder sb = new();
        sb.AppendLine("    /* Auto-generated Summary: lists all generated inputs and their parts.");
        sb.AppendLine("    This block is managed by the Input Action Sync tool.");
        sb.AppendLine("    DO NOT EDIT MANUALLY — changes may impact the tool's functionality.*/");
        sb.AppendLine();

        sb.AppendLine("/*");
        foreach (var opt in actionOptions)
        {
            List<string> parts = new();
            if (opt.generateValue) parts.Add("Value");
            if (opt.generateBool) parts.Add("Bool");
            if (opt.generatePressed) parts.Add("Pressed");
            if (opt.generateReleased) parts.Add("Released");

            string partsText = parts.Count > 0 ? string.Join(", ", parts) : "None";
            sb.AppendLine($"{opt.mapName}/{opt.actionName} → Variable: {opt.variableName} | Parts: {partsText}");
        }
        sb.AppendLine("*/");
        return sb.ToString();
    }

    private string ReplaceRegion(string source, string regionName, string replacement)
    {
        string pattern = $@"(^[ \t]*)#region\s*<{regionName}>[\s\S]*?^[ \t]*#endregion\s*//\s*</{regionName}>";
        return Regex.Replace(source, pattern,
            m => $"{m.Groups[1].Value}#region <{regionName}>\n{replacement}\n{m.Groups[1].Value}#endregion // </{regionName}>",
            RegexOptions.Multiline);
    }

    private List<ActionOptions> ParseExistingOptions(string source)
    {
        List<ActionOptions> options = new();
        var summaryMatch = Regex.Match(
            source,
            @"#region <AUTO_GENERATED_INPUT_SUMMARY>(?<content>[\s\S]*?)#endregion\s*//\s*</AUTO_GENERATED_INPUT_SUMMARY>",
            RegexOptions.Multiline
        );
        if (summaryMatch.Success)
        {
            string content = summaryMatch.Groups["content"].Value;
            var lineMatches = Regex.Matches(content,
                @"\s*(?<map>[A-Za-z0-9_]+)/(?<action>[A-Za-z0-9_]+)\s*→\s*Variable:\s*(?<var>[A-Za-z0-9_]+)\s*\|\s*Parts:\s*(?<parts>[A-Za-z0-9_, ]+)");
            foreach (Match m in lineMatches)
            {
                options.Add(new ActionOptions
                {
                    mapName = m.Groups["map"].Value,
                    actionName = m.Groups["action"].Value,
                    variableName = m.Groups["var"].Value,
                    controlType = "float",
                    generateValue = m.Groups["parts"].Value.Contains("Value"),
                    generateBool = m.Groups["parts"].Value.Contains("Bool"),
                    generatePressed = m.Groups["parts"].Value.Contains("Pressed"),
                    generateReleased = m.Groups["parts"].Value.Contains("Released"),
                    alreadyExists = true
                });
            }
        }
        return options;
    }

    private string GetFallbackControlType(InputAction action)
    {
        foreach (var binding in action.bindings)
        {
            if (binding.isComposite)
            {
                if (binding.name == "2DVector") return "Vector2";
                if (binding.name == "1DAxis") return "float";
            }
        }

        return "float"; // default fallback
    }

    private string GetCSharpType(string controlType) => controlType switch
    {
        "Button" => "bool",
        "Axis" => "float",
        "Analog" => "float",
        "Vector2" => "Vector2",
        "Vector3" => "Vector3",
        "Quaternion" => "Quaternion",
        "Digital" => "bool",
        "Dpad" => "Vector2",
        "Stick" => "Vector2",
        "Touch" => "Vector2",
        "Integer" => "int",
        _ => "float"
    };

    private string SafeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Unnamed";
        var sb = new StringBuilder();
        foreach (char c in name)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        return sb.ToString();
    }

    private string GetVariableName(ActionOptions opt)
    {
        string baseName = SafeName(opt.variableName);
        return includeMapNameInVariables ? $"{SafeName(opt.mapName)}_{baseName}" : baseName;
    }

    private class ActionOptions
    {
        public string actionName;
        public string variableName;
        public string mapName;
        public string controlType;
        public bool generateValue;
        public bool generateBool;
        public bool generatePressed;
        public bool generateReleased;
        public bool alreadyExists;
    }
}
