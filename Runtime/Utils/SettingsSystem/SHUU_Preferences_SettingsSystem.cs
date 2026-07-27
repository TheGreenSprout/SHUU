using UnityEngine;

namespace SHUU.Utils.SettingsSystem
{
    public class SHUU_Preferences_SettingsSystem : ScriptableObject
    {
        #region Variables
        private static SHUU_Preferences_SettingsSystem _instance;
        public static SHUU_Preferences_SettingsSystem instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<SHUU_Preferences_SettingsSystem>("SHUU/InnerWorkings/Preferences/SHUU_Preferences_SettingsSystem");

                return _instance;
            }
        }



        public SettingsAtlas defaultAsset;
        public static SettingsAtlas defaultAtlas => instance?.defaultAsset;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => _ = instance;
        #endregion
    }
}
