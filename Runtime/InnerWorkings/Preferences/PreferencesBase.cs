using UnityEngine;

using SHUU.Utils.Helpers;

namespace SHUU.InnerWorkings.Preferences
{
    public class PreferencesBase<T> : Singleton_ScriptableObject<T> where T : PreferencesBase<T>
    {
        #region Variables
        protected override string resourcesPath => $"SHUUResources/Preferences/{name}";
        #endregion
    }
}
