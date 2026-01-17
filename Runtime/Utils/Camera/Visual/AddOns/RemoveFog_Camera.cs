using SHUU.Utils.Cameras.Visual.Handlers;
using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class RemoveFog_Camera : MonoBehaviour
    {
        private Camera cam;




        private void Awake() => cam = GetComponent<Camera>();


        private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;



        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == cam) RenderSettings.fog = false;
            else RenderSettings.fog = true;
        }
    }
}
