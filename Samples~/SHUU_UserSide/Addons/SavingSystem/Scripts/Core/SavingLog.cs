using UnityEngine;

using SHUU.InnerWorkings.Preferences;

namespace SHUU.UserSide.Addons.SavingSystem
{
    internal static class SavingLog
    {
        #region Variables
        private const string Prefix = "[Saving] ";

        private static bool DebugLogEmission => SHUUPreferences_Saving.Instance != null && SHUUPreferences_Saving.Instance.debugLogEmission;
        private static bool WarningLogEmission => SHUUPreferences_Saving.Instance == null || SHUUPreferences_Saving.Instance.warningLogEmission;
        private static bool ErrorLogEmission => SHUUPreferences_Saving.Instance == null || SHUUPreferences_Saving.Instance.errorLogEmission;
        #endregion




        #region Logic
        public static void Info(string message)
        {
            if (DebugLogEmission) Debug.Log(Prefix + message);
        }

        public static void Warning(string message)
        {
            if (WarningLogEmission) Debug.LogWarning(Prefix + message);
        }

        public static void Error(string message)
        {
            if (ErrorLogEmission) Debug.LogError(Prefix + message);
        }
        #endregion
    }
}
