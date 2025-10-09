using System;
using System.Linq;
using System.Collections.Generic;
using SHUU.Utils.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode, RequireComponent(typeof(Camera))]
    public class CameraAddOns_Proxy : MonoBehaviour
    {
        #region Variables
        // Internal
        private const string COMBINEDSHADER_FILE_LOCATION = "Assets/SHUU/Runtime/Utils/Camera/Visual/Resources/SHUU_CombinedShader.shader";


        [HideInInspector] public enum Pipeline { BuiltIn, URP, HDRP }
        [HideInInspector] public Pipeline activePipeline;


        private List<CameraAddOn> addOns = new List<CameraAddOn>();


        private Camera cam;

        private RenderTexture output_renderTexture;

        private Shader _combinedMaterial_shader;
        [HideInInspector] public Material _combinedMaterial;



        // External
        [Tooltip("Only needed in URP/HDRP")]
        [SerializeField] private RawImage output_rawImage;

        [SerializeField] private FilterMode renderTexture_filterMode = FilterMode.Point;
        #endregion




        #region Initialization
        void Reset()
        {
            cam = GetComponent<Camera>();


            DetectPipeline();

            SetupRenderTexture();
            SetupMaterial();
        }


        public void RegisterAddOn(CameraAddOn addOn)
        {
            if (!addOns.Contains(addOn)) addOns.Add(addOn);
        }

        public void RemoveAddOn(CameraAddOn addOn)
        {
            if (addOns.Contains(addOn))
            {
                addOns.Remove(addOn);


                SetupRenderTexture();
                SetupMaterial();

                Update_Effect();
            }
        }


        private void DetectPipeline()
        {
            RenderPipelineAsset rpAsset = null;

#if UNITY_2022_1_OR_NEWER
            rpAsset = GraphicsSettings.defaultRenderPipeline;
#else
            rpAsset = GraphicsSettings.renderPipelineAsset;
#endif

            if (rpAsset == null)
                activePipeline = Pipeline.BuiltIn;
            else
            {
                string rpName = rpAsset.GetType().ToString();
                if (rpName.Contains("HD")) activePipeline = Pipeline.HDRP;
                else activePipeline = Pipeline.URP;
            }
        }

        private void SetupRenderTexture()
        {
            if (output_renderTexture) output_renderTexture.Release();

            Vector2 screenValues = HandyFunctions.GetCurrentScreenSize();
            Vector2Int int_screenValues = new Vector2Int((int)screenValues.x, (int)screenValues.y);
            output_renderTexture = new RenderTexture(int_screenValues.x, int_screenValues.y, 16, RenderTextureFormat.ARGB32);
            output_renderTexture.filterMode = renderTexture_filterMode;
            output_renderTexture.wrapMode = TextureWrapMode.Repeat;

            Update_RenderTexture();
        }

        private void SetupMaterial()
        {
#if UNITY_EDITOR
            if (!File.Exists(COMBINEDSHADER_FILE_LOCATION)) return;
            else
            {
                _combinedMaterial_shader = AssetDatabase.LoadAssetAtPath<Shader>(COMBINEDSHADER_FILE_LOCATION);
            }
#endif


            if (_combinedMaterial_shader) _combinedMaterial = new Material(_combinedMaterial_shader);

            Update_Material();
        }


        void OnDestroy()
        {
            cam.targetTexture = null;
            if (output_renderTexture != null) output_renderTexture.Release();

            if (activePipeline != Pipeline.BuiltIn && output_rawImage != null)
            {
                output_rawImage.texture = null;
                output_rawImage.material = null;
            }
        }
        #endregion



        #region Effects Logic
        // URP/HDRP logic
        public void Update_Effect()
        {
            Effect_Reloading(() =>
            {
                foreach (CameraAddOn addOn in addOns)
                {
                    addOn._UpdateRenderTexture();
                }
            });
        }

        public void Update_RenderTexture()
        {
            cam.targetTexture = output_renderTexture;

            if (activePipeline != Pipeline.BuiltIn && output_rawImage != null) output_rawImage.texture = output_renderTexture;
        }
        public void Update_Material()
        {
            if (activePipeline != Pipeline.BuiltIn && output_rawImage != null) output_rawImage.material = _combinedMaterial;
        }


        // BuiltIn logic
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            foreach (CameraAddOn addOn in addOns)
            {
                addOn._OnRenderImage(src, dest);
            }

            Update_Material();

            if (_combinedMaterial != null) Graphics.Blit(src, dest, _combinedMaterial);
            else Graphics.Blit(src, dest);
        }


        public void Effect_Reloading(Action middleExecution)
        {
            middleExecution?.Invoke();

            Update_RenderTexture();
            Update_Material();
        }
        #endregion



        #region External handling
        [ContextMenu("Reload All Effects")]
        public void RefreshEffects_MenuCommand()
        {
            Effect_Reloading(() =>
            {
                foreach (CameraAddOn addOn in addOns)
                {
                    addOn.RefreshEffect();
                }
            });
        }
        #endregion
    }
}
