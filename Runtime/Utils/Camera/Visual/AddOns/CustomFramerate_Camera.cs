using SHUU.Utils.Cameras.Visual.Handlers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class CustomFramerate_Camera : MonoBehaviour
    {
        private void Awake()
        {
            CustomFramerate_Handler.camerasToRender.Add(this.GetComponent<Camera>());
        }
    }
}