#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_SettingsSystemProvider : PreferencesProviderBase<SHUUPreferences_SettingsSystemProvider, SHUUPreferences_SettingsSystem>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_SettingsSystem Preferences() => SHUUPreferences_SettingsSystem.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Settings System");

        public SHUUPreferences_SettingsSystemProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Settings System",
                "defaultAsset",
                "codeGeneration_assetPath",
                "debugLogEmission");
        }
        #endregion
    }
}
#endif
