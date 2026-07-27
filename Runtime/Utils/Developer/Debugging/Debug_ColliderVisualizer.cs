using UnityEngine;
using System;
using System.Collections.Generic;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-10000)]
    public class Debug_ColliderVisualizer : Singleton_MonoBehaviour<Debug_ColliderVisualizer>
    {
        #region Variables
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



        [Header("Activation")]
        private bool colliderVisualizerEnabled;
        [SerializeField] private bool beginEnabled = false;

        public KeyCode activationKey = KeyCode.None;
        public string activationActionPath = "Developer/Colliders_Toggle";


        [Header("Rendering")]
        public Shader matShader;

        [Tooltip("If true the wire will be rendered on top of all geometry.")]
        public bool alwaysRenderWire = false;
        [Tooltip("If true the fill will be rendered on top of all geometry.")]
        public bool alwaysRenderFill = false;

        [Tooltip("If 0, the colliders will have to be updated manually via CacheColliders() or CacheReload().")]
        public float updateCollidersInterval = 5f;
        [Tooltip("If 0, the colliders will have to be updated manually via RebuildCache() or CacheReload().")]
        public float rebuildCacheInterval = 0.15f;

        [Tooltip("Colliders with a distance from the Camera.main greater than this will not be rendered. If 0, there will be no distance limit.")]
        [Min(0)] public float maxDistance = 80f;


        [Header("Colors")]
        public Color defaultWireColor = Color.green;
        public Color defaultFillColor = new(0, 0, 0, 0);

        [Range(0f, 1f)] public float triggerAlphaMultiplier = 0.6f;
        [Range(0f, 1f)] public float disabledAlphaMultiplier = 0.3f;


        [Header("Overrides")]
        public LayerMask excludedLayers;
        public TagMask excludedTags = new();

        public List<CustomColors> customColors = new();
        #endregion




        #region Main
        public void Init() => colliderVisualizerEnabled = this.enabled;


        private void OnProxyAdded(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;

            if (colliderVisualizerEnabled) proxy.Init(this, beginEnabled);
        }

        private void OnProxyRemoved(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;

            if (colliderVisualizerEnabled) proxy.initialized = false;
        }
        #endregion



        #region Logic
        public bool? Toggle(bool? toggle = null) => proxy && colliderVisualizerEnabled ? proxy.Toggle(toggle) : null;

        public bool? Toggle_WireRender(bool? toggle = null) => proxy && colliderVisualizerEnabled ? proxy.Toggle_WireRender(toggle) : null;
        public bool? Toggle_FillRender(bool? toggle = null) => proxy && colliderVisualizerEnabled ? proxy.Toggle_FillRender(toggle) : null;


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
