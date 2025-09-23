/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class DialoguePortraitCreator : EditorWindow
{
    private enum Mode { Single, Batch }
    private Mode creationMode = Mode.Single;

    // Single
    private Sprite singleIdleSprite;
    private Sprite singleTalkingSprite;
    private float singleIdleSize = 1f;
    private float singleTalkSize = 1f;
    private Vector2 singleIdleOffset = Vector2.zero;
    private Vector2 singleTalkOffset = Vector2.zero;

    // Batch
    [System.Serializable]
    private class PortraitEntry
    {
        public string name;
        public Sprite idle;
        public Sprite talking;
        public float idleSize = 1f;
        public float talkSize = 1f;
        public Vector2 idleOffset = Vector2.zero;
        public Vector2 talkOffset = Vector2.zero;
    }

    private List<PortraitEntry> batchPortraits = new List<PortraitEntry>();
    private string saveFolderPath = "";
    private Vector2 batchScrollPos;

    // EditorPrefs keys
    private const string SaveFolderKey = "PortraitGenerator_SaveFolder";
    private const string SingleIdleKey = "PortraitGenerator_Single_Idle";
    private const string SingleTalkingKey = "PortraitGenerator_Single_Talking";
    private const string SingleIdleSizeKey = "PortraitGenerator_Single_IdleSize";
    private const string SingleTalkSizeKey = "PortraitGenerator_Single_TalkSize";
    private const string SingleIdleOffsetXKey = "PortraitGenerator_Single_IdleOffsetX";
    private const string SingleIdleOffsetYKey = "PortraitGenerator_Single_IdleOffsetY";
    private const string SingleTalkOffsetXKey = "PortraitGenerator_Single_TalkOffsetX";
    private const string SingleTalkOffsetYKey = "PortraitGenerator_Single_TalkOffsetY";
    private const string BatchDataKey = "PortraitGenerator_Batch_List";

    private static string ProjectKeyPrefix => Application.dataPath.GetHashCode().ToString();
    private static string Key(string baseKey) => $"{ProjectKeyPrefix}_{baseKey}";

    public static void Open()
    {
        GetWindow<DialoguePortraitCreator>("Dialogue Portrait Creator");
    }

    private void OnEnable()
    {
        LoadSinglePortraitPrefs();
        LoadBatchPortraitPrefs();

        if (EditorPrefs.HasKey(Key(SaveFolderKey)))
            saveFolderPath = EditorPrefs.GetString(Key(SaveFolderKey));
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Dialogue Portrait Creator", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear Saved Data (EditorPrefs)"))
        {
            if (EditorUtility.DisplayDialog("Clear Saved Data",
                "Are you sure you want to clear all saved Dialogue Portrait data?",
                "Yes", "Cancel"))
            {
                DeleteData();
                Debug.Log("Dialogue Portrait EditorPrefs data cleared.");
            }
        }
        GUILayout.EndHorizontal();

        creationMode = (Mode)EditorGUILayout.EnumPopup("Creation Mode", creationMode);
        GUILayout.Space(10);

        if (creationMode == Mode.Single)
        {
            DrawSinglePortraitGUI();
        }
        else
        {
            DrawBatchPortraitGUI();
        }
    }

    private void DrawSinglePortraitGUI()
    {
        var newIdle = (Sprite)EditorGUILayout.ObjectField("Idle Sprite", singleIdleSprite, typeof(Sprite), false);
        var newTalking = (Sprite)EditorGUILayout.ObjectField("Talking Sprite", singleTalkingSprite, typeof(Sprite), false);
        var newIdleSize = EditorGUILayout.FloatField("Idle Size", singleIdleSize);
        var newTalkSize = EditorGUILayout.FloatField("Talk Size", singleTalkSize);
        var newIdleOffset = EditorGUILayout.Vector2Field("Idle Offset", singleIdleOffset);
        var newTalkOffset = EditorGUILayout.Vector2Field("Talk Offset", singleTalkOffset);

        if (newIdle != singleIdleSprite || newTalking != singleTalkingSprite ||
            newIdleSize != singleIdleSize || newTalkSize != singleTalkSize ||
            newIdleOffset != singleIdleOffset || newTalkOffset != singleTalkOffset)
        {
            singleIdleSprite = newIdle;
            singleTalkingSprite = newTalking;
            singleIdleSize = newIdleSize;
            singleTalkSize = newTalkSize;
            singleIdleOffset = newIdleOffset;
            singleTalkOffset = newTalkOffset;
            SaveSinglePortraitPrefs();
        }

        if (GUILayout.Button("Create Dialogue Portrait"))
        {
            CreateSinglePortrait();
        }
    }

    private void DrawBatchPortraitGUI()
    {
        if (GUILayout.Button("Add Portrait Entry"))
        {
            batchPortraits.Add(new PortraitEntry());
            SaveBatchPortraitPrefs();
        }

        batchScrollPos = EditorGUILayout.BeginScrollView(batchScrollPos);

        for (int i = 0; i < batchPortraits.Count; i++)
        {
            var entry = batchPortraits[i];
            EditorGUILayout.BeginVertical("box");

            entry.name = EditorGUILayout.TextField("Portrait Name", entry.name);
            entry.idle = (Sprite)EditorGUILayout.ObjectField("Idle Sprite", entry.idle, typeof(Sprite), false);
            entry.talking = (Sprite)EditorGUILayout.ObjectField("Talking Sprite", entry.talking, typeof(Sprite), false);
            entry.idleSize = EditorGUILayout.FloatField("Idle Size", entry.idleSize);
            entry.talkSize = EditorGUILayout.FloatField("Talk Size", entry.talkSize);
            entry.idleOffset = EditorGUILayout.Vector2Field("Idle Offset", entry.idleOffset);
            entry.talkOffset = EditorGUILayout.Vector2Field("Talk Offset", entry.talkOffset);

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                batchPortraits.RemoveAt(i);
                SaveBatchPortraitPrefs();
                i--;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Choose Save Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                saveFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                EditorPrefs.SetString(Key(SaveFolderKey), saveFolderPath);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                Debug.LogError("Folder must be inside the project's Assets folder.");
            }
        }

        EditorGUILayout.LabelField("Save Folder:", string.IsNullOrEmpty(saveFolderPath) ? "<Not Selected>" : saveFolderPath);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Create All Portraits"))
        {
            CreateBatchPortraits();
        }

        GUILayout.Space(10);
    }

    private void CreateSinglePortrait()
    {
        if (singleIdleSprite == null || singleTalkingSprite == null)
        {
            Debug.LogError("Both sprites must be assigned.");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Dialogue Portrait",
            "NewDialoguePortrait",
            "asset",
            "Choose a location and name for the DialoguePortrait asset"
        );

        if (string.IsNullOrEmpty(path)) return;

        DialoguePortrait newPortrait = ScriptableObject.CreateInstance<DialoguePortrait>();
        newPortrait.idlePortrait = singleIdleSprite;
        newPortrait.talkingPortrait = singleTalkingSprite;
        newPortrait.idleSize = singleIdleSize;
        newPortrait.talkSize = singleTalkSize;
        newPortrait.idleOffset = singleIdleOffset;
        newPortrait.talkOffset = singleTalkOffset;

        AssetDatabase.CreateAsset(newPortrait, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created DialoguePortrait at {path}");

        EditorPrefs.DeleteKey(Key(SingleIdleKey));
        EditorPrefs.DeleteKey(Key(SingleTalkingKey));
        EditorPrefs.DeleteKey(Key(SingleIdleSizeKey));
        EditorPrefs.DeleteKey(Key(SingleTalkSizeKey));
        EditorPrefs.DeleteKey(Key(SingleIdleOffsetXKey));
        EditorPrefs.DeleteKey(Key(SingleIdleOffsetYKey));
        EditorPrefs.DeleteKey(Key(SingleTalkOffsetXKey));
        EditorPrefs.DeleteKey(Key(SingleTalkOffsetYKey));

        singleIdleSprite = null;
        singleTalkingSprite = null;
        singleIdleSize = 1f;
        singleTalkSize = 1f;
        singleIdleOffset = Vector2.zero;
        singleTalkOffset = Vector2.zero;
    }

    private void CreateBatchPortraits()
    {
        if (string.IsNullOrEmpty(saveFolderPath))
        {
            Debug.LogError("You must choose a folder to save the portraits.");
            return;
        }

        foreach (var entry in batchPortraits)
        {
            if (string.IsNullOrEmpty(entry.name) || entry.idle == null || entry.talking == null)
            {
                Debug.LogWarning($"Skipping incomplete entry: {entry.name}");
                continue;
            }

            DialoguePortrait newPortrait = ScriptableObject.CreateInstance<DialoguePortrait>();
            newPortrait.idlePortrait = entry.idle;
            newPortrait.talkingPortrait = entry.talking;
            newPortrait.idleSize = entry.idleSize;
            newPortrait.talkSize = entry.talkSize;
            newPortrait.idleOffset = entry.idleOffset;
            newPortrait.talkOffset = entry.talkOffset;

            string assetPath = Path.Combine(saveFolderPath, entry.name + ".asset");
            AssetDatabase.CreateAsset(newPortrait, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created {batchPortraits.Count} DialoguePortrait assets in {saveFolderPath}");

        batchPortraits.Clear();
        saveFolderPath = "";

        EditorPrefs.DeleteKey(Key(BatchDataKey));
        EditorPrefs.DeleteKey(Key(SaveFolderKey));
    }

    private void SaveSinglePortraitPrefs()
    {
        string idleGUID = singleIdleSprite ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(singleIdleSprite)) : "";
        string talkingGUID = singleTalkingSprite ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(singleTalkingSprite)) : "";

        EditorPrefs.SetString(Key(SingleIdleKey), idleGUID);
        EditorPrefs.SetString(Key(SingleTalkingKey), talkingGUID);
        EditorPrefs.SetFloat(Key(SingleIdleSizeKey), singleIdleSize);
        EditorPrefs.SetFloat(Key(SingleTalkSizeKey), singleTalkSize);
        EditorPrefs.SetFloat(Key(SingleIdleOffsetXKey), singleIdleOffset.x);
        EditorPrefs.SetFloat(Key(SingleIdleOffsetYKey), singleIdleOffset.y);
        EditorPrefs.SetFloat(Key(SingleTalkOffsetXKey), singleTalkOffset.x);
        EditorPrefs.SetFloat(Key(SingleTalkOffsetYKey), singleTalkOffset.y);
    }

    private void LoadSinglePortraitPrefs()
    {
        string idleGUID = EditorPrefs.GetString(Key(SingleIdleKey), "");
        string talkingGUID = EditorPrefs.GetString(Key(SingleTalkingKey), "");

        if (!string.IsNullOrEmpty(idleGUID))
            singleIdleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(idleGUID));

        if (!string.IsNullOrEmpty(talkingGUID))
            singleTalkingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(talkingGUID));

        if (EditorPrefs.HasKey(Key(SingleIdleSizeKey)))
            singleIdleSize = EditorPrefs.GetFloat(Key(SingleIdleSizeKey));

        if (EditorPrefs.HasKey(Key(SingleTalkSizeKey)))
            singleTalkSize = EditorPrefs.GetFloat(Key(SingleTalkSizeKey));

        if (EditorPrefs.HasKey(Key(SingleIdleOffsetXKey)) && EditorPrefs.HasKey(Key(SingleIdleOffsetYKey)))
        {
            singleIdleOffset = new Vector2(
                EditorPrefs.GetFloat(Key(SingleIdleOffsetXKey)),
                EditorPrefs.GetFloat(Key(SingleIdleOffsetYKey))
            );
        }

        if (EditorPrefs.HasKey(Key(SingleTalkOffsetXKey)) && EditorPrefs.HasKey(Key(SingleTalkOffsetYKey)))
        {
            singleTalkOffset = new Vector2(
                EditorPrefs.GetFloat(Key(SingleTalkOffsetXKey)),
                EditorPrefs.GetFloat(Key(SingleTalkOffsetYKey))
            );
        }
    }

    [System.Serializable]
    private class PortraitEntrySerializable
    {
        public string name;
        public string idleGUID;
        public string talkingGUID;
        public float idleSize;
        public float talkSize;
        public Vector2 idleOffset;
        public Vector2 talkOffset;
    }

    [System.Serializable]
    private class PortraitEntryListWrapper
    {
        public List<PortraitEntrySerializable> entries = new List<PortraitEntrySerializable>();
    }

    private void SaveBatchPortraitPrefs()
    {
        var wrapper = new PortraitEntryListWrapper();

        foreach (var entry in batchPortraits)
        {
            var serialized = new PortraitEntrySerializable
            {
                name = entry.name,
                idleGUID = entry.idle ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.idle)) : "",
                talkingGUID = entry.talking ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.talking)) : "",
                idleSize = entry.idleSize,
                talkSize = entry.talkSize,
                idleOffset = entry.idleOffset,
                talkOffset = entry.talkOffset
            };
            wrapper.entries.Add(serialized);
        }

        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString(Key(BatchDataKey), json);
    }

    private void LoadBatchPortraitPrefs()
    {
        batchPortraits.Clear();

        if (!EditorPrefs.HasKey(Key(BatchDataKey))) return;

        string json = EditorPrefs.GetString(Key(BatchDataKey), "");
        if (string.IsNullOrEmpty(json)) return;

        var wrapper = JsonUtility.FromJson<PortraitEntryListWrapper>(json);
        if (wrapper == null) return;

        foreach (var serialized in wrapper.entries)
        {
            Sprite idle = !string.IsNullOrEmpty(serialized.idleGUID)
                ? AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(serialized.idleGUID))
                : null;

            Sprite talking = !string.IsNullOrEmpty(serialized.talkingGUID)
                ? AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(serialized.talkingGUID))
                : null;

            batchPortraits.Add(new PortraitEntry
            {
                name = serialized.name,
                idle = idle,
                talking = talking,
                idleSize = serialized.idleSize,
                talkSize = serialized.talkSize,
                idleOffset = serialized.idleOffset,
                talkOffset = serialized.talkOffset
            });
        }
    }

    private void DeleteData()
    {
        EditorPrefs.DeleteKey(Key(SingleIdleKey));
        EditorPrefs.DeleteKey(Key(SingleTalkingKey));
        EditorPrefs.DeleteKey(Key(SingleIdleSizeKey));
        EditorPrefs.DeleteKey(Key(SingleTalkSizeKey));
        EditorPrefs.DeleteKey(Key(SingleIdleOffsetXKey));
        EditorPrefs.DeleteKey(Key(SingleIdleOffsetYKey));
        EditorPrefs.DeleteKey(Key(SingleTalkOffsetXKey));
        EditorPrefs.DeleteKey(Key(SingleTalkOffsetYKey));
        EditorPrefs.DeleteKey(Key(BatchDataKey));
        EditorPrefs.DeleteKey(Key(SaveFolderKey));

        singleIdleSprite = null;
        singleTalkingSprite = null;
        singleIdleSize = 1f;
        singleTalkSize = 1f;
        singleIdleOffset = Vector2.zero;
        singleTalkOffset = Vector2.zero;
        batchPortraits.Clear();
        saveFolderPath = "";
    }
}
