using UnityEngine;

using SHUU.Utils.Cameras.Visual.Handlers;
using SHUU.Utils.Globals;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class CustomFramerate_Camera : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string identifier = "Camera1";


        [SerializeField] private float refreshRate = 0.2f;



        private Camera cam;


        private SHUU_Timer timer;

        public bool? isPaused => timer?.isPaused;
        #endregion




        #region Main
        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.enabled = false;

            CustomFramerate_Handler.Instance?.Add(identifier, this);

            Render();
        }


        private void OnDestroy() => CustomFramerate_Handler.Instance?.Remove(identifier);
        #endregion



        #region Logic
        public void Render()
        {
            if (timer != null) timer.Cancel();
            timer = SHUU_Time.Timer(refreshRate, Render);

            cam.Render();
        }


        public void Pause() => timer.Pause();
        public void Resume() => timer.Resume();


        public void SetRefreshRate(float newRate) => refreshRate = Mathf.Max(0.01f, newRate);
        #endregion
    }
}