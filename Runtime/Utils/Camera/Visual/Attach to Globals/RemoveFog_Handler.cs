using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.Handlers
{
    public class RemoveFog_Handler : MonoBehaviour
    {
        public static List<Camera> camerasWithoutFog;




        private void Start()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }


        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            bool cameraFogRemoved = false;

            foreach (var cam in camerasWithoutFog)
            {
                if (camera == cam){
                    cameraFogRemoved = true;
                    RenderSettings.fog = false;
                }
            }


            if (!cameraFogRemoved){
                RenderSettings.fog = true;
            }
        }
    }
}
