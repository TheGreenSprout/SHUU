using System.Collections.Generic;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;
using UnityEngine;


public class InputSytem_DevConsoleCommands : MonoBehaviour
{
    private static InputBindingMap GetMap(string name)
    {
        InputBindingMap _map = null;

        foreach (InputBindingMap map in InputTracker.allInputBindingMaps.Values)
        {
            if (map.mapName.ToLower() == name.ToLower())
            {
                _map = map;
            }
        }

        return _map;
    }


    private static (string[], Color?) RetrieveMap(string _map, out InputBindingMap info)
    {
        info = GetMap(_map);
        if (info == null) return (new string[] { $"Map doesn't exist or isn't assigned." }, Color.red);

        return (null, null);
    }

    private static (string[], Color?) RetrieveInfo(string _map, string _set, out (InputSet, Composite_InputSet) info)
    {
        InputBindingMap map = GetMap(_map);
        if (map == null)
        {
            info = (null, null);

            return (new string[] { $"Map doesn't exist or isn't assigned." }, Color.red);
        }

        (InputSet, Composite_InputSet) setTouple = SHUU_Input.RetrieveInputSet(map, _set);
        if (setTouple == (null, null))
        {
            info = (null, null);

            return (new string[] { $"Set doesn't exist or isn't present in the selected map." }, Color.red);
        }


        info = (setTouple.Item1, setTouple.Item2);

        return (null, null);
    }



    [DevConsoleCommand("rebind", "Changes an input set's bindings")]
    public static (string[], Color?) RebindSet(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;


        SHUU_Input.RebindInputSet(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return (new string[] { $"{_map} map, {_set} set, bindings rebound." }, null);
    }


    [DevConsoleCommand("addbind", "Adds bindings to an input set")]
    public static (string[], Color?) AddSetBinding(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;


        SHUU_Input.AddInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return (new string[] { $"{_map} map, {_set} set, binding changed." }, null);
    }


    [DevConsoleCommand("removebind", "Removes an input set's specific bind(s)")]
    public static (string[], Color?) RemoveSetBinding(string _map, string _set, params string[] newBinds)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;


        SHUU_Input.RemoveInputBinds(info, SHUU_Input.CreateDynamicInputArray(newBinds));


        return (new string[] { $"{_map} map, {_set} set, binding(s) removed." }, null);
    }


    [DevConsoleCommand("clearbind", "Removes all of an input set's bindings")]
    public static (string[], Color?) ClearSet(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;


        SHUU_Input.ClearInputBinds(info);


        return (new string[] { $"{_map} map, {_set} set, bindings cleared." }, null);
    }

    

    [DevConsoleCommand("allinputmaps", "Displays all input set names of all tracked binding maps")]
    public static (string[], Color?) DisplayAllMaps()
    {
        if (InputTracker.allInputBindingMaps.Values == null || InputTracker.allInputBindingMaps.Values.Count == 0) return (new string[] { "No input maps registered." }, Color.red);

        List<InputBindingMap> mapsList = new List<InputBindingMap>(InputTracker.allInputBindingMaps.Values);
        string[] mapNames = new string[mapsList.Count];

        for (int i = 0; i < mapsList.Count; i++)
        {
            mapNames[i] = mapsList[i].mapName;
        }


        return (mapNames, null);
    }


    [DevConsoleCommand("inputmap", "Displays all input set names on the given map")]
    public static (string[], Color?) DisplayMap(string _map)
    {
        var output = RetrieveMap(_map, out InputBindingMap map);
        if (output != (null, null)) return output;

        int count = map.inputSets_list.Count + map.compositeSets_list.Count;
        if (count == 0)
            return (new string[] { "No input sets registered." }, Color.red);

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

        return (inputList, null);
    }


    [DevConsoleCommand("inputset", "Displays all input bindings on the given input set")]
    public static (string[], Color?) DisplaySet(string _map, string _set)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;

        // ───────────────────────────────────────
        // SINGLE INPUT SET
        // ───────────────────────────────────────
        if (info.Item1 != null)
        {
            InputSet set = info.Item1;
            int count = set.valid_keyBinds.Count + set.valid_mouseBinds.Count;

            if (count == 0)
                return (new string[] { "No bindings registered." }, Color.red);

            string[] lines = new string[count];
            int i = 0;

            foreach (KeyCode key in set.valid_keyBinds)
                lines[i++] = $"Key: {key}";

            foreach (int mouse in set.valid_mouseBinds)
                lines[i++] = $"Mouse: {mouse}";

            return (lines, null);
        }

        // ───────────────────────────────────────
        // COMPOSITE INPUT SET
        // ───────────────────────────────────────
        Composite_InputSet comp = info.Item2;

        if (comp == null || comp.parts == null || comp.parts.Length != 4)
            return (new string[] { "Invalid composite input set." }, Color.red);

        // Names for each composite direction
        string[] partNames = new string[]
        {
            "[X+]", "[X-]", "[Y+]", "[Y-]"
        };

        // Calculate total line count
        int total = 0;
        foreach (var set in comp.parts)
            total += set.valid_keyBinds.Count + set.valid_mouseBinds.Count;

        if (total == 0)
            return (new string[] { "No bindings registered." }, Color.red);

        string[] outputLines = new string[total];
        int index = 0;

        for (int i = 0; i < 4; i++)
        {
            InputSet set = comp.parts[i];
            string label = partNames[i];

            foreach (KeyCode key in set.valid_keyBinds)
                outputLines[index++] = $"{label} Key: {key}";

            foreach (int mouse in set.valid_mouseBinds)
                outputLines[index++] = $"{label} Mouse: {mouse}";
        }

        return (outputLines, null);
    }



    /*[DevConsoleCommand("bindcommand", "Binds a Dev Console command to an input set")]
    public static (string[], Color?) BindCommand(OptionalParameter<string> _map, string _set, string command)
    {
        var output = RetrieveInfo(_map, _set, out (InputSet, Composite_InputSet) info);
        if (output != (null, null)) return output;
    }*/
}
