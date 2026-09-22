#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using SHUU._Editor.InnerWorkings.Preferences;
using SHUU.UserSide.Addons.AudioSystem.Preferences;

namespace SHUU.UserSide.Addons.AudioSystem._Editor.Preferences
{
    public class SHUUPreferences_AudioSystemProvider : PreferencesProviderBase<SHUUPreferences_AudioSystemProvider, SHUUPreferences_AudioSystem>
    {
        #region Variables

        #region Override points
        protected override SHUUPreferences_AudioSystem Preferences() => SHUUPreferences_AudioSystem.Instance;
        #endregion
        
        #endregion




        #region Main
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => CreateProviderInstance("Audio System");

        public SHUUPreferences_AudioSystemProvider(string path, SettingsScope scope) : base(path, scope) { }


        protected override void Categories()
        {
            DrawCategory("Audio System",
                "codeGeneration_assetPath");
        }
        #endregion
    }
}
#endif
