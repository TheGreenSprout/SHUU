using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-10000)]
    [RequireComponent(typeof(Debug_ScreenLogs), typeof(Debug_ColliderVisualizer))]
    public class SHUU_Debug : StaticInstance_Monobehaviour<SHUU_Debug>
    {
        private Debug_ColliderVisualizer colliderVisualizer;

        private Debug_ScreenLogs screenLogs;




        protected override void Awake()
        {
            base.Awake();


            colliderVisualizer = GetComponent<Debug_ColliderVisualizer>();

            screenLogs = GetComponent<Debug_ScreenLogs>();
        }



        public static bool? Toggle_DebugColliders()
        {
            if (!instance || !instance.colliderVisualizer) return null;


            return instance.colliderVisualizer.Toggle();
        }


        public static bool? Toggle_ScreenLogs()
        {
            if (!instance || !instance.screenLogs) return null;


            instance.screenLogs.listenForDebugLog = !instance.screenLogs.listenForDebugLog;

            return instance.screenLogs.listenForDebugLog;
        }
        public static void ScreenLog(string message, Color? color = null) => instance?.screenLogs?.ScreenLog(message, color);
        public static void ScreenLog_Warning(string message) => instance?.screenLogs?.ScreenLog_Warning(message);
        public static void ScreenLog_Error(string message) => instance?.screenLogs?.ScreenLog_Error(message);
    }
}
