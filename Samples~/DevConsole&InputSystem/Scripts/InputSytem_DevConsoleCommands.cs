using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;
using UnityEngine;


public class InputSytem_DevConsoleCommands : MonoBehaviour
{
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


    
    [DevConsoleCommand("createset", "Creates a new input set on the given map", "Input System")]
    public static CommandReturn CreateInputSet(string _map, string _set, OptionalParameter<bool> _composite)
    {
        if (!_composite.TryGetValue(out bool composite)) composite = false;


        var _output = RetrieveMap(_map, out InputBindingMap map);
        if (_output != null) return _output;


        if (composite) map.compositeSets_list.Add(new NAMED_Composite_InputSet { name = _set, set = new Composite_InputSet() });
        else map.inputSets_list.Add(new NAMED_InputSet { name = _set, set = new InputSet() });


        return new CommandReturn($"{_map} map, {_set} set created.");
    }

    [DevConsoleCommand("deleteset", "Deletes a new input set on the given map", "Input System")]
    public static CommandReturn DeleteInputSet(string _map, string _set)
    {
        var _output = RetrieveMap(_map, out InputBindingMap map);
        if (_output != null) return _output;

        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        // Try remove single input set
        int singleIndex = map.inputSets_list.FindIndex(
            s => s.name.Equals(_set, System.StringComparison.OrdinalIgnoreCase));

        if (singleIndex != -1)
        {
            map.inputSets_list.RemoveAt(singleIndex);
            return new CommandReturn( $"{_map} map, {_set} single set deleted.");
        }

        // Try remove composite input set
        int compositeIndex = map.compositeSets_list.FindIndex(
            s => s.name.Equals(_set, System.StringComparison.OrdinalIgnoreCase));

        if (compositeIndex != -1)
        {
            map.compositeSets_list.RemoveAt(compositeIndex);
            return new CommandReturn($"{_map} map, {_set} composite set deleted.");
        }


        return new CommandReturn(Color.red, "Set doesn't exist on the selected map.");
    }


    [DevConsoleCommand("rebind", "Changes an input set's bindings", "Input System")]
    public static CommandReturn RebindSet(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        SHUU_Input.RebindInputSet(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return new CommandReturn($"{_map} map, {_set} set, bindings rebound.");
    }

    [DevConsoleCommand("addbind", "Adds bindings to an input set", "Input System")]
    public static CommandReturn AddSetBinding(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        SHUU_Input.AddInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return new CommandReturn($"{_map} map, {_set} set, binding changed.");
    }

    [DevConsoleCommand("removebind", "Removes an input set's specific bind(s)", "Input System")]
    public static CommandReturn RemoveSetBinding(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        SHUU_Input.RemoveInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return new CommandReturn($"{_map} map, {_set} set, binding(s) removed.");
    }

    [DevConsoleCommand("clearbind", "Removes all of an input set's bindings", "Input System")]
    public static CommandReturn ClearSet(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;


        info?.ClearBindings();


        return new CommandReturn($"{_map} map, {_set} set, bindings cleared.");
    }


    [DevConsoleCommand("allinputmaps", "Displays all input set names of all tracked binding maps", "Input System")]
    public static CommandReturn DisplayAllMaps()
    {
        if (SHUU_Input.allInputBindingMaps.Values == null || SHUU_Input.allInputBindingMaps.Values.Count == 0) return new CommandReturn(Color.red, "No input maps registered.");

        List<InputBindingMap> mapsList = new List<InputBindingMap>(SHUU_Input.allInputBindingMaps.Values);
        string[] mapNames = new string[mapsList.Count];

        for (int i = 0; i < mapsList.Count; i++)
        {
            mapNames[i] = mapsList[i].mapName;
        }


        return new CommandReturn(mapNames);
    }

    [DevConsoleCommand("inputmap", "Displays all input set names on the given map", "Input System")]
    public static CommandReturn DisplayMap(string _map)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != null) return output;

        int count = map.inputSets_list.Count + map.compositeSets_list.Count;
        if (count == 0)
            return new CommandReturn(Color.red, "No input sets registered.");

        string[] inputList = new string[count];
        int i = 0;

        foreach (NAMED_InputSet set in map.inputSets_list)
        {
            inputList[i++] = $"Single: {set.name}";
        }

        foreach (NAMED_Composite_InputSet set in map.compositeSets_list)
        {
            inputList[i++] = $"Composite: {set.name}";
        }

        return new CommandReturn(inputList);
    }

    [DevConsoleCommand("inputset", "Displays all input bindings on the given input set", "Input System")]
    public static CommandReturn DisplaySet(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out IInputSet info);
        if (output != null) return output;

        if (info == null)
            return new CommandReturn(Color.red, "Input set not found.");

        // ───────────────────────────────────────
        // SINGLE INPUT SET
        // ───────────────────────────────────────
        if (info is InputSet set)
        {
            int count = set.validSources.Count;

            if (count == 0)
                return new CommandReturn(Color.red, "No bindings registered.");

            string[] lines = new string[count];
            int i = 0;

            foreach (var source in set.validSources)
                lines[i++] = source.String();

            return new CommandReturn(lines);
        }

        // ───────────────────────────────────────
        // COMPOSITE INPUT SET
        // ───────────────────────────────────────
        if (info is Composite_InputSet comp)
        {
            if (comp.axes == null || comp.axes.Count == 0)
                return new CommandReturn(Color.red, "No bindings registered.");

            List<string> lines = new();

            for (int axisIndex = 0; axisIndex < comp.axes.Count; axisIndex++)
            {
                Composite_Axis axis = comp.axes[axisIndex];
                if (axis == null) continue;

                string axisLabel = $"[Axis {axisIndex+1}]";

                // Positive
                foreach (var source in axis.positiveSet.set.validSources)
                    lines.Add($"{axisLabel}[+] {source.String()}");

                // Negative
                foreach (var source in axis.negativeSet.set.validSources)
                    lines.Add($"{axisLabel}[-] {source.String()}");
            }

            if (lines.Count == 0)
                return new CommandReturn(Color.red, "No bindings registered.");

            return new CommandReturn(lines.ToArray());
        }

        // ───────────────────────────────────────
        // UNKNOWN TYPE
        // ───────────────────────────────────────
        return new CommandReturn(Color.red, "Unsupported input set type.");
    }


    [DevConsoleCommand("is_bindcommand", "Binds a Dev Console command to an input set (Input System)", "Input System")]
    public static CommandReturn IS_BindCommand(string _map, string _set, params string[] commandData)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output !=  null) return output;


        BoundCommands.BindCommand((map, _set), commandData);


        return new CommandReturn($"Command bound to {_map} map, {_set} set");
    }

    [DevConsoleCommand("is_unbindcommands", "Unbinds a Dev Console command previously bound to an input set (Input System)", "Input System")]
    public static CommandReturn IS_UnBindCommands(string _map, string _set)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != null) return output;


        BoundCommands.UnBindCommands((map, _set));


        return new CommandReturn($"All commands unbound from {_map} map, {_set} set successfully");
    }
}
