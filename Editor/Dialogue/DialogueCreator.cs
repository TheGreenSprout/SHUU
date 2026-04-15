/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



/*using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DialogueCreator : EditorWindow
{
    private enum Mode { Single, Batch }
    private Mode creationMode = Mode.Single;

    private string singleDialogueInstanceName = "NewDialogueInstance";
    private List<DialogueLineData> singleDialogueLines = new List<DialogueLineData>();

    private List<BatchDialogueData> batchDialogues = new List<BatchDialogueData>();
    private Vector2 batchScrollPos;

    private string saveFolderPath = "";

    private const string SaveFolderKey = "DialogueInstanceCreator_SaveFolder";
    private const string BatchDataKey = "DialogueInstanceCreator_BatchData";

    public static void Open()
    {
        GetWindow<DialogueCreator>("Dialogue Instance Creator");
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey(SaveFolderKey))
            saveFolderPath = EditorPrefs.GetString(SaveFolderKey);

        LoadBatchData();
        if (singleDialogueLines.Count == 0)
            singleDialogueLines.Add(new DialogueLineData());
    }

    private void OnDisable()
    {
        SaveBatchData();
    }

    private void OnGUI()
    {
        GUILayout.Label("Dialogue Instance Creator", EditorStyles.boldLabel);

        creationMode = (Mode)EditorGUILayout.EnumPopup("Creation Mode", creationMode);
        GUILayout.Space(10);

        if (creationMode == Mode.Single)
            DrawSingleModeGUI();
        else
            DrawBatchModeGUI();

        GUILayout.Space(10);
        DrawSaveFolderChooser();
    }

    private void DrawSaveFolderChooser()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Choose Save Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                saveFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                EditorPrefs.SetString(SaveFolderKey, saveFolderPath);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                Debug.LogError("Folder must be inside the project's Assets folder.");
            }
        }
        if (GUILayout.Button("Reset EditorPrefs", GUILayout.Width(150)))
        {
            EditorPrefs.DeleteKey(SaveFolderKey);
            EditorPrefs.DeleteKey(BatchDataKey);
            saveFolderPath = "";
            batchDialogues.Clear();
            Debug.Log("EditorPrefs cleared.");
        }
        EditorGUILayout.LabelField("Save Folder:", string.IsNullOrEmpty(saveFolderPath) ? "<Not Selected>" : saveFolderPath);
        GUILayout.EndHorizontal();
    }

    #region Single Mode

    private void DrawSingleModeGUI()
    {
        singleDialogueInstanceName = EditorGUILayout.TextField("DialogueInstance Name", singleDialogueInstanceName);

        EditorGUILayout.LabelField("Dialogue Lines:");
        for (int i = 0; i < singleDialogueLines.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            var line = singleDialogueLines[i];

            line.portrait = (DialoguePortrait)EditorGUILayout.ObjectField("Portrait", line.portrait, typeof(DialoguePortrait), false);
            line.characterName = EditorGUILayout.TextField("Character Name", line.characterName);
            line.lineText = EditorGUILayout.TextField("Line Text", line.lineText);
            line.dialogueSounds = (DialogueSounds)EditorGUILayout.ObjectField("Dialogue Sounds", line.dialogueSounds, typeof(DialogueSounds), false);
            line.floatStrength = EditorGUILayout.FloatField("Strength", line.floatStrength);
            line.floatSpeed = EditorGUILayout.FloatField("Speed", line.floatSpeed);

            if (GUILayout.Button("Remove Line"))
            {
                singleDialogueLines.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Dialogue Line"))
        {
            singleDialogueLines.Add(new DialogueLineData());
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Create DialogueInstance Asset"))
        {
            CreateSingleDialogueInstance();
        }
    }

    private void CreateSingleDialogueInstance()
    {
        if (string.IsNullOrEmpty(singleDialogueInstanceName))
        {
            Debug.LogError("Please enter a valid name for the DialogueInstance.");
            return;
        }
        if (singleDialogueLines.Count == 0)
        {
            Debug.LogError("Please add at least one dialogue line.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save DialogueInstance",
            singleDialogueInstanceName + ".asset",
            "asset",
            "Choose a location to save the DialogueInstance asset"
        );

        if (string.IsNullOrEmpty(path))
            return;

        DialogueInstance newInstance = ScriptableObject.CreateInstance<DialogueInstance>();
        newInstance.allDialogueLines = new List<DialogueLineInstance>();

        foreach (var lineData in singleDialogueLines)
        {
            var lineInstance = new DialogueLineInstance();
            lineInstance.portrait = lineData.portrait;
            lineInstance.characterName = lineData.characterName;
            lineInstance.line = lineData.lineText;
            lineInstance.textSounds = lineData.dialogueSounds;
            lineInstance.floatValues = new float[2] { lineData.floatStrength, lineData.floatSpeed };
            newInstance.allDialogueLines.Add(lineInstance);
        }

        AssetDatabase.CreateAsset(newInstance, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created DialogueInstance asset at {path}");
    }

    #endregion

    #region Batch Mode

    private void DrawBatchModeGUI()
    {
        if (GUILayout.Button("Add DialogueInstance Entry"))
        {
            batchDialogues.Add(new BatchDialogueData());
            SaveBatchData();
        }

        batchScrollPos = EditorGUILayout.BeginScrollView(batchScrollPos);

        for (int i = 0; i < batchDialogues.Count; i++)
        {
            var batchEntry = batchDialogues[i];
            EditorGUILayout.BeginVertical("box");

            batchEntry.dialogueInstanceName = EditorGUILayout.TextField("DialogueInstance Name", batchEntry.dialogueInstanceName);

            EditorGUILayout.LabelField("Dialogue Lines:");
            for (int j = 0; j < batchEntry.dialogueLines.Count; j++)
            {
                var line = batchEntry.dialogueLines[j];
                EditorGUILayout.BeginVertical("helpbox");

                line.portrait = (DialoguePortrait)EditorGUILayout.ObjectField("Portrait", line.portrait, typeof(DialoguePortrait), false);
                line.characterName = EditorGUILayout.TextField("Character Name", line.characterName);
                line.lineText = EditorGUILayout.TextField("Line Text", line.lineText);
                line.dialogueSounds = (DialogueSounds)EditorGUILayout.ObjectField("Dialogue Sounds", line.dialogueSounds, typeof(DialogueSounds), false);
                line.floatStrength = EditorGUILayout.FloatField("Strength", line.floatStrength);
                line.floatSpeed = EditorGUILayout.FloatField("Speed", line.floatSpeed);

                if (GUILayout.Button("Remove Line", GUILayout.Width(100)))
                {
                    batchEntry.dialogueLines.RemoveAt(j);
                    j--;
                    SaveBatchData();
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Dialogue Line"))
            {
                batchEntry.dialogueLines.Add(new DialogueLineData());
                SaveBatchData();
            }

            if (GUILayout.Button("Remove DialogueInstance Entry"))
            {
                batchDialogues.RemoveAt(i);
                i--;
                SaveBatchData();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("Create All DialogueInstances"))
        {
            CreateBatchDialogueInstances();
        }
    }

    private void CreateBatchDialogueInstances()
    {
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Debug.LogError("Please choose a folder to save the DialogueInstances.");
            return;
        }

        int createdCount = 0;
        foreach (var batchEntry in batchDialogues)
        {
            if (string.IsNullOrEmpty(batchEntry.dialogueInstanceName))
            {
                Debug.LogWarning("Skipping an entry with no name.");
                continue;
            }
            if (batchEntry.dialogueLines.Count == 0)
            {
                Debug.LogWarning($"Skipping '{batchEntry.dialogueInstanceName}' because it has no dialogue lines.");
                continue;
            }

            DialogueInstance newInstance = ScriptableObject.CreateInstance<DialogueInstance>();
            newInstance.allDialogueLines = new List<DialogueLineInstance>();

            foreach (var lineData in batchEntry.dialogueLines)
            {
                var lineInstance = new DialogueLineInstance();
                lineInstance.portrait = lineData.portrait;
                lineInstance.characterName = lineData.characterName;
                lineInstance.line = lineData.lineText;
                lineInstance.textSounds = lineData.dialogueSounds;
                lineInstance.floatValues = new float[2] { lineData.floatStrength, lineData.floatSpeed };
                newInstance.allDialogueLines.Add(lineInstance);
            }

            string assetPath = Path.Combine(saveFolderPath, batchEntry.dialogueInstanceName + ".asset");
            AssetDatabase.CreateAsset(newInstance, assetPath);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created {createdCount} DialogueInstance assets in {saveFolderPath}");

        batchDialogues.Clear();
        SaveBatchData();
    }

    #endregion

    #region Serialization for Batch Data (EditorPrefs)

    [System.Serializable]
    private class DialogueLineData
    {
        public List<DialoguePortrait> portraitList;
        public string characterName = "";
        public string lineText = "";
        public DialogueSounds dialogueSounds;
        public float floatStrength = 1f;
        public float floatSpeed = 1f;
    }

    [System.Serializable]
    private class BatchDialogueData
    {
        public string dialogueInstanceName = "";
        public List<DialogueLineData> dialogueLines = new List<DialogueLineData>();
    }

    private void SaveBatchData()
    {
        try
        {
            string json = JsonUtility.ToJson(new BatchDialogueDataWrapper { batchDialogues = batchDialogues });
            EditorPrefs.SetString(BatchDataKey, json);
        }
        catch { }
    }

    private void LoadBatchData()
    {
        if (EditorPrefs.HasKey(BatchDataKey))
        {
            try
            {
                string json = EditorPrefs.GetString(BatchDataKey);
                var wrapper = JsonUtility.FromJson<BatchDialogueDataWrapper>(json);
                if (wrapper != null && wrapper.batchDialogues != null)
                    batchDialogues = wrapper.batchDialogues;
            }
            catch { }
        }
    }

    [System.Serializable]
    private class BatchDialogueDataWrapper
    {
        public List<BatchDialogueData> batchDialogues;
    }

    #endregion
}*/
