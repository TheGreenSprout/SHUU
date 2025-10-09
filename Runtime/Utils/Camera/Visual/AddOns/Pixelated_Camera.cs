using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class Pixelated_Camera : Shader_CameraAddOn
    {
        [Header("Pixelation Settings")]
        [Tooltip("Desired pixel block size in screen pixels (width & height).")]
        public float pixelBlockSize = 2;




        #region Effect Logic
        protected override bool ChangeMaterialValues(bool internalCall = false)
        {
            _proxy._combinedMaterial.SetFloat("_EnablePixelate", base.ChangeMaterialValues() ? 1f : 0f);


            Vector2 screenValues = HandyFunctions.GetCurrentScreenSize();

            Vector2 screen = screenValues;
            Vector2 blockCount = screen / pixelBlockSize;
            Vector2 blockSize = new Vector2(1.0f, 1.0f) / blockCount;
            Vector2 halfBlockSize = blockSize * 0.5f;


            _proxy._combinedMaterial.SetVector("_BlockCount", new Vector4(blockCount.x, blockCount.y, 0f, 0f));
            _proxy._combinedMaterial.SetVector("_BlockSize", new Vector4(blockSize.x, blockSize.y, 0f, 0f));
            _proxy._combinedMaterial.SetVector("_HalfBlockSize", new Vector4(halfBlockSize.x, halfBlockSize.y, 0f, 0f));


            return false;
        }

        protected override void RemoveMaterialValues()
        {
            if (_proxy && _proxy._combinedMaterial) _proxy._combinedMaterial.SetFloat("_EnablePixelation", 0f);
        }
        #endregion
    }
}










#region Old version (render texture resizing)
/*using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class Pixelated_Camera : CameraAddOn
    {
        // Internal
        private Vector2Int screenValues = Vector2Int.zero;

        private Vector2Int closestRatio = Vector2Int.zero;



        // External
        [Header("Pixelation Settings")]
        [Tooltip("Multiplier for the aspect ratio. Smaller, more pixelated")]
        [Min(1)][SerializeField] private int scaleMultiplier = 50;

        [SerializeField] private bool exactScreenSize = true;




        #region Setup
        protected override void Reset()
        {
            base.Reset();


            if (activePipeline != CameraAddOns_Proxy.Pipeline.BuiltIn) URP_HDRP_Logic();
        }


        public void SetScreenValues(Vector2Int? customResolution = null)
        {
            if (customResolution != null)
            {
                screenValues = (Vector2Int)customResolution;
            }
            else
            {
                screenValues = HandyFunctions.GetCurrentScreenSize();
            }

            closestRatio = HandyFunctions.GetClosestAspectRatio(screenValues, exactScreenSize);
        }
        #endregion



        #region Effect Logic
        public override void URP_HDRP_Logic(bool internalCall = false)
        {
            if (rt != null) rt.Release();

            if (internalCall) SetScreenValues();


            int rtWidth = closestRatio.x * scaleMultiplier;
            int rtHeight = closestRatio.y * scaleMultiplier;


            rt = new RenderTexture(rtWidth, rtHeight, 16);
            rt.filterMode = FilterMode.Point;



            if (internalCall) _proxy.Update_RenderTexture();
        }


        public override void BuiltIn_Logic()
        {
            int expectedWidth = closestRatio.x * scaleMultiplier;
            int expectedHeight = closestRatio.y * scaleMultiplier;

            if (rt == null || rt.width != expectedWidth || rt.height != expectedHeight)
                URP_HDRP_Logic();

            //Graphics.Blit(rt, dest);
        }
        #endregion



        #region External handling
        public override void RefreshEffect(bool internalCall = false)
        {
            SetScreenValues();

            base.RefreshEffect(internalCall);
        }
        #endregion
    }
}*/
#endregion