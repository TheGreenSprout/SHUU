using SHUU.Utils.Cameras.Visual.Handlers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [RequireComponent(typeof(Camera))]
    public class RemoveFog_Camera : MonoBehaviour
    {
        private void Awake()
        {
            RemoveFog_Handler.camerasWithoutFog.Add(this.GetComponent<Camera>());
        }
    }
}
