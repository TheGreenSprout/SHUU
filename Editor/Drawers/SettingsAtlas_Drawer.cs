#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

using SHUU.Utils.SettingsSystem;
using SHUU._Editor.CodeGeneration;
using SHUU.InnerWorkings.Preferences;

using SETB.SuperClasses;

using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SettingsAtlas))]
    public class SettingsAtlas_Inspector : Editor_Base<SettingsAtlas_Inspector>
    {
        #region Variables
        private SettingsAtlas data => (SettingsAtlas)target;
        #endregion




        #region Main
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID)
        {
            var asset = IdToObject(instanceID) as SettingsAtlas;
            if (asset == null) return false;

            SettingsDataEditorWindow.Open(asset);
            return true;
        }
        #endregion
    
    
    
        #region Override Points
        protected override void DrawInspector()
        {
            using (new EditorGUI.DisabledScope(true)) DrawInputString("Settings Name", data.settingsName);

            Space(6);

            DrawButton("Edit Asset", () => SettingsDataEditorWindow.Open(data), options: GUILayout.Height(28));

            Space(4);

            var config = SHUUPreferences_SettingsSystem.Instance;
            if (config != null && config.defaultAsset == data) DrawHelpBox("This atlas is assigned as the Project-Wide SettingsAtlas for the Settings System.", MessageType.Info);
            else
            {
                string currentPath = config?.defaultAsset != null
                    ? AssetDatabase.GetAssetPath(config.defaultAsset)
                    : null;

                string msg = currentPath != null
                    ? $"This atlas is not assigned as the Project-Wide SettingsAtlas for the Settings System. The atlas currently assigned as the project-wide default is: {currentPath}."
                    : "This atlas is not assigned as the Project-Wide SettingsAtlas for the Settings System. No project-wide default is currently assigned.";

                DrawHelpBox(msg, MessageType.Warning);

                DrawButton("Set as Project-Wide SettingsAtlas", () =>
                {
                    var preferences = SHUUPreferences_SettingsSystem.Instance;
                    if (preferences == null)
                    {
                        Debug.LogError("No SHUU_Preferences_SettingsSystem asset found. Create one in the project settings first.");
                        return;
                    }

                    preferences.defaultAsset = data;
                    EditorUtility.SetDirty(preferences);
                    AssetDatabase.SaveAssets();
                }, options: GUILayout.Height(22));
            }

            Space(4);
            
            Horizontal(() => {
                data.generateStaticReferences = DrawToggle("Auto-Generate Static References", data.generateStaticReferences);
                DrawButton("Generate Static References", () => SettingsSystem_StaticReferencesGenerator.GenerateForAtlas(data));
            });
        }
        #endregion
    }
}
#endif