/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SHUU.Utils.InputSystem;

namespace SHUU._Editor.InputSystem
{
    public class InputBindingsMap_Editor : EditorWindow
    {
        private InputBindingMap map;
        private Vector2 scroll;

        // Key listening system
        private bool isListening = false;
        private System.Action<KeyCode> onKeyDetected = null;

        // Foldout memory
        private Dictionary<string, bool> singleFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> compositeFoldouts = new Dictionary<string, bool>();
        private Dictionary<object, bool> setFoldouts = new Dictionary<object, bool>();

        [MenuItem("Tools/Sprout's Handy Unity Utils/Input System/Custom/Input Bindings Map Editor")]
        public static void Open() => GetWindow<InputBindingsMap_Editor>("Input Bindings Map Editor");

        private void OnGUI()
        {
            HandleKeyListening();

            EditorGUILayout.Space();

            // Pick a map
            map = (InputBindingMap)EditorGUILayout.ObjectField("Input Map", map, typeof(InputBindingMap), false);

            if (map == null)
            {
                EditorGUILayout.HelpBox("Assign an InputBindingMap to edit.", MessageType.Info);
                return;
            }

            // Editable map name
            EditorGUI.BeginChangeCheck();
            map.mapName = EditorGUILayout.TextField("Map Name", map.mapName);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Last Default Set:", GUILayout.Width(100));
            EditorGUILayout.LabelField(map.lastDefaultSetDateTime, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(map, "Edit Map Name");
                SaveMap();
            }

            if (GUILayout.Button("Force Save")) SaveMap();

            EditorGUILayout.Space();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawSingleInputSets();
            EditorGUILayout.Space(20);
            DrawCompositeInputSets();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Default Data")) map.SetDefaultData();
            if (GUILayout.Button("Reset to Default Data")) map.ResetToDefault();
            EditorGUILayout.EndHorizontal();
        }

        // ============================================================================================
        // KEY LISTENING
        // ============================================================================================
        private void HandleKeyListening()
        {
            if (!isListening)
                return;

            Event e = Event.current;
            if (e == null)
                return;

            // Detect only keyboard keys — ignore mouse events
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                KeyCode pressed = e.keyCode;

                Debug.Log("Key detected: " + pressed);

                isListening = false;

                onKeyDetected?.Invoke(pressed);
                onKeyDetected = null;

                GUI.FocusControl(null);
                Repaint();
            }
        }

        private bool KeyIsDuplicate(KeyCode newKey, InputSet exceptSet)
        {
            if (newKey == KeyCode.None)
                return false;

            // ------------------------------
            // Single Input Sets
            // ------------------------------
            foreach (var named in map.inputSets_list)
            {
                var set = named?.set;
                if (set == null || set == exceptSet)
                    continue;

                if (set.valid_keyBinds.Contains(newKey))
                    return true;
            }

            // ------------------------------
            // Composite Input Sets
            // ------------------------------
            foreach (var named in map.compositeSets_list)
            {
                var composite = named?.set;
                if (composite == null)
                    continue;

                foreach (var axis in composite.axes)
                {
                    if (axis == null)
                        continue;

                    // Positive
                    if (axis.positiveSet != null &&
                        axis.positiveSet.set != null &&
                        axis.positiveSet.set != exceptSet &&
                        axis.positiveSet.set.valid_keyBinds.Contains(newKey))
                    {
                        return true;
                    }

                    // Negative
                    if (axis.negativeSet != null &&
                        axis.negativeSet.set != null &&
                        axis.negativeSet.set != exceptSet &&
                        axis.negativeSet.set.valid_keyBinds.Contains(newKey))
                    {
                        return true;
}
                }
            }

            return false;
        }

        // ============================================================================================
        // SAFE REMOVE
        // ============================================================================================
        private void SafeRemove<T>(List<T> list, int index, System.Action onRemove)
        {
            EditorApplication.delayCall += () =>
            {
                Undo.RecordObject(map, "Remove Entry");
                list.RemoveAt(index);
                onRemove?.Invoke();
                SaveMap();
            };
        }

        // ============================================================================================
        // SINGLE INPUT SETS
        // ============================================================================================
        void DrawSingleInputSets()
        {
            EditorGUILayout.LabelField("Single Input Sets", EditorStyles.boldLabel);

            if (GUILayout.Button("Add New Input Set"))
            {
                Undo.RecordObject(map, "Add Input Set");
                map.inputSets_list.Add(new NAMED_InputSet() { name = "NewInputSet" });
                SaveMap();
            }

            EditorGUILayout.Space(5);

            for (int i = 0; i < map.inputSets_list.Count; i++)
            {
                var element = map.inputSets_list[i];
                if (!singleFoldouts.ContainsKey(element.name))
                    singleFoldouts[element.name] = true;

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                singleFoldouts[element.name] =
                    EditorGUILayout.Foldout(singleFoldouts[element.name], element.name, true);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    string oldName = element.name;
                    SafeRemove(map.inputSets_list, i, () =>
                    {
                        singleFoldouts.Remove(oldName);
                    });
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (singleFoldouts[element.name])
                {
                    element.name = EditorGUILayout.TextField("Name", element.name);

                    EditorGUILayout.Space(3);

                    DrawInputSet(element.set);
                }

                EditorGUILayout.EndVertical();
            }
        }

        // ============================================================================================
        // DRAW INPUT SET WITH KEY LISTENING
        // ============================================================================================
        void DrawInputSet(InputSet set)
        {
            if (!setFoldouts.ContainsKey(set))
                setFoldouts[set] = false;

            setFoldouts[set] = EditorGUILayout.Foldout(setFoldouts[set], "Bindings", true);
            if (!setFoldouts[set]) return;

            EditorGUI.indentLevel++;

            // ----------------------------------------------------------------------
            // KEY BINDS
            // ----------------------------------------------------------------------
            EditorGUILayout.LabelField("Key Binds", EditorStyles.boldLabel);
            for (int k = 0; k < set.valid_keyBinds.Count; k++)
            {
                EditorGUILayout.BeginHorizontal();

                // Enum key selector
                set.valid_keyBinds[k] = (KeyCode)EditorGUILayout.EnumPopup(set.valid_keyBinds[k]);

                // 🎧 LISTEN button
                if (GUILayout.Button("🎧", GUILayout.Width(30)))
                {
                    isListening = true;

                    int index = k;
                    InputSet capturedSet = set;

                    onKeyDetected = (KeyCode newKey) =>
                    {
                        Undo.RecordObject(map, "Assign Key Bind");
                        capturedSet.valid_keyBinds[index] = newKey;
                        SaveMap();
                    };
                }

                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    SafeRemove(set.valid_keyBinds, k, null);
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Key Bind"))
            {
                Undo.RecordObject(map, "Add Key Bind");
                set.valid_keyBinds.Add(KeyCode.None);
                SaveMap();
            }

            EditorGUILayout.Space(8);

            // ----------------------------------------------------------------------
            // MOUSE BINDS
            // ----------------------------------------------------------------------
            EditorGUILayout.LabelField("Mouse Buttons", EditorStyles.boldLabel);
            for (int m = 0; m < set.valid_mouseBinds.Count; m++)
            {
                EditorGUILayout.BeginHorizontal();
                set.valid_mouseBinds[m] = EditorGUILayout.IntSlider(set.valid_mouseBinds[m], 0, 6);

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    SafeRemove(set.valid_mouseBinds, m, null);
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Mouse Bind"))
            {
                Undo.RecordObject(map, "Add Mouse Bind");
                set.valid_mouseBinds.Add(0);
                SaveMap();
            }

            EditorGUI.indentLevel--;
        }
        void DrawNamedInputSet(NAMED_InputSet namedSet, string fallbackLabel = null)
        {
            if (namedSet == null || namedSet.set == null)
                return;

            EditorGUILayout.BeginVertical("box");

            // Header
            EditorGUILayout.BeginHorizontal();

            string label = string.IsNullOrEmpty(namedSet.name)
                ? fallbackLabel ?? "Unnamed Set"
                : namedSet.name;

            if (!setFoldouts.ContainsKey(namedSet))
                setFoldouts[namedSet] = true;

            setFoldouts[namedSet] =
                EditorGUILayout.Foldout(setFoldouts[namedSet], label, true);

            EditorGUILayout.EndHorizontal();

            if (setFoldouts[namedSet])
            {
                EditorGUI.indentLevel++;

                // Name field
                EditorGUI.BeginChangeCheck();
                namedSet.name = EditorGUILayout.TextField("Name", namedSet.name);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(map, "Rename Axis Part");
                    SaveMap();
                }

                EditorGUILayout.Space(4);

                // Existing bindings UI
                DrawInputSet(namedSet.set);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        // ============================================================================================
        // COMPOSITE INPUT SETS
        // ============================================================================================
        void DrawCompositeInputSets()
        {
            EditorGUILayout.LabelField("Composite Input Sets", EditorStyles.boldLabel);

            if (GUILayout.Button("Add New Composite Set"))
            {
                Undo.RecordObject(map, "Add Composite Set");
                map.compositeSets_list.Add(new NAMED_Composite_InputSet() { name = "NewCompositeSet" });
                SaveMap();
            }

            EditorGUILayout.Space(5);

            for (int i = 0; i < map.compositeSets_list.Count; i++)
            {
                var element = map.compositeSets_list[i];

                if (!compositeFoldouts.ContainsKey(element.name))
                    compositeFoldouts[element.name] = true;

                EditorGUILayout.BeginVertical("box");

                // Header
                EditorGUILayout.BeginHorizontal();
                compositeFoldouts[element.name] =
                    EditorGUILayout.Foldout(compositeFoldouts[element.name], element.name, true);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    string oldName = element.name;
                    SafeRemove(map.compositeSets_list, i, () =>
                    {
                        compositeFoldouts.Remove(oldName);
                    });
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                // Body
                if (compositeFoldouts[element.name])
                {
                    element.name = EditorGUILayout.TextField("Name", element.name);

                    EditorGUILayout.Space(5);

                    // AXIS COUNT
                    EditorGUI.BeginChangeCheck();
                    int axisCount = EditorGUILayout.IntField("Axis Count", element.set.axisCount);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(map, "Change Axis Count");
                        element.set.axisCount = axisCount;
                        SaveMap();
                    }

                    EditorGUILayout.Space(8);

                    // DRAW AXES
                    for (int a = 0; a < element.set.axes.Count; a++)
                    {
                        var axis = element.set.axes[a];

                        EditorGUILayout.BeginVertical("helpbox");
                        EditorGUILayout.LabelField($"Axis {a+1}", EditorStyles.boldLabel);

                        DrawNamedInputSet(axis.positiveSet, "Positive");
                        EditorGUILayout.Space(4);
                        DrawNamedInputSet(axis.negativeSet, "Negative");

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(6);
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        // ============================================================================================
        // SAVE MAP
        // ============================================================================================
        void SaveMap()
        {
            EditorUtility.SetDirty(map);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
