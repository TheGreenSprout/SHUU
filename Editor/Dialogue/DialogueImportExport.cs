/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



/*using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueImportExport : EditorWindow
{
    [SerializeField] private List<DialogueInstance> dialoguesToExport = new List<DialogueInstance>();
    private List<string> txtPathsToImport = new List<string>();
    private string presetFilePath = "";

    private const string PortraitPrefix = "# Portrait: ";
    private const string NamePrefix = "# Name: ";
    private const string SoundsPrefix = "# Sounds: ";
    private const string StrengthPrefix = "# Strength: ";
    private const string SpeedPrefix = "# Speed: ";
    private const string PresetPrefix = "# preset ";
    private const string RepeatToken = "# repeat";

    private static readonly string[] FieldPrefixes = { PortraitPrefix, NamePrefix, SoundsPrefix, StrengthPrefix, SpeedPrefix };

    private Dictionary<string, DialogueLineInstance> presets = new Dictionary<string, DialogueLineInstance>(StringComparer.OrdinalIgnoreCase);

    [MenuItem("Tools/Dialogue/Dialogue Import & Export")]
    public static void OpenWindow()
    {
        GetWindow<DialogueImportExport>("Dialogue Import & Export");
    }

    private void OnGUI()
    {
        // EXPORT SECTION
        GUILayout.Label("Export DialogueInstances to TXT", EditorStyles.boldLabel);
        DrawExportList();
        if (GUILayout.Button("Export Selected DialogueInstances")) ExportDialogues();

        EditorGUILayout.Space(20);

        // IMPORT SECTION
        GUILayout.Label("Import TXT Files into DialogueInstances", EditorStyles.boldLabel);
        DrawImportList();

        GUILayout.Space(10);
        GUILayout.Label("Optional Preset TXT File:", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.IsNullOrEmpty(presetFilePath) ? "None" : Path.GetFileName(presetFilePath));
        if (GUILayout.Button("Select Preset File"))
        {
            string file = EditorUtility.OpenFilePanel("Select Preset TXT File", "", "txt");
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
                presetFilePath = file;
        }
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
            presetFilePath = "";
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Import Selected TXT Files"))
            ImportDialogues();
    }

    private void DrawExportList()
    {
        int removeIndex = -1;
        for (int i = 0; i < dialoguesToExport.Count; i++)
        {
            GUILayout.BeginHorizontal();
            dialoguesToExport[i] = (DialogueInstance)EditorGUILayout.ObjectField(dialoguesToExport[i], typeof(DialogueInstance), false);
            if (GUILayout.Button("X", GUILayout.Width(20))) removeIndex = i;
            GUILayout.EndHorizontal();
        }
        if (removeIndex >= 0) dialoguesToExport.RemoveAt(removeIndex);
        if (GUILayout.Button("Add DialogueInstance")) dialoguesToExport.Add(null);
    }

    private void DrawImportList()
    {
        for (int i = 0; i < txtPathsToImport.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Path.GetFileName(txtPathsToImport[i]));
            if (GUILayout.Button("X", GUILayout.Width(20))) { txtPathsToImport.RemoveAt(i); i--; }
            GUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add TXT File from Computer"))
        {
            string file = EditorUtility.OpenFilePanel("Select Dialogue TXT File", "", "txt");
            if (!string.IsNullOrEmpty(file) && File.Exists(file) && !txtPathsToImport.Contains(file))
                txtPathsToImport.Add(file);
        }
    }

    // ------------------- EXPORT -------------------
    private void ExportDialogues()
    {
        if (dialoguesToExport.Count == 0)
        {
            Debug.LogWarning("No DialogueInstances selected for export.");
            return;
        }
        string folderPath = EditorUtility.OpenFolderPanel("Select Export Folder", "", "");
        if (string.IsNullOrEmpty(folderPath)) return;

        if (!string.IsNullOrEmpty(presetFilePath) && File.Exists(presetFilePath))
            LoadPresets(presetFilePath);

        foreach (var instance in dialoguesToExport)
        {
            if (instance == null) continue;
            StringBuilder sb = new StringBuilder();
            DialogueLineInstance prev = null;

            foreach (var line in instance.allDialogueLines)
            {
                // Check preset match first
                string presetMatch = FindMatchingPreset(line);
                if (!string.IsNullOrEmpty(presetMatch))
                {
                    sb.AppendLine(PresetPrefix + presetMatch);
                    DialogueLineInstance preset = presets[presetMatch];
                    if (line.floatValues[0] != preset.floatValues[0])
                        sb.AppendLine(StrengthPrefix + line.floatValues[0]);
                    if (line.floatValues[1] != preset.floatValues[1])
                        sb.AppendLine(SpeedPrefix + line.floatValues[1]);
                }
                else
                {
                    // Repeat logic with grouping
                    string[] currentValues =
                    {
                        line.portrait ? line.portrait.name : "",
                        line.characterName ?? "",
                        line.textSounds ? line.textSounds.name : "",
                        line.floatValues[0].ToString(),
                        line.floatValues[1].ToString()
                    };
                    string[] prevValues =
                    {
                        prev?.portrait ? prev.portrait.name : "",
                        prev?.characterName ?? "",
                        prev?.textSounds ? prev.textSounds.name : "",
                        prev != null ? prev.floatValues[0].ToString() : "",
                        prev != null ? prev.floatValues[1].ToString() : ""
                    };

                    int fieldIndex = 0;
                    while (fieldIndex < FieldPrefixes.Length)
                    {
                        if (prev != null && currentValues[fieldIndex] == prevValues[fieldIndex])
                        {
                            int count = 1;
                            int checkIndex = fieldIndex + 1;
                            while (checkIndex < FieldPrefixes.Length && currentValues[checkIndex] == prevValues[checkIndex])
                            {
                                count++;
                                checkIndex++;
                            }
                            if (count > 1)
                            {
                                sb.AppendLine(FieldPrefixes[fieldIndex] + RepeatToken + " " + count);
                                fieldIndex += count;
                                continue;
                            }
                            else sb.AppendLine(FieldPrefixes[fieldIndex] + RepeatToken);
                        }
                        else sb.AppendLine(FieldPrefixes[fieldIndex] + currentValues[fieldIndex]);
                        fieldIndex++;
                    }
                }
                sb.AppendLine(line.line ?? "");
                sb.AppendLine();
                prev = line;
            }
            string filePath = Path.Combine(folderPath, instance.name + ".txt");
            File.WriteAllText(filePath, sb.ToString());
        }
        Debug.Log("Export completed.");
    }

    // ------------------- IMPORT -------------------
    private void ImportDialogues()
    {
        if (txtPathsToImport.Count == 0)
        {
            Debug.LogWarning("No TXT files selected for import.");
            return;
        }
        if (!string.IsNullOrEmpty(presetFilePath) && File.Exists(presetFilePath))
            LoadPresets(presetFilePath);

        foreach (var filePath in txtPathsToImport)
        {
            if (!File.Exists(filePath)) continue;
            string[] lines = File.ReadAllLines(filePath);
            DialogueInstance instance = GetOrCreateDialogueInstance(Path.GetFileNameWithoutExtension(filePath));
            if (instance == null) continue;

            instance.allDialogueLines.Clear();
            DialogueLineInstance prev = null;
            DialogueLineInstance current = null;
            int currentFieldIndex = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    if (current != null)
                    {
                        instance.allDialogueLines.Add(current);
                        prev = current;
                        current = null;
                        currentFieldIndex = 0;
                    }
                    continue;
                }

                // Preset line
                if (line.StartsWith(PresetPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    string presetName = line.Substring(PresetPrefix.Length).Trim();
                    if (presets.TryGetValue(presetName, out DialogueLineInstance preset))
                    {
                        // Clone preset fully
                        current = CloneLine(preset);
                    }
                    else
                    {
                        Debug.LogWarning($"Preset '{presetName}' not found.");
                        if (current == null) current = new DialogueLineInstance();
                    }
                    continue;
                }

                // Field prefix match
                int matchedFieldIndex = -1;
                for (int fi = 0; fi < FieldPrefixes.Length; fi++)
                    if (line.StartsWith(FieldPrefixes[fi], StringComparison.OrdinalIgnoreCase))
                        { matchedFieldIndex = fi; break; }

                if (matchedFieldIndex >= 0)
                {
                    if (current == null) current = new DialogueLineInstance();
                    string val = line.Substring(FieldPrefixes[matchedFieldIndex].Length).Trim();
                    if (val.StartsWith(RepeatToken, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prev == null) Debug.LogError($"Error: '{val}' on first line for {FieldPrefixes[matchedFieldIndex]}");
                        else
                        {
                            int count = 1;
                            string[] parts = val.Split(' ');
                            if (parts.Length > 1 && int.TryParse(parts[1], out int parsed) && parsed > 1) count = parsed;
                            for (int offset = 0; offset < count && matchedFieldIndex + offset < FieldPrefixes.Length; offset++)
                                CopyField(current, prev, matchedFieldIndex + offset);
                        }
                        continue;
                    }
                    else SetField(current, matchedFieldIndex, val);
                    currentFieldIndex = matchedFieldIndex + 1;
                    continue;
                }

                // Bare repeat
                if (line.StartsWith(RepeatToken, StringComparison.OrdinalIgnoreCase))
                {
                    if (current == null) current = new DialogueLineInstance();
                    if (prev == null) { Debug.LogError($"Error: '{line}' used on first line."); continue; }
                    int count = 1;
                    string[] parts = line.Split(' ');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int parsed2) && parsed2 > 1) count = parsed2;
                    for (int offset = 0; offset < count && currentFieldIndex + offset < FieldPrefixes.Length; offset++)
                        CopyField(current, prev, currentFieldIndex + offset);
                    currentFieldIndex += count;
                    continue;
                }

                // Dialogue text
                if (current == null) current = new DialogueLineInstance();
                if (string.IsNullOrEmpty(current.line)) current.line = line;
                else current.line += "\n" + line;
            }
            if (current != null) instance.allDialogueLines.Add(current);
            EditorUtility.SetDirty(instance);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Import completed.");
    }

    // ------------------- PRESETS -------------------
    private void LoadPresets(string path)
    {
        presets.Clear();
        if (!File.Exists(path)) return;
        string[] lines = File.ReadAllLines(path);

        DialogueLineInstance current = null;
        string currentName = null;

        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrEmpty(line))
            {
                if (current != null && !string.IsNullOrEmpty(currentName))
                    presets[currentName] = current;
                current = null; currentName = null;
                continue;
            }
            if (line.StartsWith(PresetPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (current != null && !string.IsNullOrEmpty(currentName))
                    presets[currentName] = current;
                current = new DialogueLineInstance();
                currentName = line.Substring(PresetPrefix.Length).Trim();
                continue;
            }
            for (int fi = 0; fi < FieldPrefixes.Length; fi++)
                if (line.StartsWith(FieldPrefixes[fi], StringComparison.OrdinalIgnoreCase))
                    { SetField(current, fi, line.Substring(FieldPrefixes[fi].Length).Trim()); break; }
        }
        if (current != null && !string.IsNullOrEmpty(currentName))
            presets[currentName] = current;
        Debug.Log($"Loaded {presets.Count} presets.");
    }

    private string FindMatchingPreset(DialogueLineInstance line)
    {
        foreach (var kvp in presets)
        {
            var p = kvp.Value;
            if (!string.Equals(line.characterName, p.characterName, StringComparison.OrdinalIgnoreCase)) continue;
            if (line.portrait != p.portrait) continue;
            if (line.textSounds != p.textSounds) continue;
            if (!Mathf.Approximately(line.floatValues[0], p.floatValues[0])) continue;
            if (!Mathf.Approximately(line.floatValues[1], p.floatValues[1])) continue;
            return kvp.Key;
        }
        return null;
    }

    // ------------------- HELPERS -------------------
    private DialogueLineInstance CloneLine(DialogueLineInstance src)
    {
        return new DialogueLineInstance
        {
            portrait = src.portrait,
            characterName = src.characterName,
            textSounds = src.textSounds,
            floatValues = new float[] { src.floatValues[0], src.floatValues[1] },
            line = ""
        };
    }

    private void CopyField(DialogueLineInstance target, DialogueLineInstance source, int fieldIndex)
    {
        switch (fieldIndex)
        {
            case 0: target.portrait = source.portrait; break;
            case 1: target.characterName = source.characterName; break;
            case 2: target.textSounds = source.textSounds; break;
            case 3: target.floatValues[0] = source.floatValues[0]; break;
            case 4: target.floatValues[1] = source.floatValues[1]; break;
        }
    }

    private void SetField(DialogueLineInstance target, int fieldIndex, string val)
    {
        switch (fieldIndex)
        {
            case 0: target.portrait = LoadAssetByName<DialoguePortrait>(val); break;
            case 1: target.characterName = val; break;
            case 2: target.textSounds = LoadAssetByName<DialogueSounds>(val); break;
            case 3: float.TryParse(val, out target.floatValues[0]); break;
            case 4: float.TryParse(val, out target.floatValues[1]); break;
        }
    }

    private DialogueInstance GetOrCreateDialogueInstance(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:DialogueInstance");
        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<DialogueInstance>(assetPath);
        }
        DialogueInstance newInstance = ScriptableObject.CreateInstance<DialogueInstance>();
        string savePath = EditorUtility.SaveFilePanelInProject("Save DialogueInstance", name, "asset", "Select save location for new DialogueInstance");
        if (!string.IsNullOrEmpty(savePath))
        {
            AssetDatabase.CreateAsset(newInstance, savePath);
            AssetDatabase.SaveAssets();
            return newInstance;
        }
        return null;
    }

    private static T LoadAssetByName<T>(string assetName) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetName)) return null;
        string[] guids = AssetDatabase.FindAssets(assetName + " t:" + typeof(T).Name);
        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return null;
    }
}*/
