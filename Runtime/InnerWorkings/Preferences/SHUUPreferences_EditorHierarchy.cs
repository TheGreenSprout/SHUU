using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_EditorHierarchy), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_EditorHierarchy")]
    public sealed class SHUUPreferences_EditorHierarchy : PreferencesBase<SHUUPreferences_EditorHierarchy>
    {
        #region Preferences
        [Tooltip("Click a scene's name in the Hierarchy to open a dropdown with every scene in the project, where you can search and star your favourites, and switch to the one you pick.")]
        public bool sceneSwitcher_enabled = true;
        #endregion
    }
}
