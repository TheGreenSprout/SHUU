#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_RandomSystemProvider : PreferencesProviderBase<SHUUPreferences_RandomSystemProvider, SHUUPreferences_RandomSystem>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_RandomSystem Preferences() => SHUUPreferences_RandomSystem.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Random System");

        public SHUUPreferences_RandomSystemProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Random System",
                "debugLogEmission");
        }
        #endregion
    }
}
#endif
