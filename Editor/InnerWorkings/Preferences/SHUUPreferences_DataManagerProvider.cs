#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_DataManagerProvider : PreferencesProviderBase<SHUUPreferences_DataManagerProvider, SHUUPreferences_DataManager>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_DataManager Preferences() => SHUUPreferences_DataManager.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Data Manager");

        public SHUUPreferences_DataManagerProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Data Manager",
                "debugLogEmission",
                "warningLogEmission",
                "errorLogEmission");
        }
        #endregion
    }
}
#endif
