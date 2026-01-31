/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SHUU.Utils.InputSystem;
using SHUU.Utils.Helpers;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(InputBindingMap))]
    public class InputBindingMap_Drawer : Editor
    {
        private InputBindingMap map;

        private bool isListening;
        private InputSet listeningSet;
        private int listeningIndex;

        // 🔹 ADDED
        private int listeningControlId;

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

                if (open) DrawInputSet(named.set, allowAxis: true);

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
                EditorGUILayout.BeginVertical();
                InputParser.AxisNames currentEnum = InputParser.ParseAxisEnum(axis.axisName) ?? InputParser.AxisNames.Horizontal;
                InputParser.AxisNames newEnum = (InputParser.AxisNames)EditorGUILayout.EnumPopup(currentEnum);

                axis.axisName = new AxisSource(newEnum, axis.threshold, axis.raw).axisName;

                axis.threshold = EditorGUILayout.Slider(axis.threshold, 0f, 1f);
                axis.raw = EditorGUILayout.ToggleLeft("Raw", axis.raw, GUILayout.Width(50));
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("🎧", GUILayout.Width(30)))
            {
                isListening = true;
                listeningSet = set;
                listeningIndex = index;

                // 🔹 ADDED
                listeningControlId = GUIUtility.GetControlID(FocusType.Passive);
                GUIUtility.hotControl = listeningControlId;
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

            EditorGUILayout.HelpBox(
                "Listening for key or mouse input… (Esc to cancel)",
                MessageType.Info
            );

            Event e = Event.current;
            if (e == null) return;

            // 🔹 ADDED: force mouse capture
            if (e.isMouse)
                GUIUtility.hotControl = listeningControlId;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    StopListening();
                    e.Use();
                    return;
                }

                if (e.keyCode != KeyCode.None)
                {
                    listeningSet.validSources[listeningIndex] =
                        new KeySource(e.keyCode);

                    StopListening();
                    e.Use();
                    return;
                }
            }

            if (e.type == EventType.MouseDown)
            {
                listeningSet.validSources[listeningIndex] =
                    new MouseSource(e.button);

                StopListening();
                e.Use();
            }
        }

        private void StopListening()
        {
            isListening = false;
            listeningSet = null;
            listeningIndex = -1;

            // 🔹 ADDED
            GUIUtility.hotControl = 0;
        }

        // =============================================================
        // FOLDOUTS (EditorPrefs)
        // =============================================================
        private bool DrawFoldout(string key, string label)
        {
            bool open = EditorPrefs.GetBool(key, false);
            bool newOpen = EditorGUILayout.Foldout(open, label, true);

            if (newOpen != open) EditorPrefs.SetBool(key, newOpen);

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




    public static class InputStaticClassGenerator
    {
        // Resources path (no extension)
        private const string ResourcesPath =
            "AutoGenerated/GeneratedCode_InputSystem";

        // Package filesystem path
        private const string PackagePath =
            "Packages/com.sproutinggames.sprouts.huu/Runtime/Utils/InputSystem/Resources/AutoGenerated/GeneratedCode_InputSystem.cs";

        private const string StartMarker = "// <auto-generated-input>";
        private const string EndMarker   = "// </auto-generated-input>";

        // ---------------- MENU ----------------

        [MenuItem("Tools/Sprout's Handy Unity Utils/Input/Regenerate Static Input References")]
        public static void Generate()
        {
            if (!TryResolveTargetFile(out var filePath))
            {
                Debug.LogError(
                    "Dedicated Input script not found.\n" +
                    "Expected either:\n" +
                    "- Resources/AutoGenerated/GeneratedCode_InputSystem.cs\n" +
                    "- Package install path"
                );
                return;
            }

            var maps = FindAllInputBindingMaps();
            var generatedCode = GenerateAllClasses(maps);

            InjectIntoFile(filePath, generatedCode);

            AssetDatabase.Refresh();
            Debug.Log($"Static Input References regenerated:\n{filePath}");
        }

        // ---------------- PATH RESOLUTION ----------------

        private static bool TryResolveTargetFile(out string filePath)
        {
            // 1️⃣ Resources (UnityPackage install)
            var textAsset = Resources.Load<TextAsset>(ResourcesPath);
            if (textAsset != null)
            {
                filePath = AssetDatabase.GetAssetPath(textAsset);
                return true;
            }

            // 2️⃣ Package path
            if (File.Exists(PackagePath))
            {
                filePath = PackagePath;
                return true;
            }

            filePath = null;
            return false;
        }

        // ---------------- FIND MAPS ----------------

        private static List<InputBindingMap> FindAllInputBindingMaps()
        {
            var result = new List<InputBindingMap>();
            var guids = AssetDatabase.FindAssets("t:InputBindingMap");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var map = AssetDatabase.LoadAssetAtPath<InputBindingMap>(path);
                if (map != null)
                    result.Add(map);
            }

            return result;
        }

        // ---------------- CODE GENERATION ----------------

        private static string GenerateAllClasses(List<InputBindingMap> maps)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// GENERATED CODE — DO NOT EDIT MANUALLY");
            sb.AppendLine();

            foreach (var map in maps)
            {
                var className = Sanitize(map.mapName) + "_Input";

                sb.AppendLine($"public static class {className}");
                sb.AppendLine("{");
                sb.AppendLine($"    private static InputBindingMap Map => SHUU_Input.RetrieveBindingMap(\"{map.mapName}\");");
                sb.AppendLine();

                // ---------- SINGLE SETS ----------
                foreach (var set in map.inputSets_list)
                {
                    if (string.IsNullOrEmpty(set.name)) continue;

                    var setName = Sanitize(set.name);

                    sb.AppendLine($"    public static class {setName}");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        private const string KEY = \"{set.name}\";");
                    sb.AppendLine();
                    sb.AppendLine("        public static bool Down(bool all = false) => SHUU_Input.GetInputDown(Map, KEY, all);");
                    sb.AppendLine("        public static bool Up(bool all = false) => SHUU_Input.GetInputUp(Map, KEY, all);");
                    sb.AppendLine("        public static bool Held(bool all = false) => SHUU_Input.GetInput(Map, KEY, all);");
                    sb.AppendLine("        public static float Value(bool all = false) => SHUU_Input.GetInputValue(Map, KEY, all).TryGetFloat(out var v) ? v : 0f;");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                // ---------- COMPOSITE SETS ----------
                foreach (var set in map.compositeSets_list)
                {
                    if (string.IsNullOrEmpty(set.name)) continue;

                    var setName = Sanitize(set.name);
                    int axisCount = set.set.axisCount;

                    sb.AppendLine($"    public static class {setName}");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        private const string KEY = \"{set.name}\";");
                    sb.AppendLine();
                    sb.AppendLine("        public static bool Down(bool all = false) => SHUU_Input.GetInputDown(Map, KEY, all);");
                    sb.AppendLine("        public static bool Up(bool all = false) => SHUU_Input.GetInputUp(Map, KEY, all);");
                    sb.AppendLine("        public static bool Held(bool all = false) => SHUU_Input.GetInput(Map, KEY, all);");
                    sb.AppendLine();

                    switch (axisCount)
                    {
                        case 1:
                            sb.AppendLine("        public static float Value(bool all = false)");
                            sb.AppendLine("        {");
                            sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                            sb.AppendLine("            return v.TryGetFloat(out var r) ? r : 0f;");
                            sb.AppendLine("        }");
                            break;

                        case 2:
                            sb.AppendLine("        public static Vector2 Value(bool all = false)");
                            sb.AppendLine("        {");
                            sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                            sb.AppendLine("            return v.TryGetVector2(out var r) ? r : Vector2.zero;");
                            sb.AppendLine("        }");
                            break;

                        case 3:
                            sb.AppendLine("        public static Vector3 Value(bool all = false)");
                            sb.AppendLine("        {");
                            sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                            sb.AppendLine("            return v.TryGetVector3(out var r) ? r : Vector3.zero;");
                            sb.AppendLine("        }");
                            break;

                        case 4:
                            sb.AppendLine("        public static Vector4 Value(bool all = false)");
                            sb.AppendLine("        {");
                            sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                            sb.AppendLine("            return v.TryGetVector4(out var r) ? r : Vector4.zero;");
                            sb.AppendLine("        }");
                            break;
                    }

                    sb.AppendLine("    }");
                    sb.AppendLine();
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();

            foreach (char c in s)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
            }

            if (sb.Length == 0 || char.IsDigit(sb[0]))
                sb.Insert(0, '_');

            return sb.ToString();
        }

        // ---------------- FILE INJECTION ----------------

        private static void InjectIntoFile(string filePath, string generatedContent)
        {
            var text = File.ReadAllText(filePath);

            var start = text.IndexOf(StartMarker);
            var end   = text.IndexOf(EndMarker);

            if (start == -1 || end == -1 || end <= start)
            {
                Debug.LogError("Auto-generated input markers not found or invalid.");
                return;
            }

            end += EndMarker.Length;

            // ADD EXACTLY ONE TAB TO EVERY LINE
            var indented =
                "\t" + generatedContent.Replace("\n", "\n\t");

            var newText =
                text.Substring(0, start) +
                StartMarker + "\n" +
                indented +
                EndMarker +
                text.Substring(end);

            File.WriteAllText(filePath, newText);
        }
    }
}
#endif
