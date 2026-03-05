using System.Collections.Generic;
using SHUU.Utils.Cameras.Visual.AddOns;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.Handlers
{
    public class CustomFramerate_Handler : StaticInstance_Monobehaviour<CustomFramerate_Handler>
    {
        private Dictionary<string, CustomFramerate_Camera> trackedCameras = new();




        public void Add(string id, CustomFramerate_Camera instance) => trackedCameras.Add(id, instance);

        public void Remove(string id)
        {
            if (trackedCameras.ContainsKey(id)) trackedCameras.Remove(id);
        }


        public bool TryGet(string id, out CustomFramerate_Camera item) => trackedCameras.TryGetValue(id, out item);
    }
}