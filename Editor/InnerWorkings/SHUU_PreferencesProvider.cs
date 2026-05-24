#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

using static SETB.EditorGUI_Base;

namespace SHUU._Editor.InnerWorkings
{
    public class SHUU_PreferencesProvider : SettingsProvider
    {
        #region Variables
        private SerializedObject _serializedObject;
        private SHUU_Preferences _preferences => SHUU_Preferences.instance;



        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new SHUU_PreferencesProvider("Project/SHUU Preferences", SettingsScope.Project);
        #endregion




        #region Main
        public SHUU_PreferencesProvider(string path, SettingsScope scope) : base(path, scope) { }


        public override void OnActivate(string searchContext, VisualElement rootElement) => _serializedObject = new SerializedObject(_preferences);


        public override void OnGUI(string searchContext)
        {
            if (_preferences == null)
            {
                EditorGUILayout.HelpBox("Preferences asset missing.", MessageType.Error);
                return;
            }

            _serializedObject.Update();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 400f;

            DrawCategory("Input System",
                "inputSystem_debugLogEmission",
                "_inputSystem_mapDisabledWarning_debugLogEmission");

            DrawCategory("Handy Classes",
                "singleton_debugLogEmission");

            DrawCategory("Scene Loader",
                "sceneLoader_fallbackSceneName",
                "sceneLoader_loadingSceneName",
                "sceneLoader_useLoadingScreenDefault",
                "sceneLoader_debugLogEmission");

            DrawCategory("Data Manager",
                "dataManager_debugLogEmission",
                "dataManager_warningLogEmission",
                "dataManager_errorLogEmission");

            DrawCategory("Saving",
                "saving_debugLogEmission");

            DrawCategory("Random System",
                "randomSystem_debugLogEmission");

            DrawCategory("UI",
                "ui_debugLogEmission");

            EditorGUIUtility.labelWidth = previousLabelWidth;

            _serializedObject.ApplyModifiedProperties();

            if (GUI.changed) EditorUtility.SetDirty(_preferences);
        }
        #endregion
        


        #region Logic
        private void DrawCategory(string title, params string[] properties)
        {
            Space(10);

            Vertical(() =>
            {
                DrawLabel(title, EditorStyles.boldLabel);

                foreach (string propertyName in properties)
                {
                    SerializedProperty property = _serializedObject.FindProperty(propertyName);
    
                    if (property != null) DrawInputProperty(null, property);
                }
            }, "box");
        }
        #endregion
    }
}
#endif
