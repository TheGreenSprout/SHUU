#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_EditorInspectorProvider : PreferencesProviderBase<SHUUPreferences_EditorInspectorProvider, SHUUPreferences_EditorInspector>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_EditorInspector Preferences() => SHUUPreferences_EditorInspector.Instance;
        #endregion

        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Editor/Inspector");

        public SHUUPreferences_EditorInspectorProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Play Mode Saver",
                "playModeSaver_enabled",
                "playModeSaver_debugLogEmission");
        }
        #endregion
    }
}
#endif
