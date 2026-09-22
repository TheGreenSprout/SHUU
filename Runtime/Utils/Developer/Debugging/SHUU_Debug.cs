using UnityEngine;
using System.Collections.Generic;

using Alchemy.Inspector;

using SHUU.Utils.Helpers;
using SHUU.Utils.Developer.Debugging.Systems;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-10000)]
    public class SHUU_Debug : Singleton_MonoBehaviour<SHUU_Debug>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        private Debug_ColliderVisualizer colliderVisualizer;

        private Debug_ScreenLogs screenLogs;



        #region Method Bind
        [SerializeField, BoxGroup("Method Bind"), LabelText("Enabled")] internal bool methodBind_enabled = false;
        #endregion



        #region Debug Info
        [SerializeField, BoxGroup("Debug Info"), LabelText("Enabled")] public bool debugInfo_enabled = false;
        #endregion



        #region Screen Logs
        [SerializeField, BoxGroup("Screen Logs"), LabelText("Enabled")]
        public bool screenLogs_enabled = false;


        [SerializeField, BoxGroup("Screen Logs"), LabelText("ListenForDebugLogs"), ShowIf("screenLogs_enabled")]
        internal bool screenLogs_listenForDebugLogs = true;

        private bool screenLogs_showFlags => screenLogs_enabled && screenLogs_listenForDebugLogs;
        [SerializeField, BoxGroup("Screen Logs/Flags"), LabelText("ListenForNormalLogs"), ShowIf("screenLogs_showFlags")]
        internal bool screenLogs_listenForNormalLogs = true;
        [SerializeField, BoxGroup("Screen Logs/Flags"), LabelText("ListenForWarningLogs"), ShowIf("screenLogs_showFlags")]
        internal bool screenLogs_listenForWarningLogs = true;
        [SerializeField, BoxGroup("Screen Logs/Flags"), LabelText("ListenForErrorLogs"), ShowIf("screenLogs_showFlags")]
        internal bool screenLogs_listenForErrorLogs = true;
        [SerializeField, BoxGroup("Screen Logs/Flags"), LabelText("ListenForExceptionLogs"), ShowIf("screenLogs_showFlags")]
        internal bool screenLogs_listenForExceptionLogs = true;
        [SerializeField, BoxGroup("Screen Logs/Flags"), LabelText("ListenForAssertLogs"), ShowIf("screenLogs_showFlags")]
        internal bool screenLogs_listenForAssertLogs = true;


        [SerializeField, BoxGroup("Screen Logs/Misc"), LabelText("DefaultTextColor"), ShowIf("screenLogs_enabled")]
        internal Color screenLogs_defaultTextColor = Color.white;
        [SerializeField, BoxGroup("Screen Logs/Misc"), LabelText("LogMessagePrefab"), ShowIf("screenLogs_enabled")]
        internal Debug_LogMessage screenLogs_logMessagePrefab;


        [SerializeField, BoxGroup("Screen Logs/Misc"), LabelText("InitialPoolSize"), Tooltip("If 0, the screen logs won't use an object pool."), ShowIf("screenLogs_enabled"), Min(0)]
        internal int screenLogs_initialPoolSize = 10;
        #endregion



        #region Debug Colliders
        [SerializeField, BoxGroup("Colliders"), LabelText("Enabled")]
        internal bool colliderVisualizer_enabled;


        [SerializeField, BoxGroup("Colliders"), LabelText("Begin Enabled"), ShowIf("colliderVisualizer_enabled")]
        internal bool colliderVisualizer_beginEnabled = false;
        
        #if ENABLE_INPUT_SYSTEM
        [SerializeField, BoxGroup("Colliders/Input"), LabelText("Activation Key"), ShowIf("colliderVisualizer_enabled")]
        internal KeyCode colliderVisualizer_activationKey = KeyCode.None;
        #endif
        [SerializeField, BoxGroup("Colliders/Input"), LabelText("Activation Action Path"), ShowIf("colliderVisualizer_enabled")]
        internal string colliderVisualizer_activationActionPath = "Developer/Colliders_Toggle";


        [SerializeField, BoxGroup("Colliders/Rendering"), LabelText("Material Shader"), ShowIf("colliderVisualizer_enabled")]
        internal Shader colliderVisualizer_matShader;

        [Tooltip("If true the wire will be rendered on top of all geometry.")]
        [SerializeField, BoxGroup("Colliders/Rendering"), LabelText("Always Render Wire"), ShowIf("colliderVisualizer_enabled")]
        internal bool colliderVisualizer_alwaysRenderWire = false;
        [Tooltip("If true the fill will be rendered on top of all geometry.")]
        [SerializeField, BoxGroup("Colliders/Rendering"), LabelText("Always Render Fill"), ShowIf("colliderVisualizer_enabled")]
        internal bool colliderVisualizer_alwaysRenderFill = false;

        [Tooltip("If 0, the colliders will have to be updated manually via CacheColliders() or CacheReload().")]
        [SerializeField, BoxGroup("Colliders/Settings"), LabelText("Update Colliders Interval"), ShowIf("colliderVisualizer_enabled")]
        internal float colliderVisualizer_updateCollidersInterval = 5f;
        [Tooltip("If 0, the colliders will have to be updated manually via RebuildCache() or CacheReload().")]
        [SerializeField, BoxGroup("Colliders/Settings"), LabelText("Rebuild Cache Interval"), ShowIf("colliderVisualizer_enabled")]
        internal float colliderVisualizer_rebuildCacheInterval = 0.15f;

        [Tooltip("Colliders with a distance from the Camera.main greater than this will not be rendered. If 0, there will be no distance limit.")]
        [SerializeField, BoxGroup("Colliders/Settings"), LabelText("Max Distance"), ShowIf("colliderVisualizer_enabled"), Min(0)]
        internal float colliderVisualizer_maxDistance = 80f;


        [SerializeField, BoxGroup("Colliders/Colors"), LabelText("Default Wire Color"), ShowIf("colliderVisualizer_enabled")]
        internal Color colliderVisualizer_defaultWireColor = Color.green;
        [SerializeField, BoxGroup("Colliders/Colors"), LabelText("Default Fill Color"), ShowIf("colliderVisualizer_enabled")]
        internal Color colliderVisualizer_defaultFillColor = new(0, 0, 0, 0);

        [SerializeField, BoxGroup("Colliders/Colors"), LabelText("Trigger Alpha Multiplier"), ShowIf("colliderVisualizer_enabled"), Range(0f, 1f)]
        internal float colliderVisualizer_triggerAlphaMultiplier = 0.6f;
        [SerializeField, BoxGroup("Colliders/Colors"), LabelText("Disabled Alpha Multiplier"), ShowIf("colliderVisualizer_enabled"), Range(0f, 1f)]
        internal float colliderVisualizer_disabledAlphaMultiplier = 0.3f;


        [SerializeField, BoxGroup("Colliders/Overrides"), ShowIf("colliderVisualizer_enabled")]
        internal LayerMask colliderVisualizer_excludedLayers;
        [SerializeField, BoxGroup("Colliders/Overrides"), ShowIf("colliderVisualizer_enabled")]
        internal TagMask colliderVisualizer_excludedTags = new();

        [SerializeField, BoxGroup("Colliders/Overrides"), ShowIf("colliderVisualizer_enabled")]
        internal List<CustomColors> colliderVisualizer_customColors = new();
        #endregion

        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();


            colliderVisualizer = transform.SearchComponent_InSelfAndChildren<Debug_ColliderVisualizer>();

            screenLogs = transform.SearchComponent_InSelfAndChildren<Debug_ScreenLogs>();
        }
        #endregion



        #region Proxy
        
        #region Debug Colliders
        public static bool? DebugColliders_Toggle(bool? toggle = null)
        {
            if (!instance || !instance.colliderVisualizer) return null;

            return instance.colliderVisualizer.Toggle(toggle);
        }

        public static bool? DebugColliders_ToggleWireRender(bool? toggle = null)
        {
            if (!instance || !instance.colliderVisualizer || !instance.colliderVisualizer_enabled) return null;

            return instance.colliderVisualizer.Toggle_WireRender(toggle);
        }
        public static bool? DebugColliders_ToggleFillRender(bool? toggle = null)
        {
            if (!instance || !instance.colliderVisualizer || !instance.colliderVisualizer_enabled) return null;

            return instance.colliderVisualizer.Toggle_FillRender(toggle);
        }

        public static bool DebugColliders_CacheReload()
        {
            if (!instance || !instance.colliderVisualizer || !instance.colliderVisualizer.proxy || !instance.colliderVisualizer_enabled) return false;

            instance.colliderVisualizer.CacheReload();
            return true;
        }
        public static bool DebugColliders_CacheColliders()
        {
            if (!instance || !instance.colliderVisualizer || !instance.colliderVisualizer.proxy || !instance.colliderVisualizer_enabled) return false;

            instance.colliderVisualizer.CacheColliders();
            return true;
        }
        public static bool DebugColliders_RebuildCache()
        {
            if (!instance || !instance.colliderVisualizer || !instance.colliderVisualizer.proxy || !instance.colliderVisualizer_enabled) return false;

            instance.colliderVisualizer.RebuildCache();
            return true;
        }
        #endregion



        #region Screen Logs
        public static bool? ScreenLogs_Toggle()
        {
            if (!instance || instance.screenLogs == null) return null;

            return instance.screenLogs_enabled = !instance.screenLogs_enabled;
        }
        public static bool? ScreenLogsListener_Toggle()
        {
            if (!instance || instance.screenLogs == null || !instance.screenLogs_enabled) return null;

            return instance.screenLogs_listenForDebugLogs = !instance.screenLogs_listenForDebugLogs;
        }

        public static void ScreenLog(string message, Color? color = null) => instance?.screenLogs?.ScreenLog(message, color);
        public static void ScreenLog_Warning(string message) => instance?.screenLogs?.ScreenLog_Warning(message);
        public static void ScreenLog_Error(string message) => instance?.screenLogs?.ScreenLog_Error(message);
        #endregion
    
        #endregion
    }
}
