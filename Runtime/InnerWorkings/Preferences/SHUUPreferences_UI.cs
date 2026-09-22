using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_UI), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_UI")]
    public sealed class SHUUPreferences_UI : PreferencesBase<SHUUPreferences_UI>
    {
        #region Preferences
        public bool debugLogEmission = false;
        #endregion
    }
}
