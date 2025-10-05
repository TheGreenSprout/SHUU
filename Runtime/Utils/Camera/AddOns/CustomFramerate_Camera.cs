using SHUU.Utils.Cameras.Managers;
using UnityEngine;

namespace SHUU.Utils.Cameras.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class CustomFramerate_Camera : MonoBehaviour
    {
        private void Awake()
        {
            CustomFramerate_Manager.camerasToRender.Add(this.GetComponent<Camera>());
        }
    }
}