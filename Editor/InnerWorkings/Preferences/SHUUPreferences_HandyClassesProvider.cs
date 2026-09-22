#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_HandyClassesProvider : PreferencesProviderBase<SHUUPreferences_HandyClassesProvider, SHUUPreferences_HandyClasses>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_HandyClasses Preferences() => SHUUPreferences_HandyClasses.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Handy Classes");

        public SHUUPreferences_HandyClassesProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Handy Classes",
                "singleton_debugLogEmission");
        }
        #endregion
    }
}
#endif
