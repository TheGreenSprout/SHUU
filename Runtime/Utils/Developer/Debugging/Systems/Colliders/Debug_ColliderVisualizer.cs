using UnityEngine;
using System;
using System.Collections.Generic;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    [DefaultExecutionOrder(-10000)]
    public class Debug_ColliderVisualizer : HiddenSingleton_MonoBehaviour<Debug_ColliderVisualizer>
    {
        #region Variables

        #region Singleton
        protected override bool PersistantSingleton() => false;


        private Debug_ColliderVisualizerProxy _proxy;

        public Debug_ColliderVisualizerProxy proxy
        {
            get => _proxy;
            set
            {
                if (value == null) OnProxyRemoved(_proxy);
                else OnProxyAdded(value);

                _proxy = value;
            }
        }
        #endregion



        #region Inspector
        private bool active => SHUU_Debug.Instance.colliderVisualizer_enabled;
        private bool beginEnabled => SHUU_Debug.Instance.colliderVisualizer_beginEnabled;


        #if ENABLE_INPUT_SYSTEM
        public KeyCode activationKey => SHUU_Debug.Instance.colliderVisualizer_activationKey;
        #endif
        public string activationActionPath => SHUU_Debug.Instance.colliderVisualizer_activationActionPath;


        public Shader matShader => SHUU_Debug.Instance.colliderVisualizer_matShader;

        public bool alwaysRenderWire => SHUU_Debug.Instance.colliderVisualizer_alwaysRenderWire;
        public bool alwaysRenderFill => SHUU_Debug.Instance.colliderVisualizer_alwaysRenderFill;

        public float updateCollidersInterval => SHUU_Debug.Instance.colliderVisualizer_updateCollidersInterval;
        public float rebuildCacheInterval => SHUU_Debug.Instance.colliderVisualizer_rebuildCacheInterval;

        public float maxDistance => SHUU_Debug.Instance.colliderVisualizer_maxDistance;


        public Color defaultWireColor => SHUU_Debug.Instance.colliderVisualizer_defaultWireColor;
        public Color defaultFillColor => SHUU_Debug.Instance.colliderVisualizer_defaultFillColor;

        public float triggerAlphaMultiplier => SHUU_Debug.Instance.colliderVisualizer_triggerAlphaMultiplier;
        public float disabledAlphaMultiplier => SHUU_Debug.Instance.colliderVisualizer_disabledAlphaMultiplier;


        public LayerMask excludedLayers => SHUU_Debug.Instance.colliderVisualizer_excludedLayers;
        public TagMask excludedTags => SHUU_Debug.Instance.colliderVisualizer_excludedTags;

        public List<CustomColors> customColors => SHUU_Debug.Instance.colliderVisualizer_customColors;
        #endregion

        #endregion




        #region Main
        private void OnProxyAdded(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;

            if (active) proxy.Init(this, beginEnabled);
        }

        private void OnProxyRemoved(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;

            if (active) proxy.initialized = false;
        }
        #endregion



        #region Logic
        public bool? Toggle(bool? toggle = null) => proxy && active ? proxy.Toggle(toggle) : null;

        public bool? Toggle_WireRender(bool? toggle = null) => proxy && active ? proxy.Toggle_WireRender(toggle) : null;
        public bool? Toggle_FillRender(bool? toggle = null) => proxy && active ? proxy.Toggle_FillRender(toggle) : null;


        public void CacheReload() => proxy?.CacheReload();

        public void CacheColliders() => proxy?.CacheColliders();
        public void RebuildCache() => proxy?.RebuildCache();
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
