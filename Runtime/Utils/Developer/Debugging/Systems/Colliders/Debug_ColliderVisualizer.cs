using UnityEngine;
using UnityEngine.SceneManagement;
using System;

using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.InputSystem;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    public static class Debug_ColliderVisualizer
    {
        #region Variables
        public static bool Active => settings != null && settings.colliderVisualizer_enabled;

        public static bool Visible => visible;
        public static bool AlwaysRenderWire => alwaysRenderWire;
        public static bool AlwaysRenderFill => alwaysRenderFill;



        // Internal
        private static SHUU_Debug settings;

        private static ColliderVisualizer_Cache cache;

        private static bool started;

        private static bool visible;
        private static bool alwaysRenderWire;
        private static bool alwaysRenderFill;

        private static SHUU_Timer cacheColliders_timer;
        private static SHUU_Timer rebuildCache_timer;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Dispose();

            settings = null;
            started = false;
            visible = false;
            alwaysRenderWire = false;
            alwaysRenderFill = false;


            SceneLoader.OnSceneLoaded -= HandleSceneLoaded;
            SceneLoader.OnSceneLoaded += HandleSceneLoaded;

            Application.quitting -= Dispose;
            Application.quitting += Dispose;

            SHUU_Time.OnUpdate -= Tick;
            SHUU_Time.OnUpdate += Tick;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            settings = SHUU_Debug.Instance;

            StopTimers();
            cache?.ClearMeshes();

            if (settings == null) return;


            if (!Active) return;

            bool wasStarted = started;

            EnsureStarted();
            cache.camera = Camera.main;

            if (wasStarted && visible) CacheReload();
        }


        private static void Tick()
        {
            if (!Active) return;

            EnsureStarted();
            HandleInput();

            if (!visible) return;

            cache.Draw(alwaysRenderWire, alwaysRenderFill);
        }
        #endregion




        #region Logic

        #region Main
        private static void EnsureStarted()
        {
            cache ??= new ColliderVisualizer_Cache();
            cache.EnsureMaterials(settings.colliderVisualizer_matShader);

            if (started) return;
            started = true;

            alwaysRenderWire = settings.colliderVisualizer_alwaysRenderWire;
            alwaysRenderFill = settings.colliderVisualizer_alwaysRenderFill;
            cache.camera = Camera.main;

            SetVisible(settings.colliderVisualizer_beginEnabled);
        }


        private static void HandleInput()
        {
            string actionPath = settings.colliderVisualizer_activationActionPath;

            if (!string.IsNullOrEmpty(actionPath) && SHUU_Input.GetInputDown(actionPath)) Toggle();
#if ENABLE_LEGACY_INPUT_MANAGER
            else if (settings.colliderVisualizer_activationKey != KeyCode.None && Input.GetKeyDown(settings.colliderVisualizer_activationKey)) Toggle();
#endif
        }


        private static void Dispose()
        {
            StopTimers();

            cache?.Dispose();
            cache = null;
        }
        #endregion



        #region Toggles
        public static bool? Toggle(bool? toggle = null)
        {
            if (!Active) return null;

            EnsureStarted();
            SetVisible(toggle ?? !visible);

            return visible;
        }

        public static bool? Toggle_WireRender(bool? toggle = null) => Active ? (alwaysRenderWire = toggle ?? !alwaysRenderWire) : null;
        public static bool? Toggle_FillRender(bool? toggle = null) => Active ? (alwaysRenderFill = toggle ?? !alwaysRenderFill) : null;


        private static void SetVisible(bool value)
        {
            visible = value;

            if (visible) CacheReload();
            else StopTimers();
        }
        #endregion



        #region Cache
        public static bool CacheReload()
        {
            if (!CacheColliders()) return false;

            return RebuildCache();
        }

        public static bool CacheColliders()
        {
            if (!Active) return false;
            EnsureStarted();

            cacheColliders_timer?.Cancel();
            cacheColliders_timer = visible ? StartTimer(settings.colliderVisualizer_updateCollidersInterval, () => CacheColliders()) : null;

            cache.CacheColliders(settings);
            return true;
        }

        public static bool RebuildCache()
        {
            if (!Active) return false;
            EnsureStarted();

            rebuildCache_timer?.Cancel();
            rebuildCache_timer = visible ? StartTimer(settings.colliderVisualizer_rebuildCacheInterval, () => RebuildCache()) : null;

            cache.Rebuild(settings);
            return true;
        }


        private static SHUU_Timer StartTimer(float seconds, Action onComplete)=> seconds > 0f && SHUU_Time.Instance != null ? SHUU_Time.Timer(seconds, onComplete) : null;

        private static void StopTimers()
        {
            cacheColliders_timer?.Cancel();
            rebuildCache_timer?.Cancel();

            cacheColliders_timer = null;
            rebuildCache_timer = null;
        }
        #endregion

        #endregion
    }




    #region Helper classes
    [Serializable]
    public class CustomColors
    {
        public LayerMask layerMask = 0;
        public TagMask tagMask = TagMask.Nothing;


        [Tooltip("If true, both layer and tag must match. If false, either can match.")]
        public bool useAndMatching = false;

        public bool overrideWireColor = true;
        public Color wireColor;

        public bool overrideFillColor = true;
        public Color fillColor;
    }
    #endregion
}
