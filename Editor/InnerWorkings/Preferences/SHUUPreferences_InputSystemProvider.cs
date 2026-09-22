#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_InputSystemProvider : PreferencesProviderBase<SHUUPreferences_InputSystemProvider, SHUUPreferences_InputSystem>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_InputSystem Preferences() => SHUUPreferences_InputSystem.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Input System");

        public SHUUPreferences_InputSystemProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Input System",
                "actionAsset",
                "codeGeneration_assetPath",
                "debugLogEmission",
                "mapDisabledWarning_debugLogEmission");
        }
        #endregion
    }
}
#endif
