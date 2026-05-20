using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class RemoveFog_Camera : MonoBehaviour
    {
        #region Variables
        private Camera cam;
        #endregion




        #region Main
        private void Awake() => cam = GetComponent<Camera>();


        private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        #endregion



        #region Logic
        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == cam) RenderSettings.fog = false;
            else RenderSettings.fog = true;
        }
        #endregion
    }
}
