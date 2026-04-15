#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SHUU.Utils.InputSystem;
using SHUU.Utils.Helpers;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SHUU.UserSide.Commons;
using System;

using SETB;
using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(InputBindingMap))]
    public class InputBindingMap_Drawer : Editor_Base<InputBindingMap_Drawer>
    {
        #region Variables
        private const string editorPrefID_owner = "InputBindingMap";
        private const string editorPrefID_namespace= "SHUU";

        private EditorPrefID editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner);



        private InputBindingMap map;


        private bool isListening;

        private InputSet listeningSet;

        private int listeningIndex;
        private int listeningControlId;


        private enum BindingKind { Key, Mouse, Axis }
        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();


            map = (InputBindingMap)target;

            map.ForceBuildDictionaries();


            if (map.mapName != editorPrefID_localID.tag)
            {
                var from = editorPrefID_localID;
                editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner, map.mapName);

                MoveAndDeleteEditorPrefs(from, editorPrefID_localID);
            } 
        }


        protected override void DrawInspector()
        {
            DrawHeader();
            DrawSingleSets();
            DrawCompositeSets();
            DrawDefaults();

            HandleListening();

            if (GUI.changed) EditorUtility.SetDirty(map);
        }
        #endregion



        #region Drawing
            #region General
            private new void DrawHeader()
            {
                DrawLabel("Input Binding Map", EditorStyles.boldLabel);


                var oldName = map.mapName;
                DrawInputString("Map Name", ref map.mapName);
                if (map.mapName != oldName)
                {
                    var from = editorPrefID_localID;
                    editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner, map.mapName);

                    MoveAndDeleteEditorPrefs(from, editorPrefID_localID);
                } 

                DrawToggle("Enabled", ref map.enabled);

                Space(10);
            }
            #endregion


            #region Sets
            private void DrawInputSet(InputSet set, bool allowAxis, string label = null)
            {
                if (!string.IsNullOrEmpty(label)) DrawLabel(label, EditorStyles.miniBoldLabel);

                for (int i = 0; i < set.validSources.Count; i++)
                {
                    DrawBindingRow(set, i, allowAxis);
                }

                DrawButton("+ Add Binding", () => set.validSources.Add(new KeySource(KeyCode.None)));
            }
            
            private void DrawSingleSets()
            {
                Horizontal(() =>
                {
                    DrawLabel("Single Input Sets", EditorStyles.boldLabel);
                    DrawButton("+ Add Input Set", () => map.inputSets_list.Add(new NAMED_InputSet { name = "New Set" }));
                });


                for (int i = 0; i < map.inputSets_list.Count; i++)
                {
                    var named = map.inputSets_list[i];
                    string foldKey = GetFoldKey("Single", named.name, i);


                    bool ret = false;

                    Vertical(() =>
                    {
                        bool open = false;

                        Horizontal(() =>
                        {
                            open = false;
                            QuickFoldout(foldKey, named.name, ref open);

                            DrawInputString(null, ref named.name);
                            DrawToggleLeft("Enabled", ref named.set.enabled, null, GUILayout.Width(80));

                            DrawButton("X", () =>
                            {
                                map.inputSets_list.RemoveAt(i);

                                ret = true;
                            }, null, GUILayout.Width(22));
                        });
                        if (ret) return;

                        if (open) DrawInputSet(named.set, true);
                    }, "box");

                    if (ret) return;
                }
            }

            private void DrawCompositeSets()
            {
                Horizontal(() =>
                {
                    DrawLabel("Composite Input Sets", EditorStyles.boldLabel);
                    DrawButton("+ Add Composite Set", () => map.compositeSets_list.Add(new NAMED_Composite_InputSet { name = "New Composite" }));
                });

            
                for (int i = 0; i < map.compositeSets_list.Count; i++)
                {
                    var named = map.compositeSets_list[i];
                    string foldKey = GetFoldKey("Composite", named.name, i);

                    
                    bool ret = false;

                    Vertical(() =>
                    {
                        bool open = false;

                        Horizontal(() =>
                        {
                            open = false;
                            QuickFoldout(foldKey, named.name, ref open);

                            named.set.enabled = DrawToggleLeft("Enabled", named.set.enabled, null, GUILayout.Width(80));


                            DrawButton("X", () =>
                            {
                                map.compositeSets_list.RemoveAt(i);

                                ret = true;
                            }, null, GUILayout.Width(22));
                        });
                        if (ret) return;

                        if (open) Indent(() =>
                        {
                            named.set.axisCount = DrawLabeledSlider("Axis Count", named.set.axisCount, 0, 4);

                            for (int ax = 0; ax < named.set.axes.Count; ax++)
                            {
                                var axis = named.set.axes[ax];

                                string axisKey = GetFoldKey("Axis", named.name, ax);


                                Vertical(() =>
                                {
                                    bool axisOpen = false;
                                    QuickFoldout(axisKey, $"Axis {ax}", ref axisOpen);

                                    if (axisOpen)
                                    {
                                        DrawInputSet(axis.positiveSet.set, false, "Positive");
                                        DrawInputSet(axis.negativeSet.set, false, "Negative");
                                    }
                                }, "helpbox");
                            }
                        });
                    }, "box");

                    if (ret) return;
                }
            }
            #endregion


            #region Misc
            private void DrawBindingRow(InputSet set, int index, bool allowAxis)
            {
                var src = set.validSources[index];

                Horizontal(() =>
                {
                    BindingKind kind = GetKind(src);
                    DrawSelectionPopup(ref kind, null, target, GUILayout.Width(70));

                    if (src.GetType() != GetSourceType(kind))
                    {
                        set.validSources[index] = CreateSource(kind, allowAxis);

                        return;
                    }

                    if (src is KeySource key) DrawSelectionPopup(ref key.key);
                    else if (src is MouseSource mouse) DrawSelectionPopup(ref mouse.mouse, new[] { "Left", "Right", "Middle", "Button 3", "Button 4", "Button 5" });
                    else if (src is AxisSource axis && allowAxis)
                    {
                        Vertical(() =>
                        {
                            InputParser.AxisNames currentEnum = InputParser.ParseAxisEnum(axis.axisName) ?? InputParser.AxisNames.Horizontal;
                            DrawSelectionPopup(ref currentEnum);

                            axis.axisName = new AxisSource(currentEnum, axis.threshold, axis.raw).axisName;

                            DrawLabeledSlider("Threshold", ref axis.threshold, 0f, 1f);
                            DrawToggleLeft("Raw", ref axis.raw, null, GUILayout.Width(50));
                        });
                    }

                    DrawButton("🎧", () =>
                    {
                        isListening = true;
                        listeningSet = set;
                        listeningIndex = index;

                        listeningControlId = GUIUtility.GetControlID(FocusType.Passive);
                        GUIUtility.hotControl = listeningControlId;
                    }, null, GUILayout.Width(30));

                    DrawButton("X", () => set.validSources.RemoveAt(index), null, GUILayout.Width(22));
                }, "box");
            }

            private void DrawDefaults()
            {
                Space(10);


                DrawLabel("Defaults", EditorStyles.boldLabel);

                DrawLabel( "Last Default Set:", string.IsNullOrEmpty(map.lastDefaultSetDateTime) ? "Never" : map.lastDefaultSetDateTime);


                Horizontal(() =>
                {
                    DrawButton("Set Default Data", () =>
                    {
                        Record(map, "Set Default Input Data");

                        map.SaveDefaults();

                        UtilitySetDirty(map);
                    });


                    DrawButton("Reset To Default", () =>
                    {
                        Record(map, "Reset Input Map To Default");

                        map.RestoreDefaults();

                        UtilitySetDirty(map);
                    });
                });
            }
            #endregion
        #endregion



        #region Helpers
            #region Listening
            private void HandleListening()
            {
                if (!isListening) return;


                DrawHelpBox(
                    "Listening for key or mouse input… (Esc to cancel)",
                    MessageType.Info
                );


                Event e = Event.current;
                if (e == null) return;


                if (e.isMouse) GUIUtility.hotControl = listeningControlId;

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
                        listeningSet.validSources[listeningIndex] = new KeySource(e.keyCode);

                        StopListening();
                        e.Use();

                        return;
                    }
                }

                if (e.type == EventType.MouseDown)
                {
                    listeningSet.validSources[listeningIndex] = new MouseSource(e.button);

                    StopListening();
                    e.Use();
                }
            }

            private void StopListening()
            {
                isListening = false;
                listeningSet = null;
                listeningIndex = -1;

                GUIUtility.hotControl = 0;
            }
            #endregion

        
            #region Foldouts
            private bool QuickFoldout(string key, string label, ref bool open, Action logic = null)
            {
                open = GetEditorPref(key, false);

                bool newOpen = open;
                DrawFoldout(label, ref newOpen, logic, null, true);


                if (newOpen != open) SetEditorPref(key, newOpen);

                return newOpen;
            }

            private string GetFoldKey(string type, string name, int index) => $"{map.GetInstanceID()}_{type}_{index}_{name}";
            #endregion

        
            #region Fetching
            private BindingKind GetKind(InputSource src)
            {
                if (src is KeySource) return BindingKind.Key;
                if (src is MouseSource) return BindingKind.Mouse;
                if (src is AxisSource) return BindingKind.Axis;

                return BindingKind.Key;
            }

            private Type GetSourceType(BindingKind kind)
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
            #endregion
        #endregion
    }



    
    /*
    ⚠️‼️ AI ASSISTED CODE

    This code was written with the assistance of AI.
    */
    #region Static Generator
    public static class InputStaticClassGenerator
    {
        // Resources path (no extension)
        private const string ResourcesPath = "AutoGenerated/GeneratedCode_InputSystem";

        // Package filesystem path
        private const string PackagePath = "Packages/com.sproutinggames.sprouts.huu/Runtime/Utils/InputSystem/Resources/AutoGenerated/GeneratedCode_InputSystem.cs";


        private const string StartMarker = "// <auto-generated-input>";
        private const string EndMarker = "// </auto-generated-input>";

        
        
        
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
            var textAsset = Resources.Load<TextAsset>(ResourcesPath);
            if (textAsset != null)
            {
                filePath = AssetDatabase.GetAssetPath(textAsset);
                return true;
            }

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

            var loader = Resources.Load<ScriptableObjectLoader>("ScriptableObjectLoader_Asset");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var map = AssetDatabase.LoadAssetAtPath<InputBindingMap>(path);
                if (map != null)
                {
                    result.Add(map);

                    if (loader != null)
                    {
                        loader.Track(map);

                        AssetDatabase.SaveAssets();
                    }
                }
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
                    AppendInputSet(sb, set.name, 1);
                }

                // ---------- COMPOSITE SETS ----------
                foreach (var set in map.compositeSets_list)
                {
                    if (string.IsNullOrEmpty(set.name)) continue;
                    AppendInputSet(sb, set.name, set.set.axisCount);
                }

                sb.AppendLine("}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void AppendInputSet(StringBuilder sb, string rawName, int axisCount)
        {
            var name = Sanitize(rawName);

            sb.AppendLine($"    public static class {name}");
            sb.AppendLine("    {");
            sb.AppendLine($"        private const string KEY = \"{rawName}\";");
            sb.AppendLine();

            // ---- basic ----
            sb.AppendLine("        public static bool GetDown(bool all = false) => SHUU_Input.GetInputDown(Map, KEY, all);");
            sb.AppendLine("        public static bool GetUp(bool all = false) => SHUU_Input.GetInputUp(Map, KEY, all);");
            sb.AppendLine("        public static bool Get(bool all = false) => SHUU_Input.GetInput(Map, KEY, all);");
            sb.AppendLine();

            // ---- buffered ----
            sb.AppendLine("        public static void RegisterBuffer_Down(float bufferTime = 0.15f, bool all = false) => Map.RegisterBufferInput_Down(KEY, bufferTime, all);");
            sb.AppendLine("        public static void RegisterBuffer_Up(float bufferTime = 0.15f, bool all = false) => Map.RegisterBufferInput_Up(KEY, bufferTime, all);");
            sb.AppendLine("        public static void UnregisterBuffer_Down(bool all = false) => Map.UnregisterBufferInput_Down(KEY, all);");
            sb.AppendLine("        public static void UnregisterBuffer_Up(bool all = false) => Map.UnregisterBufferInput_Up(KEY, all);");
            sb.AppendLine();
            sb.AppendLine("        public static bool GetBufferedInput_Down(bool all = false, bool consume = true) => Map.GetBufferedInput_Down(KEY, all, consume);");
            sb.AppendLine("        public static bool GetBufferedInput_Up(bool all = false, bool consume = true) => Map.GetBufferedInput_Up(KEY, all, consume);");
            sb.AppendLine();

            // ---- listeners ----
            sb.AppendLine("        public static void RegisterListener_Down(System.Action cb, bool all = false) => Map.RegisterListener_Down(KEY, cb, all);");
            sb.AppendLine("        public static void RegisterListener_Up(System.Action cb, bool all = false) => Map.RegisterListener_Up(KEY, cb, all);");
            sb.AppendLine("        public static void UnregisterListener_Down(System.Action cb) => Map.UnregisterListener_Down(KEY, cb);");
            sb.AppendLine("        public static void UnregisterListener_Up(System.Action cb) => Map.UnregisterListener_Up(KEY, cb);");
            sb.AppendLine();

            // ---- value ----
            switch (axisCount)
            {
                case 1:
                    sb.AppendLine("        public static float GetValue(bool all = false)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                    sb.AppendLine("            return v.TryGetFloat(out var r) ? r : 0f;");
                    sb.AppendLine("        }");
                    break;

                case 2:
                    sb.AppendLine("        public static Vector2 GetValue(bool all = false)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                    sb.AppendLine("            return v.TryGetVector2(out var r) ? r : Vector2.zero;");
                    sb.AppendLine("        }");
                    break;

                case 3:
                    sb.AppendLine("        public static Vector3 GetValue(bool all = false)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                    sb.AppendLine("            return v.TryGetVector3(out var r) ? r : Vector3.zero;");
                    sb.AppendLine("        }");
                    break;

                case 4:
                    sb.AppendLine("        public static Vector4 GetValue(bool all = false)");
                    sb.AppendLine("        {");
                    sb.AppendLine("            var v = SHUU_Input.GetInputValue(Map, KEY, all);");
                    sb.AppendLine("            return v.TryGetVector4(out var r) ? r : Vector4.zero;");
                    sb.AppendLine("        }");
                    break;
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        private static string Sanitize(string s)
        {
            var sb = new StringBuilder();

            foreach (char c in s)
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);

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

            // EXACTLY ONE TAB because code is inside a namespace
            var indented = "\t" + generatedContent.Replace("\n", "\n\t");

            var newText =
                text.Substring(0, start) +
                StartMarker + "\n" +
                indented +
                EndMarker +
                text.Substring(end);

            File.WriteAllText(filePath, newText);
        }
    }
    #endregion
}
#endif
