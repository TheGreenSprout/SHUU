#if UNITY_EDITOR
using UnityEditor;

using SHUU.InnerWorkings.Preferences;

namespace SHUU._Editor.InnerWorkings.Preferences
{
    internal class SHUUPreferences_SceneLoaderProvider : PreferencesProviderBase<SHUUPreferences_SceneLoaderProvider, SHUUPreferences_SceneLoader>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_SceneLoader Preferences() => SHUUPreferences_SceneLoader.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Scene Loader");

        public SHUUPreferences_SceneLoaderProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Scene Loader",
                "fallbackSceneName",
                "loadingSceneName",
                "useLoadingScreenDefault",
                "debugLogEmission");
        }
        #endregion
    }
}
#endif
