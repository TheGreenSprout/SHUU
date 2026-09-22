using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_HandyClasses), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_HandyClasses")]
    public sealed class SHUUPreferences_HandyClasses : PreferencesBase<SHUUPreferences_HandyClasses>
    {
        #region Preferences
        public bool singleton_debugLogEmission = false;
        #endregion
    }
}
