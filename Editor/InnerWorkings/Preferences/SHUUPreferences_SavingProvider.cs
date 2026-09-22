#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_SavingProvider : PreferencesProviderBase<SHUUPreferences_SavingProvider, SHUUPreferences_Saving>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_Saving Preferences() => SHUUPreferences_Saving.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Saving");

        public SHUUPreferences_SavingProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Saving",
                "debugLogEmission",
                "warningLogEmission",
                "errorLogEmission");
        }
        #endregion
    }
}
#endif
