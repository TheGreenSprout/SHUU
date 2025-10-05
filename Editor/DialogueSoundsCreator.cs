/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class DialogueSoundsCreator : EditorWindow
{
    private enum Mode { Single, Batch }
    private Mode creationMode = Mode.Single;

    // Single
    private List<AudioClip> singleClips = new List<AudioClip>();

    // Batch
    [System.Serializable]
    private class TextSoundsEntry
    {
        public string name;
        public List<AudioClip> clips = new List<AudioClip>();
    }

    private List<TextSoundsEntry> batchEntries = new List<TextSoundsEntry>();
    private Vector2 batchScrollPos;
    private Vector2 singleScrollPos;
    private string saveFolderPath = "";

    // EditorPrefs keys
    private static string ProjectPrefix => Application.dataPath.GetHashCode().ToString();
    private static string Key(string baseKey) => $"{ProjectPrefix}_TextSounds_{baseKey}";
    private const string SaveFolderKey = "SaveFolder";
    private const string BatchDataKey = "BatchEntries";

    public static void Open()
    {
        GetWindow<DialogueSoundsCreator>("Text Sounds Creator");
    }

    private void OnEnable()
    {
        LoadBatchPrefs();
        saveFolderPath = EditorPrefs.GetString(Key(SaveFolderKey), "");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Text Sounds Creator", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear Saved Data", GUILayout.Width(140)))
        {
            if (EditorUtility.DisplayDialog("Clear All Saved Data?", "Are you sure?", "Yes", "Cancel"))
            {
                ClearAllPrefs();
            }
        }
        GUILayout.EndHorizontal();

        creationMode = (Mode)EditorGUILayout.EnumPopup("Creation Mode", creationMode);
        GUILayout.Space(10);

        if (creationMode == Mode.Single)
            DrawSingleGUI();
        else
            DrawBatchGUI();
    }

    private void DrawSingleGUI()
    {
        singleScrollPos = EditorGUILayout.BeginScrollView(singleScrollPos);

        int toRemove = -1;
        for (int i = 0; i < singleClips.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            singleClips[i] = (AudioClip)EditorGUILayout.ObjectField($"Clip {i + 1}", singleClips[i], typeof(AudioClip), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
                toRemove = i;
            EditorGUILayout.EndHorizontal();
        }
        if (toRemove >= 0)
            singleClips.RemoveAt(toRemove);

        if (GUILayout.Button("Add Clip"))
            singleClips.Add(null);

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Create TextSounds Asset"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Save TextSounds", "NewTextSounds", "asset", "Choose save location");
            if (!string.IsNullOrEmpty(path))
            {
                CreateTextSoundsAsset(path, singleClips);
                singleClips.Clear();
            }
        }
    }

    private void DrawBatchGUI()
    {
        if (GUILayout.Button("Add Entry"))
        {
            batchEntries.Add(new TextSoundsEntry());
            SaveBatchPrefs();
        }

        batchScrollPos = EditorGUILayout.BeginScrollView(batchScrollPos);

        for (int i = 0; i < batchEntries.Count; i++)
        {
            var entry = batchEntries[i];
            EditorGUILayout.BeginVertical("box");
            entry.name = EditorGUILayout.TextField("Asset Name", entry.name);

            int toRemove = -1;
            for (int j = 0; j < entry.clips.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();
                entry.clips[j] = (AudioClip)EditorGUILayout.ObjectField($"Clip {j + 1}", entry.clips[j], typeof(AudioClip), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    toRemove = j;
                EditorGUILayout.EndHorizontal();
            }
            if (toRemove >= 0)
                entry.clips.RemoveAt(toRemove);

            if (GUILayout.Button("Add Clip"))
                entry.clips.Add(null);

            if (GUILayout.Button("Remove Entry"))
            {
                batchEntries.RemoveAt(i);
                SaveBatchPrefs();
                i--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Choose Save Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                saveFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                EditorPrefs.SetString(Key(SaveFolderKey), saveFolderPath);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                Debug.LogError("Folder must be inside the Assets folder.");
            }
        }

        EditorGUILayout.LabelField("Save To:", string.IsNullOrEmpty(saveFolderPath) ? "<Not Set>" : saveFolderPath);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create All TextSounds Assets"))
        {
            foreach (var entry in batchEntries)
            {
                if (string.IsNullOrEmpty(entry.name))
                {
                    Debug.LogWarning("Skipping entry with no name.");
                    continue;
                }

                string fullPath = Path.Combine(saveFolderPath, entry.name + ".asset");
                CreateTextSoundsAsset(fullPath, entry.clips);
            }

            batchEntries.Clear();
            saveFolderPath = "";
            EditorPrefs.DeleteKey(Key(BatchDataKey));
            EditorPrefs.DeleteKey(Key(SaveFolderKey));
        }
    }

    private void CreateTextSoundsAsset(string path, List<AudioClip> clips)
    {
        TalkingSounds asset = ScriptableObject.CreateInstance<TalkingSounds>();
        asset.SetClips(clips);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created TextSounds asset at {path}");
    }

    // === Pref Save/Load ===

    [System.Serializable]
    private class BatchWrapper
    {
        public List<BatchEntrySerialized> entries = new List<BatchEntrySerialized>();
    }

    [System.Serializable]
    private class BatchEntrySerialized
    {
        public string name;
        public List<string> clipGUIDs = new List<string>();
    }

    private void SaveBatchPrefs()
    {
        var wrapper = new BatchWrapper();

        foreach (var entry in batchEntries)
        {
            var s = new BatchEntrySerialized { name = entry.name };
            foreach (var clip in entry.clips)
            {
                string guid = clip ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip)) : "";
                s.clipGUIDs.Add(guid);
            }
            wrapper.entries.Add(s);
        }

        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString(Key(BatchDataKey), json);
    }

    private void LoadBatchPrefs()
    {
        if (!EditorPrefs.HasKey(Key(BatchDataKey))) return;

        string json = EditorPrefs.GetString(Key(BatchDataKey));
        var wrapper = JsonUtility.FromJson<BatchWrapper>(json);

        batchEntries.Clear();
        foreach (var s in wrapper.entries)
        {
            var entry = new TextSoundsEntry { name = s.name };
            foreach (string guid in s.clipGUIDs)
            {
                if (!string.IsNullOrEmpty(guid))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    entry.clips.Add(clip);
                }
                else
                {
                    entry.clips.Add(null);
                }
            }
            batchEntries.Add(entry);
        }
    }

    private void ClearAllPrefs()
    {
        EditorPrefs.DeleteKey(Key(SaveFolderKey));
        EditorPrefs.DeleteKey(Key(BatchDataKey));
        batchEntries.Clear();
        saveFolderPath = "";
        singleClips.Clear();
    }
}
