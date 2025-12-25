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

        [MenuItem("Tools/Sprout's Handy Unity Utils/Input System/Classic/Input Bindings Map Editor")]
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
            foreach (var s in map.inputSets_list)
            {
                if (s.set == exceptSet) continue;

                foreach (var key in s.set.valid_keyBinds)
                    if (key == newKey)
                        return true;
            }

            foreach (var s in map.compositeSets_list)
            {
                foreach (var part in s.set.parts)
                {
                    if (part == exceptSet) continue;

                    foreach (var key in part.valid_keyBinds)
                        if (key == newKey)
                            return true;
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

                if (compositeFoldouts[element.name])
                {
                    element.name = EditorGUILayout.TextField("Name", element.name);

                    EditorGUILayout.Space(5);

                    for (int p = 0; p < 4; p++)
                    {
                        string header = p switch
                        {
                            0 => "+x",
                            1 => "-x",
                            2 => "+y",
                            3 => "-y",
                            _ => "?"
                        };

                        EditorGUILayout.LabelField($"Axis {header}", EditorStyles.boldLabel);
                        DrawInputSet(element.set.parts[p]);
                        EditorGUILayout.Space();
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
