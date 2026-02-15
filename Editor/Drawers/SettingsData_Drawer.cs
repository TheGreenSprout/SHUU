/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using SHUU.Utils.SettingsSytem;
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SettingsData))]
    public class SettingsData_Drawer : Editor
    {
        private SettingsData settingsData;

        SerializedProperty settingsName;
        SerializedProperty fields;
        SerializedProperty defaultFields;

        // Foldout states saved with EditorPrefs
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        void OnEnable()
        {
            settingsData = (SettingsData)target;

            settingsName = serializedObject.FindProperty("settingsName");
            fields = serializedObject.FindProperty("fields");
            defaultFields = serializedObject.FindProperty("defaultFields");

            LoadFoldouts();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(settingsName);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Field")) fields.InsertArrayElementAtIndex(fields.arraySize);
            EditorGUILayout.EndHorizontal();

            DrawFieldList(fields);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Last Default Set:",
                string.IsNullOrEmpty(settingsData.lastDefaultSetDateTime)
                    ? "Never"
                    : settingsData.lastDefaultSetDateTime
            );
            DrawDefaultsButtons();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                settingsData.NotifyChanged(null);
                EditorUtility.SetDirty(settingsData);
            }
        }

        // ---------------- UI ----------------

        void DrawDefaultsButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Current As Defaults"))
            {
                Undo.RecordObject(target, "Save Defaults");
                settingsData.SaveAsDefaults();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Restore Defaults"))
            {
                Undo.RecordObject(target, "Restore Defaults");
                settingsData.RestoreDefaults();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawFieldList(SerializedProperty list)
        {
            // Global fold/unfold buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Expand All"))
                SetAllFoldouts(true);
            if (GUILayout.Button("Collapse All"))
                SetAllFoldouts(false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty field = list.GetArrayElementAtIndex(i);
                SerializedProperty keyProp = field.FindPropertyRelative("key");

                string key = keyProp.stringValue;
                if (string.IsNullOrEmpty(key)) key = $"Field_{i}";

                // Use index + instanceID as stable key for EditorPrefs
                string foldKey = $"{settingsData.GetInstanceID()}_Field_{i}";

                // Get current foldout state from EditorPrefs (default = true)
                bool open = EditorPrefs.GetBool(foldKey, true);

                EditorGUILayout.BeginVertical("box");

                // Header: foldout toggle + editable name + remove button
                EditorGUILayout.BeginHorizontal();
                bool newOpen = EditorGUILayout.Foldout(open, key, true);
                keyProp.stringValue = EditorGUILayout.TextField(keyProp.stringValue);
                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    list.DeleteArrayElementAtIndex(i);
                    EditorPrefs.DeleteKey(foldKey);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Save any change to foldout immediately
                if (newOpen != open)
                    EditorPrefs.SetBool(foldKey, newOpen);

                // Expanded content
                if (newOpen)
                {
                    SerializedProperty type = field.FindPropertyRelative("type");
                    EditorGUILayout.PropertyField(type);

                    DrawValue(field, (SettingType)type.enumValueIndex);
                    DrawMinMaxControls(field, (SettingType)type.enumValueIndex);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
        }

        // ---------------- VALUE DRAWING ----------------

        void DrawValue(SerializedProperty field, SettingType type)
        {
            switch (type)
            {
                case SettingType.Bool:
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("boolValue"));
                    break;

                case SettingType.String:
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("stringValue"));
                    break;

                case SettingType.Int:
                    DrawIntValue(field);
                    break;

                case SettingType.Float:
                    DrawFloatValue(field);
                    break;
            }
        }

        void DrawIntValue(SerializedProperty field)
        {
            var value = field.FindPropertyRelative("intValue");
            var useMin = field.FindPropertyRelative("useMin");
            var useMax = field.FindPropertyRelative("useMax");
            var min = field.FindPropertyRelative("intMin");
            var max = field.FindPropertyRelative("intMax");

            /*if (EditorGUIUtility.editingTextField)
            {
                EditorGUILayout.PropertyField(value);
                return;
            }*/

            try
            {
                if (useMin.boolValue && useMax.boolValue)
                {
                    int minVal = min.intValue;
                    int maxVal = max.intValue;

                    // 🔒 HARD SAFETY CHECK
                    if (minVal >= maxVal)
                    {
                        EditorGUILayout.HelpBox(
                            "Min must be less than Max",
                            MessageType.Warning
                        );
                        return;
                    }

                    value.intValue = Mathf.Clamp(value.intValue, minVal, maxVal);

                    value.intValue = EditorGUILayout.IntSlider(
                        "Value",
                        value.intValue,
                        minVal,
                        maxVal
                    );
                }
                else
                {
                    EditorGUILayout.PropertyField(value);

                    if (useMin.boolValue)
                        value.intValue = Mathf.Max(value.intValue, min.intValue);
                    if (useMax.boolValue)
                        value.intValue = Mathf.Min(value.intValue, max.intValue);
                }
            }
            catch { }
        }

        void DrawFloatValue(SerializedProperty field)
        {
            var value = field.FindPropertyRelative("floatValue");
            var useMin = field.FindPropertyRelative("useMin");
            var useMax = field.FindPropertyRelative("useMax");
            var min = field.FindPropertyRelative("floatMin");
            var max = field.FindPropertyRelative("floatMax");

            /*if (EditorGUIUtility.editingTextField)
            {
                EditorGUILayout.PropertyField(value);
                return;
            }*/

            try
            {
                if (useMin.boolValue && useMax.boolValue)
                {
                    float minVal = min.floatValue;
                    float maxVal = max.floatValue;

                    // Hard guards
                    if (float.IsNaN(minVal) || float.IsNaN(maxVal))
                        return;

                    if (minVal >= maxVal)
                    {
                        EditorGUILayout.HelpBox("Min must be less than Max", MessageType.Warning);
                        return;
                    }

                    // 🔑 CRITICAL LINE
                    value.floatValue = Mathf.Clamp(value.floatValue, minVal, maxVal);

                    value.floatValue = EditorGUILayout.Slider(
                        "Value",
                        value.floatValue,
                        minVal,
                        maxVal
                    );
                }
                else
                {
                    EditorGUILayout.PropertyField(value);

                    if (useMin.boolValue)
                        value.floatValue = Mathf.Max(value.floatValue, min.floatValue);
                    if (useMax.boolValue)
                        value.floatValue = Mathf.Min(value.floatValue, max.floatValue);
                }
            }
            catch { }
        }

        // ---------------- MIN / MAX ----------------

        void DrawMinMaxControls(SerializedProperty field, SettingType type)
        {
            if (type != SettingType.Int && type != SettingType.Float)
                return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(field.FindPropertyRelative("useMin"), new GUIContent("Min"));
            EditorGUILayout.PropertyField(field.FindPropertyRelative("useMax"), new GUIContent("Max"));
            EditorGUILayout.EndHorizontal();

            if (type == SettingType.Int)
            {
                if (field.FindPropertyRelative("useMin").boolValue)
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("intMin"));

                if (field.FindPropertyRelative("useMax").boolValue)
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("intMax"));
            }
            else
            {
                if (field.FindPropertyRelative("useMin").boolValue)
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("floatMin"));

                if (field.FindPropertyRelative("useMax").boolValue)
                    EditorGUILayout.PropertyField(field.FindPropertyRelative("floatMax"));
            }
        }

        // ---------------- FOLDOUTS ----------------

        private string GetFoldoutKey(string fieldKey) => $"{settingsData.name}_{fieldKey}";

        private void SaveFoldouts()
        {
            foreach (var kvp in foldouts)
            {
                EditorPrefs.SetBool(GetFoldoutKey(kvp.Key), kvp.Value);
            }
        }

        private void LoadFoldouts()
        {
            foldouts.Clear();

            if (fields == null)
                return;

            for (int i = 0; i < fields.arraySize; i++)
            {
                var field = fields.GetArrayElementAtIndex(i);
                var key = field.FindPropertyRelative("key").stringValue;
                if (string.IsNullOrEmpty(key)) key = $"Field_{i}";

                bool value = EditorPrefs.GetBool(GetFoldoutKey(key), true);
                foldouts[key] = value;
            }
        }

        private void SetAllFoldouts(bool expand)
        {
            if (fields == null)
                return;

            for (int i = 0; i < fields.arraySize; i++)
            {
                SerializedProperty field = fields.GetArrayElementAtIndex(i);
                string key = field.FindPropertyRelative("key").stringValue;
                if (string.IsNullOrEmpty(key)) key = $"Field_{i}";

                string foldKey = $"{settingsData.GetInstanceID()}_Field_{i}";
                EditorPrefs.SetBool(foldKey, expand);
            }
        }
    }




    public static class SettingsStaticClassGenerator
    {
        // Resources path (no extension!)
        private const string ResourcesPath = "AutoGenerated/GeneratedCode_SettingsSystem";

        // Package filesystem path
        private const string PackagePath =
            "Packages/com.sproutinggames.sprouts.huu/Runtime/Utils/SettingsSystem/Resources/AutoGenerated/GeneratedCode_SettingsSystem.cs";

        private const string StartMarker = "// <auto-generated-settings>";
        private const string EndMarker   = "// </auto-generated-settings>";

        // ---------------- MENU ----------------

        [MenuItem("Tools/Sprout's Handy Unity Utils/Settings/Regenerate Static References")]
        public static void Generate()
        {
            if (!TryResolveTargetFile(out var filePath))
            {
                Debug.LogError(
                    "Dedicated script not found.\n" +
                    "Expected either:\n" +
                    "- Resources/AutoGenerated/GeneratedSettings.cs\n" +
                    "- Package install path"
                );
                return;
            }

            var allSettings = FindAllSettingsData();
            var generatedCode = GenerateAllClasses(allSettings);

            InjectIntoFile(filePath, generatedCode);

            AssetDatabase.Refresh();
            Debug.Log($"Static Settings References regenerated:\n{filePath}");
        }

        // ---------------- PATH RESOLUTION ----------------

        private static bool TryResolveTargetFile(out string filePath)
        {
            // 1️⃣ Try Resources (UnityPackage install)
            var textAsset = Resources.Load<TextAsset>(ResourcesPath);
            if (textAsset != null)
            {
                filePath = AssetDatabase.GetAssetPath(textAsset);
                return true;
            }

            // 2️⃣ Try Package path
            if (File.Exists(PackagePath))
            {
                filePath = PackagePath;
                return true;
            }

            filePath = null;
            return false;
        }

        // ---------------- FIND SETTINGS DATA ----------------

        private static List<SettingsData> FindAllSettingsData()
        {
            var result = new List<SettingsData>();
            var guids = AssetDatabase.FindAssets("t:SettingsData");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<SettingsData>(path);
                if (data != null) result.Add(data);
            }

            return result;
        }

        // ---------------- CODE GENERATION ----------------

        private static string GenerateAllClasses(List<SettingsData> allSettings)
        {
            var sb = new StringBuilder();

            sb.AppendLine("    // GENERATED CODE — DO NOT EDIT MANUALLY");
            sb.AppendLine();

            foreach (var data in allSettings)
            {
                var className = $"{data.settingsName}_Settings";

                sb.AppendLine($"    public static class {className}");
                sb.AppendLine("    {");
                sb.AppendLine($"        private static SettingsData Data => SettingsData.GetSettingsData(\"{data.settingsName}\");");
                sb.AppendLine();

                foreach (var field in data.fields)
                {
                    sb.AppendLine(GenerateProperty(field));
                }

                sb.AppendLine("    }");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GenerateProperty(SettingField field)
        {
            return field.type switch
            {
                SettingType.Bool => $@"
        public static bool {field.key}
        {{
            get => Data.GetBool(""{field.key}"");
            set => Data.SetBool(""{field.key}"", value);
        }}",

                SettingType.Int => $@"
        public static int {field.key}
        {{
            get => Data.GetInt(""{field.key}"");
            set => Data.SetInt(""{field.key}"", value);
        }}",

                SettingType.Float => $@"
        public static float {field.key}
        {{
            get => Data.GetFloat(""{field.key}"");
            set => Data.SetFloat(""{field.key}"", value);
        }}",

                SettingType.String => $@"
        public static string {field.key}
        {{
            get => Data.GetString(""{field.key}"");
            set => Data.SetString(""{field.key}"", value);
        }}",

                _ => ""
            };
        }

        // ---------------- FILE INJECTION ----------------

        private static void InjectIntoFile(string filePath, string generatedContent)
        {
            var text = File.ReadAllText(filePath);

            var start = text.IndexOf(StartMarker);
            var end   = text.IndexOf(EndMarker);

            if (start == -1 || end == -1 || end <= start)
            {
                Debug.LogError("Auto-generated markers not found or invalid.");
                return;
            }

            end += EndMarker.Length;

            var newText =
                text.Substring(0, start) +
                StartMarker + "\n" +
                generatedContent +
                "    " + EndMarker +
                text.Substring(end);

            File.WriteAllText(filePath, newText);
        }
    }
}
#endif
