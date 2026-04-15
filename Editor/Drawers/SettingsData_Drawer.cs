#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using SHUU.UserSide.Commons;
using SHUU.Utils.SettingsSytem;
using UnityEditor;
using UnityEngine;

using SETB;
using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SettingsData))]
    public class SettingsData_Drawer : Editor_Base<SettingsData_Drawer>
    {
        #region Variables
        private const string editorPrefID_owner = "SettingsData";
        private const string editorPrefID_namespace= "SHUU";

        private EditorPrefID editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner);



        private SettingsData settingsData;


        private SerializedProperty settingsName;

        private SerializedProperty fields;
        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

            
            settingsData = (SettingsData)target;

            settingsName = Prop("settingsName");
            fields = Prop("fields");


            if (settingsName.stringValue != editorPrefID_localID.tag)
            {
                var from = editorPrefID_localID;
                editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner, settingsName.stringValue);

                MoveAndDeleteEditorPrefs(from, editorPrefID_localID);
            } 
        }


        protected override void DrawInspector()
        {
            bool changeCheck = ChangeCheck(() =>
            {
                var oldName = settingsName.stringValue;
                DrawInputProperty(null, settingsName);
                if (settingsName.stringValue != oldName)
                {
                    var from = editorPrefID_localID;
                    editorPrefID_localID = new EditorPrefID(editorPrefID_owner, editorPrefID_owner, settingsName.stringValue);

                    MoveAndDeleteEditorPrefs(from, editorPrefID_localID);
                } 


                Space(10);

                Horizontal(() =>
                {
                    DrawLabel("Current Settings", EditorStyles.boldLabel);
                    DrawButton("Add Field", () => fields.InsertArrayElementAtIndex(fields.arraySize));
                });

                DrawFieldList(fields);

                Space(10);

                DrawLabel("Last Default Set:", string.IsNullOrEmpty(settingsData.lastDefaultSetDateTime) ? "Never" : settingsData.lastDefaultSetDateTime);
                DrawDefaultsButtons();
            });
            

            if (changeCheck)
            {
                settingsData.NotifyChanged(null);

                UtilitySetDirty(settingsData);
            }
        }
        #endregion



        #region Drawing
            #region UI
            private void DrawDefaultsButtons()
            {
                Horizontal(() =>
                {
                    DrawButton("Save Current As Defaults", () =>
                    {
                        RecordTarget("Save Defaults");

                        settingsData.SaveAsDefaults();

                        UtilitySetDirty(target);
                    });


                    DrawButton("Restore Defaults", () =>
                    {
                        RecordTarget("Restore Defaults");

                        settingsData.RestoreDefaults();

                        UtilitySetDirty(target);
                    });
                });
            }

            private void DrawFieldList(SerializedProperty list)
            {
                Horizontal(() =>
                {
                    DrawButton("Expand All", () => SetAllFoldouts(true));
                    DrawButton("Collapse All", () => SetAllFoldouts(false));
                });

                Space(4);

                for (int i = 0; i < list.arraySize; i++)
                {
                    SerializedProperty field = list.GetArrayElementAtIndex(i);
                    SerializedProperty keyProp = PropRelative(field, "key");

                    string key = keyProp.stringValue;
                    if (string.IsNullOrEmpty(key)) key = $"Field_{i}";

                    string foldKey = $"{settingsData.GetInstanceID()}_{key}";
                    bool open = GetEditorPref(foldKey, true);
                    

                    bool brk = false;
                    Vertical(() =>  
                    {
                        bool newOpen = open;
                        Horizontal(() =>
                        {
                            DrawFoldout(key, ref newOpen, null, null, true);
                            keyProp.stringValue = DrawInputString(null, keyProp.stringValue);


                            DrawButton("X", () =>
                            {
                                list.DeleteArrayElementAtIndex(i);
                                DeleteEditorPref(foldKey);

                                brk = true;
                            }, null, GUILayout.Width(22));
                        });
                        if (brk) return;


                        if (newOpen != open) SetEditorPref(foldKey, newOpen);

                        if (newOpen)
                        {
                            SerializedProperty type = PropRelative(field, "type");
                            DrawInputProperty(null, type);

                            DrawValue(field, (SettingType)type.enumValueIndex);
                            DrawMinMaxControls(field, (SettingType)type.enumValueIndex);
                        }
                    }, "box");

                    if (brk) break;


                    Space(4);
                }
            }
            #endregion

            
            #region Values
            private void DrawValue(SerializedProperty field, SettingType type)
            {
                switch (type)
                {
                    case SettingType.Bool:
                        DrawInputProperty(null, PropRelative(field, "boolValue"));
                        break;

                    case SettingType.String:
                        DrawInputProperty(null, PropRelative(field, "stringValue"));
                        break;

                    case SettingType.Int:
                        DrawIntValue(field);
                        break;

                    case SettingType.Float:
                        DrawFloatValue(field);
                        break;
                }
            }

            private void DrawIntValue(SerializedProperty field)
            {
                var value = PropRelative(field, "intValue");
                var useMin = PropRelative(field, "useMin");
                var useMax = PropRelative(field, "useMax");
                var min = PropRelative(field, "intMin");
                var max = PropRelative(field, "intMax");


                try
                {
                    if (useMin.boolValue && useMax.boolValue)
                    {
                        int minVal = min.intValue;
                        int maxVal = max.intValue;

                        if (minVal >= maxVal)
                        {
                            DrawHelpBox("Min must be less than Max", MessageType.Warning);

                            return;
                        }


                        value.intValue = Mathf.Clamp(value.intValue, minVal, maxVal);

                        value.intValue = DrawLabeledSlider("Value", value.intValue, minVal, maxVal);
                    }
                    else
                    {
                        DrawInputProperty(null, value);

                        if (useMin.boolValue) value.intValue = Mathf.Max(value.intValue, min.intValue);
                        if (useMax.boolValue) value.intValue = Mathf.Min(value.intValue, max.intValue);
                    }
                }
                catch { }
            }

            private void DrawFloatValue(SerializedProperty field)
            {
                var value = PropRelative(field, "floatValue");
                var useMin = PropRelative(field, "useMin");
                var useMax = PropRelative(field, "useMax");
                var min = PropRelative(field, "floatMin");
                var max = PropRelative(field, "floatMax");

                
                try
                {
                    if (useMin.boolValue && useMax.boolValue)
                    {
                        float minVal = min.floatValue;
                        float maxVal = max.floatValue;

                        if (float.IsNaN(minVal) || float.IsNaN(maxVal)) return;


                        if (minVal >= maxVal)
                        {
                            DrawHelpBox("Min must be less than Max", MessageType.Warning);

                            return;
                        }

                        value.floatValue = Mathf.Clamp(value.floatValue, minVal, maxVal);

                        value.floatValue = DrawLabeledSlider("Value", value.floatValue, minVal, maxVal);
                    }
                    else
                    {
                        DrawInputProperty(null, value);

                        if (useMin.boolValue) value.floatValue = Mathf.Max(value.floatValue, min.floatValue);
                        if (useMax.boolValue) value.floatValue = Mathf.Min(value.floatValue, max.floatValue);
                    }
                }
                catch { }
            }
            #endregion

            
            #region Misc
            private void DrawMinMaxControls(SerializedProperty field, SettingType type)
            {
                if (type != SettingType.Int && type != SettingType.Float) return;


                var useMin = PropRelative(field, "useMin");
                var useMax = PropRelative(field, "useMax");


                Horizontal(() =>
                {
                    DrawInputProperty("Min", useMin);
                    DrawInputProperty("Max", useMax);
                });


                if (type == SettingType.Int)
                {
                    if (useMin.boolValue) DrawInputProperty(null, PropRelative(field, "intMin"));
                    if (useMax.boolValue) DrawInputProperty(null, PropRelative(field, "intMax"));
                }
                else
                {
                    if (useMin.boolValue) DrawInputProperty(null, PropRelative(field, "floatMin"));
                    if (useMax.boolValue) DrawInputProperty(null, PropRelative(field, "floatMax"));
                }
            }
            #endregion
        #endregion

        

        #region Helpers
        private void SetAllFoldouts(bool expand)
        {
            if (fields == null) return;

            for (int i = 0; i < fields.arraySize; i++)
            {
                string key = PropRelative(fields.GetArrayElementAtIndex(i), "key")?.stringValue;
                if (string.IsNullOrEmpty(key)) key = $"Field_{i}";

                string foldKey = $"{settingsData.GetInstanceID()}_{key}";
                SetEditorPref(foldKey, expand);
            }
        }
        #endregion
    }




    /*
    ⚠️‼️ AI ASSISTED CODE

    This code was written with the assistance of AI.
    */
    #region Static Generator
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

            var loader = Resources.Load<ScriptableObjectLoader>("ScriptableObjectLoader_Asset");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<SettingsData>(path);
                if (data != null)
                {
                    result.Add(data);

                    if (loader != null)
                    {
                        loader.Track(data);

                        AssetDatabase.SaveAssets();
                    }
                }
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
    #endregion
}
#endif
