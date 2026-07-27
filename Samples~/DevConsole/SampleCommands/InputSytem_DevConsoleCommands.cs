using UnityEngine;
using UnityEngine.InputSystem;

using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;
using System.Linq;

public class InputSytem_DevConsoleCommands : MonoBehaviour
{
    #region Helpers
    private static CommandReturn RetrieveAction(string _actionPath, out InputAction action)
    {
        action = SHUU_Input.GetAction(_actionPath);
        if (action == null) return new CommandReturn(Color.red, $"Action '{_actionPath}' doesn't exist.");

        return null;
    }
    #endregion




    #region Commands

    #region Rebinding
    [DevConsoleCommand("rebind", "Overrides a binding on an action to a new control path", "Input System")]
    public static CommandReturn Rebind(string _actionPath, int _bindingIndex, string _newControlPath)
    {
        var output = RetrieveAction(_actionPath, out InputAction action);
        if (output != null) return output;

        if (_bindingIndex < 0 || _bindingIndex >= action.bindings.Count) return new CommandReturn(Color.red, $"Binding index {_bindingIndex} out of range (action has {action.bindings.Count} binding(s)).");


        action.ApplyBindingOverride(_bindingIndex, _newControlPath);

        return new CommandReturn(Color.green, $"'{_actionPath}' binding {_bindingIndex} overridden to '{_newControlPath}'.");
    }


    [DevConsoleCommand("resetbind", "Resets an action's bindings to default, or all actions if none specified", "Input System")]
    public static CommandReturn ResetBind(OptionalParameter<string> _actionPath, OptionalParameter<int> _bindingIndex)
    {
        if (_actionPath.TryGetValue(out string actionPath))
        {
            var output = RetrieveAction(actionPath, out InputAction action);
            if (output != null) return output;

            if (_bindingIndex.TryGetValue(out int bindingIndex))
            {
                if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return new CommandReturn(Color.red, $"Binding index {bindingIndex} out of range (action has {action.bindings.Count} binding(s)).");

                action.RemoveBindingOverride(bindingIndex);

                return new CommandReturn(Color.green, $"'{actionPath}' binding {bindingIndex} reset to default.");
            }


            action.RemoveAllBindingOverrides();

            return new CommandReturn(Color.green, $"All bindings on '{actionPath}' reset to default.");
        }


        SHUU_Input.ResetToDefaults();

        return new CommandReturn(Color.green, "All bindings reset to default.");
    }
    #endregion



    #region Display
    [DevConsoleCommand("allinputmaps", "Displays all action map names", "Input System")]
    public static CommandReturn AllInputMaps()
    {
        var names = SHUU_Input.GetAllMapNames().ToArray();

        if (names.Length == 0) return new CommandReturn(Color.red, "No action maps found.");

        return new CommandReturn(names);
    }


    [DevConsoleCommand("inputmap", "Displays all action names on the given action map", "Input System")]
    public static CommandReturn InputMap(string _map)
    {
        InputActionMap map = SHUU_Input.GetMap(_map);
        if (map == null) return new CommandReturn(Color.red, $"Map '{_map}' doesn't exist or isn't assigned.");

        if (map.actions.Count == 0) return new CommandReturn(Color.red, "No actions registered on this map.");


        string[] actionNames = new string[map.actions.Count];

        for (int i = 0; i < map.actions.Count; i++)
            actionNames[i] = map.actions[i].name;

        return new CommandReturn(actionNames);
    }

    [DevConsoleCommand("inputaction", "Displays all bindings on the given action", "Input System")]
    public static CommandReturn InputAction(string _actionPath)
    {
        var output = RetrieveAction(_actionPath, out InputAction action);
        if (output != null) return output;

        if (action.bindings.Count == 0) return new CommandReturn(Color.red, "No bindings registered on this action.");


        string[] lines = new string[action.bindings.Count];

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            string path = string.IsNullOrEmpty(binding.overridePath) ? binding.path : binding.overridePath;
            string overrideTag = string.IsNullOrEmpty(binding.overridePath) ? "" : " (overridden)";

            lines[i] = $"[{i}] {path}{overrideTag}";
        }

        return new CommandReturn(lines);
    }


    [DevConsoleCommand("mapcontextstack", "Displays the current map context stack", "Input System")]
    public static CommandReturn MapContextStack()
    {
        string stack = SHUU_Input.PrintContextStack();
        return new CommandReturn($"Current context stack: {stack}");
    }
    #endregion



    #region Command Binding
    // To the input directly
    [DevConsoleCommand("bindcommanddirect", "Binds a command to a raw control path (<Device>/input)", "Input System")]
    public static CommandReturn BindCommandDirect(string _controlPath, params string[] commandData)
    {
        InputControl control = InputSystem.FindControl(_controlPath);
        if (control == null) return new CommandReturn(Color.red, $"Invalid control path '{_controlPath}'");

        BoundCommands.BindDirectCommand(_controlPath, commandData);

        return new CommandReturn(Color.green, $"Command bound to '{_controlPath}' successfully.");
    }


    [DevConsoleCommand("unbindcommandsdirect", "Unbinds commands bound to a control path (all or one)", "Input System")]
    public static CommandReturn UnBindCommandsDirect(string _controlPath, params string[] commandData)
    {
        bool specific = commandData != null && commandData.Length > 0;

        if (!BoundCommands.UnBindDirectCommands(_controlPath, specific ? commandData : null)) return new CommandReturn(Color.red, specific
                                                                                                ? $"Command '{string.Join(" ", commandData)}' not found on '{_controlPath}'."
                                                                                                : $"No commands bound to '{_controlPath}'.");

        return new CommandReturn(Color.green, specific
            ? $"Command '{string.Join(" ", commandData)}' unbound from '{_controlPath}'."
            : $"All commands unbound from '{_controlPath}'.");
    }


    // To InputAction
    [DevConsoleCommand("bindcommand", "Binds a command to an input action", "Input System")]
    public static CommandReturn BindCommand(string _actionPath, params string[] commandData)
    {
        var output = RetrieveAction(_actionPath, out _);
        if (output != null) return output;

        BoundCommands.BindCommand(_actionPath, commandData);

        return new CommandReturn(Color.green, $"Command bound to '{_actionPath}' successfully.");
    }

    [DevConsoleCommand("unbindcommands", "Unbinds commands bound to an input action (all or one)", "Input System")]
    public static CommandReturn UnBindCommands(string _actionPath, params string[] commandData)
    {
        bool specific = commandData != null && commandData.Length > 0;

        if (!BoundCommands.UnBindCommands(_actionPath, specific ? commandData : null)) return new CommandReturn(Color.red, specific
                                                                                        ? $"Command '{string.Join(" ", commandData)}' not found on '{_actionPath}'."
                                                                                        : $"No commands bound to '{_actionPath}'.");

        return new CommandReturn(Color.green, specific
            ? $"Command '{string.Join(" ", commandData)}' unbound from '{_actionPath}'."
            : $"All commands unbound from '{_actionPath}'.");
    }
    #endregion

    #endregion
}
