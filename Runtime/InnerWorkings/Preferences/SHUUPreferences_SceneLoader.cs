using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_SceneLoader), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_SceneLoader")]
    public sealed class SHUUPreferences_SceneLoader : PreferencesBase<SHUUPreferences_SceneLoader>
    {
        #region Preferences
        public string fallbackSceneName = "ErrorScene";
        public string loadingSceneName = "LoadingScene";


        public bool useLoadingScreenDefault = true;

        public bool debugLogEmission = false;    
        #endregion
    }
}
