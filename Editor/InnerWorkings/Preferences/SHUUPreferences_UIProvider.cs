#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_UIProvider : PreferencesProviderBase<SHUUPreferences_UIProvider, SHUUPreferences_UI>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_UI Preferences() => SHUUPreferences_UI.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("UI");

        public SHUUPreferences_UIProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("UI",
                "debugLogEmission");
        }
        #endregion
    }
}
#endif
