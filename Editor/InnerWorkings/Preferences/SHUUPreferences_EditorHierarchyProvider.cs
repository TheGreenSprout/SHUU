#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_EditorHierarchyProvider : PreferencesProviderBase<SHUUPreferences_EditorHierarchyProvider, SHUUPreferences_EditorHierarchy>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_EditorHierarchy Preferences() => SHUUPreferences_EditorHierarchy.Instance;
        #endregion

        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Editor/Hierarchy");

        public SHUUPreferences_EditorHierarchyProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Scene Switcher",
                "sceneSwitcher_enabled");
        }
        #endregion
    }
}
#endif
