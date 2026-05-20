using UnityEngine;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-10000)]
    [RequireComponent(typeof(Debug_ScreenLogs), typeof(Debug_ColliderVisualizer))]
    public class SHUU_Debug : Singleton_MonoBehaviour<SHUU_Debug>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        private Debug_ColliderVisualizer colliderVisualizer;

        private Debug_ScreenLogs screenLogs;
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();


            colliderVisualizer = GetComponent<Debug_ColliderVisualizer>();
            colliderVisualizer?.Init();

            screenLogs = GetComponent<Debug_ScreenLogs>();
        }
        #endregion



        #region Proxy
        
        #region Debug Colliders
        public static bool? Toggle_DebugColliders()
        {
            if (!instance || !instance.colliderVisualizer) return null;

            return instance.colliderVisualizer.Toggle();
        }
        #endregion



        #region Screen Logs
        public static bool? Toggle_ScreenLogs()
        {
            if (!instance || instance.screenLogs == null) return null;

            return instance.screenLogs.Toggle();
        }

        public static void ScreenLog(string message, Color? color = null) => instance?.screenLogs?.ScreenLog(message, color);
        public static void ScreenLog_Warning(string message) => instance?.screenLogs?.ScreenLog_Warning(message);
        public static void ScreenLog_Error(string message) => instance?.screenLogs?.ScreenLog_Error(message);
        #endregion
    
        #endregion
    }
}
