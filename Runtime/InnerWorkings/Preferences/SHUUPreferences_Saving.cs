using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_Saving), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_Saving")]
    public sealed class SHUUPreferences_Saving : PreferencesBase<SHUUPreferences_Saving>
    {
        #region Preferences
        public bool debugLogEmission = false;
        public bool warningLogEmission = true;
        public bool errorLogEmission = true;
        #endregion
    }
}
