using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.Handlers
{
    public class CustomFramerate_Handler : MonoBehaviour
    {
        public static List<Camera> camerasToRender;

        public float refreshRate = 0.1f;

        private int currentIteration;




        private void Start()
        {
            currentIteration = -1;

            foreach (var cam in camerasToRender)
            {
                cam.enabled = false;
            }


            RenderCamera();
        }


        private void RenderCamera()
        {
            currentIteration++;

            if (currentIteration == camerasToRender.Count)
            {
                currentIteration = -1;

                Invoke(nameof(RenderCamera), refreshRate);

                return;
            }

            camerasToRender[currentIteration].Render();


            Invoke(nameof(RenderCamera), 0.01f);
        }
    }
}