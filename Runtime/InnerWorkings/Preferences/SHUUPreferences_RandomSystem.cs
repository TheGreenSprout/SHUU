using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_RandomSystem), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_RandomSystem")]
    public sealed class SHUUPreferences_RandomSystem : PreferencesBase<SHUUPreferences_RandomSystem>
    {
        #region Preferences
        public bool debugLogEmission = false;
        #endregion
    }
}
