using SHUU.Utils.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SHUU.Utils.Developer.Debugging
{
    [RequireComponent(typeof(Debug_ColliderVisualizer), typeof(Debug_ScreenLogs))]
    public class SHUU_Debug : MonoBehaviour
    {
        private static SHUU_Debug instance;



        [Tooltip("If true at the start of the game, the debug collider visualization will be available.")]
        [SerializeField] private bool debugColliders = false;



        private Debug_ColliderVisualizer colliderVisualizer;

        private Debug_ScreenLogs screenLogs;




        private void Awake()
        {
            instance = this;


            colliderVisualizer = GetComponent<Debug_ColliderVisualizer>();
            if (debugColliders) colliderVisualizer.Init();

            screenLogs = GetComponent<Debug_ScreenLogs>();
        }


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => colliderVisualizer?.Reset();



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
