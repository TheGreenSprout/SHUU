using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_DataManager), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_DataManager")]
    public sealed class SHUUPreferences_DataManager : PreferencesBase<SHUUPreferences_DataManager>
    {
        #region Preferences
        public bool debugLogEmission = false;
        public bool warningLogEmission = false;
        public bool errorLogEmission = false;
        #endregion
    }
}
