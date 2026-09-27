#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

using SHUU.InnerWorkings.Preferences;
using SHUU.Utils.Helpers;

using static SETB.EditorGUI_Base;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    public abstract class PreferencesProviderBase<TProvider, TBase> : SettingsProvider
        where TProvider : PreferencesProviderBase<TProvider, TBase>
        where TBase : PreferencesBase<TBase>
    {
        #region Variables
        private SerializedObject serializedObject;

        
        private TBase fallbackAsset;
        private string fallbackPath;
        private bool searchedForFallback;



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


        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            fallbackAsset = null;
            fallbackPath = null;
            searchedForFallback = false;


            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();

            _ = Preferences();

            if (timer.ElapsedMilliseconds > 250) Debug.LogWarning($"[SHUU] Fetching the '{label}' preferences took {timer.ElapsedMilliseconds} ms.");
        }

        public override void OnDeactivate()
        {
            serializedObject?.Dispose();
            serializedObject = null;
        }


        public override void OnGUI(string searchContext)
        {
            #region Failsafe
            TBase preferences = Preferences();
            bool foundByResources = preferences != null;

            if (!foundByResources)
            {
                if (!searchedForFallback)
                {
                    FindFallbackAsset();
                    searchedForFallback = true;
                }

                preferences = fallbackAsset;
            }


            string resourcesPath = Singleton_ScriptableObject<TBase>.ResourcesPath;

            if (preferences == null)
            {
                EditorGUILayout.HelpBox($"Preferences asset missing. It has to be an asset of type {typeof(TBase).Name} inside a folder named 'Resources', at 'Resources/{resourcesPath}.asset'.", MessageType.Error);
                return;
            }

            if (!foundByResources)
                EditorGUILayout.HelpBox($"Resources.Load(\"{resourcesPath}\") can't find this asset, so the game can't either. It's at '{fallbackPath}', which has to end in 'Resources/{resourcesPath}.asset'.", MessageType.Warning);
            #endregion


            #region Draw
            if (serializedObject == null || serializedObject.targetObject != preferences) serializedObject = new SerializedObject(preferences);

            serializedObject.Update();

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 400f;

            try { Categories(); }
            finally { EditorGUIUtility.labelWidth = previousLabelWidth; }

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed) EditorUtility.SetDirty(preferences);
            #endregion
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

                    if (property != null) DrawInputProperty(new GUIContent(property.displayName, property.tooltip), property);
                }
            }, "box");
        }


        private void FindFallbackAsset()
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(TBase).Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TBase asset = AssetDatabase.LoadAssetAtPath<TBase>(path);

                if (asset == null) continue;

                fallbackAsset = asset;
                fallbackPath = path;

                return;
            }
        }
        #endregion
    }
}
#endif
