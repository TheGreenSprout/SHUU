using System.Collections.Generic;

using SHUU.Utils.Cameras.Visual.AddOns;
using SHUU.Utils.Helpers;

namespace SHUU.Utils.Cameras.Visual.Handlers
{
    public class CustomFramerate_Handler : Singleton_MonoBehaviour<CustomFramerate_Handler>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        private Dictionary<string, CustomFramerate_Camera> trackedCameras = new();
        #endregion




        #region Logic
        public void Add(string id, CustomFramerate_Camera instance) => trackedCameras.Add(id, instance);

        public void Remove(string id)
        {
            if (trackedCameras.ContainsKey(id)) trackedCameras.Remove(id);
        }


        public bool TryGet(string id, out CustomFramerate_Camera item) => trackedCameras.TryGetValue(id, out item);
        #endregion
    }
}