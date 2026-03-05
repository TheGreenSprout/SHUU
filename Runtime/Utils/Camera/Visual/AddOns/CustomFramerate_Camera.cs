using SHUU.Utils.Cameras.Visual.Handlers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class CustomFramerate_Camera : MonoBehaviour
    {
        [SerializeField] private string identifier = "Camera1";



        [SerializeField] private float refreshRate = 0.2f;



        private Camera cam;


        private float timer;

        private bool paused;
        public bool isPaused => paused;




        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.enabled = false;

            CustomFramerate_Handler.instance?.Add(identifier, this);
        }

        private void OnDestroy() => CustomFramerate_Handler.instance?.Remove(identifier);



        private void Update()
        {
            if (paused) return;


            timer += Time.deltaTime;

            if (timer >= refreshRate)
            {
                timer = 0f;

                cam.Render();
            }
        }


        public void Pause() => paused = true;
        public void Resume() => paused = false;


        public void SetRefreshRate(float newRate) => refreshRate = Mathf.Max(0.01f, newRate);

        public void ForceRender()
        {
            cam.Render();

            timer = 0f;
        }
    }
}