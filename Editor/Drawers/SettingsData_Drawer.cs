/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System.Collections.Generic;
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

            if (useMin.boolValue && useMax.boolValue)
            {
                value.intValue = EditorGUILayout.IntSlider(
                    "Value",
                    value.intValue,
                    min.intValue,
                    max.intValue
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

        void DrawFloatValue(SerializedProperty field)
        {
            var value = field.FindPropertyRelative("floatValue");
            var useMin = field.FindPropertyRelative("useMin");
            var useMax = field.FindPropertyRelative("useMax");
            var min = field.FindPropertyRelative("floatMin");
            var max = field.FindPropertyRelative("floatMax");

            if (useMin.boolValue && useMax.boolValue)
            {
                value.floatValue = EditorGUILayout.Slider(
                    "Value",
                    value.floatValue,
                    min.floatValue,
                    max.floatValue
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
}
#endif
