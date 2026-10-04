using UnityEngine;

namespace SHUU.InnerWorkings.Preferences
{
    //[CreateAssetMenu(fileName = nameof(SHUUPreferences_EditorInspector), menuName = "SHUU/InnerWorkings/Preferences/SHUUPreferences_EditorInspector")]
    public sealed class SHUUPreferences_EditorInspector : PreferencesBase<SHUUPreferences_EditorInspector>
    {
        #region Preferences
        [Tooltip("In Play Mode, every component's header gets a save icon. Turn it on for a component and its values are kept in the scene when you exit Play Mode.")]
        public bool playModeSaver_enabled = true;


        [Tooltip("Logs what the Play Mode saver kept, and what it couldn't.")]
        public bool playModeSaver_debugLogEmission = true;
        #endregion
    }
}
