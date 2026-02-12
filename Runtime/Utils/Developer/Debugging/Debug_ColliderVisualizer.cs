using UnityEngine;
using System.Collections.Generic;

namespace SHUU.Utils.Developer.Debugging
{
    #region Data classes

    [System.Serializable]
    public class LayerWireColor
    {
        public LayerMask layers;
        public Color wireColor;
    }

    [System.Serializable]
    public class TagFillColor
    {
        public List<string> tags = new();
        public Color fillColor;
    }

    #endregion


    
    [DefaultExecutionOrder(-10000)]
    public class Debug_ColliderVisualizer : MonoBehaviour
    {
        private static Debug_ColliderVisualizer _instance;
        
        public static Debug_ColliderVisualizer instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<Debug_ColliderVisualizer>(FindObjectsInactive.Include);

                    if (_instance == null) Debug.LogError("No Debug_ColliderVisualizer found in scene.");
                }

                return _instance;
            }
        }


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
        [SerializeField] private bool colliderVisualizerEnabled = false;
        [SerializeField] private bool beginEnabled = false;

        [SerializeField] private KeyCode activationKey = KeyCode.None;


        [Header("Rendering")]
        [SerializeField] private Shader matShader;

        [Tooltip("If true the wire will be rendered on top of all geometry.")]
        [SerializeField] private bool alwaysRenderWire = false;
        [Tooltip("If true the fill will be rendered on top of all geometry.")]
        [SerializeField] private bool alwaysRenderFill = false;

        [Tooltip("If 0, the colliders will have to be updated manually via CacheColliders() or CacheReload().")]
        [SerializeField] private float updateCollidersInterval = 5f;
        [Tooltip("If 0, the colliders will have to be updated manually via RebuildCache() or CacheReload().")]
        [SerializeField] private float updateCacheInterval = 0.15f;

        [Tooltip("Colliders with a distance from the Camera.main greater than this will not be rendered.")]
        [SerializeField] private float maxDistance = 80f;


        [Header("Colors")]
        [SerializeField] private Color defaultWireColor = Color.green;
        [SerializeField] private Color defaultFillColor = new(0, 0, 0, 0);

        [Range(0f, 1f)] [SerializeField] private float triggerAlphaMultiplier = 0.6f;
        [Range(0f, 1f)] [SerializeField] private float disabledAlphaMultiplier = 0.3f;


        [Header("Overrides")]
        [SerializeField] private LayerMask excludedLayers;
        [SerializeField] private List<string> excludedTags = new();

        [SerializeField] private List<LayerWireColor> layerWireColors = new();
        [SerializeField] private List<TagFillColor> tagFillColors = new();




        private void Awake()
        {
            if (_instance == null) _instance = this;
        }


        private void OnProxyAdded(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;


            if (colliderVisualizerEnabled) proxy.Init(activationKey, matShader, alwaysRenderWire, alwaysRenderFill, updateCollidersInterval, updateCacheInterval, maxDistance, defaultWireColor, defaultFillColor, triggerAlphaMultiplier, disabledAlphaMultiplier, excludedLayers, excludedTags, layerWireColors, tagFillColors, beginEnabled);
        }

        private void OnProxyRemoved(Debug_ColliderVisualizerProxy proxy)
        {
            if (!proxy) return;


            if (colliderVisualizerEnabled) proxy.initialized = false;
        }



        public bool? Toggle() => proxy && colliderVisualizerEnabled ? proxy.Toggle() : null;


        public static void CacheReload() => instance?.proxy?.CacheReload();

        public static void CacheColliders() => instance?.proxy?.CacheColliders();
        public static void RebuildCache() => instance?.proxy?.RebuildCache();
    }
}
