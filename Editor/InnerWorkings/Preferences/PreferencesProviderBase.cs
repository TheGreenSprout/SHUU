#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

using static SETB.EditorGUI_Base;
using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    public abstract class PreferencesProviderBase<TProvider, TBase> : SettingsProvider
        where TProvider : PreferencesProviderBase<TProvider, TBase>
        where TBase : PreferencesBase<TBase>
    {
        #region Variables
        private SerializedObject serializedObject;
        


        #region Override points
        protected abstract TBase Preferences();
        #endregion


        protected static string PreferencesPathFinal(string pathSuffix) => $"SHUU{(!string.IsNullOrEmpty(pathSuffix) ? $"/{pathSuffix}" : "")}";
        protected static SettingsScope PreferencesSettingsScope() => SettingsScope.User;
        
        #endregion




        #region Main
        public PreferencesProviderBase(string path, SettingsScope scope) : base(path, scope) { }

        //[SettingsProvider]
        public static SettingsProvider CreateProviderInstance(string pathSuffix)
        {
            return (TProvider)Activator.CreateInstance(
                typeof(TProvider),
                PreferencesPathFinal(pathSuffix),
                PreferencesSettingsScope()
            );
        }


        public override void OnActivate(string searchContext, VisualElement rootElement) => serializedObject = new SerializedObject(Preferences());


        public override void OnGUI(string searchContext)
        {
            if (Preferences() == null)
            {
                EditorGUILayout.HelpBox("Preferences asset missing.", MessageType.Error);
                return;
            }

            serializedObject.Update();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 400f;

            Categories();

            EditorGUIUtility.labelWidth = previousLabelWidth;

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed) EditorUtility.SetDirty(Preferences());
        }

        protected abstract void Categories();
        #endregion



        #region Logic
        protected void DrawCategory(string title, params string[] properties)
        {
            Space(10);

            Vertical(() =>
            {
                DrawLabel(title, EditorStyles.boldLabel);

                foreach (string propertyName in properties)
                {
                    SerializedProperty property = serializedObject.FindProperty(propertyName);
    
                    if (property != null) DrawInputProperty(null, property);
                }
            }, "box");
        }
        #endregion
    }
}
#endif
