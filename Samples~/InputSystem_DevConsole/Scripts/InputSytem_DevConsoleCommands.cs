using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;

public class InputSytem_DevConsoleCommands : MonoBehaviour
{
    #region Helpers
    private static CommandReturn RetrieveMap(string _map, out InputBindingMap info)
    {
        info = SHUU_Input.RetrieveBindingMap(_map);
        if (info == null) return new CommandReturn(Color.red, $"Map doesn't exist or isn't assigned.");

        return null;
    }


    private static CommandReturn RetrieveInfo(string _map, string _set, out IInputSet info)
    {
        info = null;


        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != null) return output;

        IInputSet set = SHUU_Input.RetrieveInputSet(map, _set);
        if (set == null) return new CommandReturn(Color.red, $"Set doesn't exist or isn't present in the selected map.");


        info = set;
        return null;
    }
    #endregion



    
    #region Commands
    
    #region Creation & Deletion
    [DevConsoleCommand("createset", "Creates a new input set on the given map", "Input System")]
    public static CommandReturn CreateSet(string _map, string _set, OptionalParameter<bool> _composite)
    {
        if (!_composite.TryGetValue(out bool composite)) composite = false;


        var _output = RetrieveMap(_map, out InputBindingMap map);
        if (_output != null) return _output;


        if (composite) map.compositeSets_list.Add(new NAMED_Composite_InputSet { name = _set, set = new Composite_InputSet() });
        else map.inputSets_list.Add(new NAMED_InputSet { name = _set, set = new InputSet() });


        return new CommandReturn(Color.green, $"{_map} map, {_set} set created.");
    }
    

    [DevConsoleCommand("deleteset", "Deletes a new input set on the given map", "Input System")]
    public static CommandReturn DeleteSet(string _map, string _set)
    {
        var _output = RetrieveMap(_map, out InputBindingMap map);
        if (_output != null) return _output;

        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        int singleIndex = map.inputSets_list.FindIndex(s => s.name.Equals(_set, System.StringComparison.OrdinalIgnoreCase));

        if (singleIndex != -1)
        {
            map.inputSets_list.RemoveAt(singleIndex);
            return new CommandReturn(Color.green, $"{_map} map, {_set} single set deleted.");
        }

        int compositeIndex = map.compositeSets_list.FindIndex(s => s.name.Equals(_set, System.StringComparison.OrdinalIgnoreCase));

        if (compositeIndex != -1)
        {
            map.compositeSets_list.RemoveAt(compositeIndex);
            return new CommandReturn(Color.green, $"{_map} map, {_set} composite set deleted.");
        }


        return new CommandReturn(Color.red, "Set doesn't exist on the selected map.");
    }
    #endregion



    #region Altering Bindings
    [DevConsoleCommand("rebind", "Changes an input set's bindings", "Input System")]
    public static CommandReturn Rebind(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        SHUU_Input.RebindInputSet(info, SHUU_Input.CreateDynamicInputArray(newBinds));

        return new CommandReturn(Color.green, $"{_map} map, {_set} set, bindings rebound.");
    }


    [DevConsoleCommand("addbind", "Adds bindings to an input set", "Input System")]
    public static CommandReturn AddBind(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        SHUU_Input.AddInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));

        return new CommandReturn(Color.green, $"{_map} map, {_set} set, binding changed.");
    }


    [DevConsoleCommand("removebind", "Removes an input set's specific bind(s)", "Input System")]
    public static CommandReturn RemoveBind(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        SHUU_Input.RemoveInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));

        return new CommandReturn(Color.green, $"{_map} map, {_set} set, binding(s) removed.");
    }

    [DevConsoleCommand("clearbind", "Removes all of an input set's bindings", "Input System")]
    public static CommandReturn ClearBind(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        info?.ClearBindings();

        return new CommandReturn(Color.green, $"{_map} map, {_set} set, bindings cleared.");
    }
    #endregion



    #region Display
    [DevConsoleCommand("allinputmaps", "Displays all input set names of all tracked binding maps", "Input System")]
    public static CommandReturn AllInputMaps()
    {
        if (SHUU_Input.allInputBindingMaps.Values == null || SHUU_Input.allInputBindingMaps.Values.Count == 0) return new CommandReturn(Color.red, "No input maps registered.");

        List<InputBindingMap> mapsList = new List<InputBindingMap>(SHUU_Input.allInputBindingMaps.Values);
        string[] mapNames = new string[mapsList.Count];

        for (int i = 0; i < mapsList.Count; i++)
            mapNames[i] = mapsList[i].mapName;

        return new CommandReturn(Color.green, mapNames);
    }


    [DevConsoleCommand("inputmap", "Displays all input set names on the given map", "Input System")]
    public static CommandReturn InputMap(string _map)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != null) return output;

        int count = map.inputSets_list.Count + map.compositeSets_list.Count;
        if (count == 0) return new CommandReturn(Color.red, "No input sets registered.");

        string[] inputList = new string[count];
        int i = 0;

        foreach (NAMED_InputSet set in map.inputSets_list)
            inputList[i++] = $"Single: {set.name}";

        foreach (NAMED_Composite_InputSet set in map.compositeSets_list)
            inputList[i++] = $"Composite: {set.name}";

        return new CommandReturn(Color.green, inputList);
    }


    [DevConsoleCommand("inputset", "Displays all input bindings on the given input set", "Input System")]
    public static CommandReturn InputSet(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        if (info == null) return new CommandReturn(Color.red, "Input set not found.");

        if (info is InputSet set)
        {
            int count = set.validSources.Count;

            if (count == 0) return new CommandReturn(Color.red, "No bindings registered.");

            string[] lines = new string[count];
            int i = 0;

            foreach (var source in set.validSources)
                lines[i++] = source.String();

            return new CommandReturn(Color.green, lines);
        }

        if (info is Composite_InputSet comp)
        {
            if (comp.axes == null || comp.axes.Count == 0) return new CommandReturn(Color.red, "No bindings registered.");

            List<string> lines = new();

            for (int axisIndex = 0; axisIndex < comp.axes.Count; axisIndex++)
            {
                Composite_Axis axis = comp.axes[axisIndex];
                if (axis == null) continue;

                string axisLabel = $"[Axis {axisIndex+1}]";

                foreach (var source in axis.positiveSet.set.validSources)
                    lines.Add($"{axisLabel}[+] {source.String()}");

                foreach (var source in axis.negativeSet.set.validSources)
                    lines.Add($"{axisLabel}[-] {source.String()}");
            }

            if (lines.Count == 0) return new CommandReturn(Color.red, "No bindings registered.");

            return new CommandReturn(Color.green, lines.ToArray());
        }

        return new CommandReturn(Color.red, "Unsupported input set type.");
    }
    #endregion



    #region Command Binding
    [DevConsoleCommand("is_bindcommand", "Binds a Dev Console command to an input set (Input System)", "Input System")]
    public static CommandReturn IS_BindCommand(string _map, string _set, params string[] commandData)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output !=  null) return output;

        BoundCommands.BindCommand((map, _set), commandData);

        return new CommandReturn(Color.green, $"Command bound to {_map} map, {_set} set");
    }


    [DevConsoleCommand("is_unbindcommands", "Unbinds a Dev Console command previously bound to an input set (Input System)", "Input System")]
    public static CommandReturn IS_UnBindCommands(string _map, string _set)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != null) return output;

        BoundCommands.UnBindCommands((map, _set));

        return new CommandReturn(Color.green, $"All commands unbound from {_map} map, {_set} set successfully");
    }
    #endregion
    
    #endregion
}
