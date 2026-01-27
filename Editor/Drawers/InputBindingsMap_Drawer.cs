/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SHUU.Utils.InputSystem;
using SHUU.Utils.Helpers;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(InputBindingMap))]
    public class InputBindingMap_Drawer : Editor
    {
        private InputBindingMap map;

        private bool isListening;
        private InputSet listeningSet;
        private int listeningIndex;

        private enum BindingKind { Key, Mouse, Axis }

        // =============================================================
        // UNITY
        // =============================================================
        private void OnEnable()
        {
            map = (InputBindingMap)target;
            map.ForceBuildDictionaries();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            DrawSingleSets();
            DrawCompositeSets();
            DrawDefaults();

            HandleListening();

            if (GUI.changed)
                EditorUtility.SetDirty(map);

            serializedObject.ApplyModifiedProperties();
        }

        // =============================================================
        // HEADER
        // =============================================================
        private new void DrawHeader()
        {
            EditorGUILayout.LabelField("Input Binding Map", EditorStyles.boldLabel);
            map.mapName = EditorGUILayout.TextField("Map Name", map.mapName);
            map.enabled = EditorGUILayout.Toggle("Enabled", map.enabled);
            EditorGUILayout.Space(10);
        }

        // =============================================================
        // SINGLE SETS
        // =============================================================
        private void DrawSingleSets()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Single Input Sets", EditorStyles.boldLabel);
            if (GUILayout.Button("+ Add Input Set")) map.inputSets_list.Add(new NAMED_InputSet { name = "New Set" });
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < map.inputSets_list.Count; i++)
            {
                var named = map.inputSets_list[i];
                string foldKey = GetFoldKey("Single", named.name, i);

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                bool open = DrawFoldout(foldKey, named.name);

                named.name = EditorGUILayout.TextField(named.name);
                named.set.enabled = EditorGUILayout.ToggleLeft("Enabled", named.set.enabled, GUILayout.Width(80));

                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    map.inputSets_list.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (open)
                    DrawInputSet(named.set, allowAxis: true);

                EditorGUILayout.EndVertical();
            }
        }

        // =============================================================
        // COMPOSITE SETS
        // =============================================================
        private void DrawCompositeSets()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Composite Input Sets", EditorStyles.boldLabel);
            if (GUILayout.Button("+ Add Composite Set")) map.compositeSets_list.Add(new NAMED_Composite_InputSet { name = "New Composite" });
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < map.compositeSets_list.Count; i++)
            {
                var named = map.compositeSets_list[i];
                string foldKey = GetFoldKey("Composite", named.name, i);

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                bool open = DrawFoldout(foldKey, named.name);

                named.name = EditorGUILayout.TextField(named.name);
                named.set.enabled = EditorGUILayout.ToggleLeft("Enabled", named.set.enabled, GUILayout.Width(80));

                if (GUILayout.Button("X", GUILayout.Width(22)))
                {
                    map.compositeSets_list.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }
                EditorGUILayout.EndHorizontal();

                if (open)
                {
                    named.set.axisCount = EditorGUILayout.IntSlider("Axis Count", named.set.axisCount, 0, 4);

                    for (int a = 0; a < named.set.axes.Count; a++)
                    {
                        var axis = named.set.axes[a];

                        string axisKey = GetFoldKey("Axis", named.name, a);

                        EditorGUILayout.BeginVertical("helpbox");
                        bool axisOpen = DrawFoldout(axisKey, $"Axis {a}");

                        if (axisOpen)
                        {
                            DrawInputSet(axis.positiveSet.set, false, "Positive");
                            DrawInputSet(axis.negativeSet.set, false, "Negative");
                        }

                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        // =============================================================
        // INPUT SET
        // =============================================================
        private void DrawInputSet(InputSet set, bool allowAxis, string label = null)
        {
            if (!string.IsNullOrEmpty(label))
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

            for (int i = 0; i < set.validSources.Count; i++)
                DrawBindingRow(set, i, allowAxis);

            if (GUILayout.Button("+ Add Binding"))
                set.validSources.Add(new KeySource(KeyCode.None));
        }

        private void DrawBindingRow(InputSet set, int index, bool allowAxis)
        {
            var src = set.validSources[index];

            EditorGUILayout.BeginHorizontal("box");

            BindingKind kind = GetKind(src);
            kind = (BindingKind)EditorGUILayout.EnumPopup(kind, GUILayout.Width(70));

            if (src.GetType() != GetSourceType(kind))
            {
                set.validSources[index] = CreateSource(kind, allowAxis);
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (src is KeySource key)
                key.key = (KeyCode)EditorGUILayout.EnumPopup(key.key);
            else if (src is MouseSource mouse)
                mouse.mouse = EditorGUILayout.Popup(mouse.mouse,
                    new[] { "Left", "Right", "Middle", "Button 3", "Button 4", "Button 5" });
            else if (src is AxisSource axis && allowAxis)
            {
                axis.axisName = EditorGUILayout.TextField(axis.axisName);
                axis.threshold = EditorGUILayout.Slider(axis.threshold, 0f, 1f);
                axis.raw = EditorGUILayout.ToggleLeft("Raw", axis.raw, GUILayout.Width(50));
            }

            if (GUILayout.Button("🎧", GUILayout.Width(30)))
            {
                isListening = true;
                listeningSet = set;
                listeningIndex = index;
            }

            if (GUILayout.Button("X", GUILayout.Width(22)))
                set.validSources.RemoveAt(index);

            EditorGUILayout.EndHorizontal();
        }

        // =============================================================
        // DEFAULTS
        // =============================================================
        private void DrawDefaults()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Defaults", EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "Last Default Set:",
                string.IsNullOrEmpty(map.lastDefaultSetDateTime)
                    ? "Never"
                    : map.lastDefaultSetDateTime
            );

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Default Data"))
            {
                Undo.RecordObject(map, "Set Default Input Data");
                map.SetDefaultData();
                EditorUtility.SetDirty(map);
            }

            if (GUILayout.Button("Reset To Default"))
            {
                Undo.RecordObject(map, "Reset Input Map To Default");
                map.ResetToDefault();
                EditorUtility.SetDirty(map);
            }

            EditorGUILayout.EndHorizontal();
        }

        // =============================================================
        // LISTEN
        // =============================================================
        private void HandleListening()
        {
            if (!isListening) return;

            EditorGUILayout.HelpBox("Listening for key or mouse input… (Esc to cancel)", MessageType.Info);

            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                listeningSet.validSources[listeningIndex] = new KeySource(e.keyCode);
                StopListening();
                e.Use();
            }
            else if (e.type == EventType.MouseDown)
            {
                listeningSet.validSources[listeningIndex] = new MouseSource(e.button);
                StopListening();
                e.Use();
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                StopListening();
                e.Use();
            }
        }

        private void StopListening()
        {
            isListening = false;
            listeningSet = null;
            listeningIndex = -1;
        }

        // =============================================================
        // FOLDOUTS (EditorPrefs)
        // =============================================================
        private bool DrawFoldout(string key, string label)
        {
            bool open = EditorPrefs.GetBool(key, false); // folded by default
            bool newOpen = EditorGUILayout.Foldout(open, label, true);

            if (newOpen != open)
                EditorPrefs.SetBool(key, newOpen);

            return newOpen;
        }

        private string GetFoldKey(string type, string name, int index)
        {
            return $"{map.GetInstanceID()}_{type}_{index}_{name}";
        }

        // =============================================================
        // HELPERS
        // =============================================================
        private BindingKind GetKind(InputSource src)
        {
            if (src is KeySource) return BindingKind.Key;
            if (src is MouseSource) return BindingKind.Mouse;
            if (src is AxisSource) return BindingKind.Axis;
            return BindingKind.Key;
        }

        private System.Type GetSourceType(BindingKind kind)
        {
            return kind switch
            {
                BindingKind.Key => typeof(KeySource),
                BindingKind.Mouse => typeof(MouseSource),
                BindingKind.Axis => typeof(AxisSource),
                _ => typeof(KeySource)
            };
        }

        private InputSource CreateSource(BindingKind kind, bool allowAxis)
        {
            return kind switch
            {
                BindingKind.Key => new KeySource(KeyCode.None),
                BindingKind.Mouse => new MouseSource(0),
                BindingKind.Axis when allowAxis => new AxisSource(InputParser.AxisNames.Horizontal),
                _ => new KeySource(KeyCode.None)
            };
        }
    }
}
#endif