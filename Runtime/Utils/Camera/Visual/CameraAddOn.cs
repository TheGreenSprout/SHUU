using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode, RequireComponent(typeof(CameraAddOns_Proxy))]
    public class CameraAddOn : MonoBehaviour
    {
        protected bool instantiated = false;


        public CameraAddOns_Proxy _proxy;


        protected CameraAddOns_Proxy.Pipeline activePipeline
        {
            get => _proxy.activePipeline;
        }




        #region Setup
        protected virtual void Reset()
        {
            if (_proxy == null)
            {
                _proxy = GetComponent<CameraAddOns_Proxy>();

                _proxy.RegisterAddOn(this);


                instantiated = true;
            }
        }


        protected virtual void OnDestroy()
        {
            if (!instantiated) return;


            if (_proxy != null) _proxy.RemoveAddOn(this);
        }
        #endregion



        #region Effect Logic
        public void _UpdateRenderTexture(bool internalCall = false)
        {
            if (!this.enabled) return;


            if (_proxy != null && activePipeline == CameraAddOns_Proxy.Pipeline.BuiltIn) return;
            

            if (_proxy != null) URP_HDRP_Logic(internalCall);
        }
        public void _OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!this.enabled) return;


            if (_proxy != null && activePipeline != CameraAddOns_Proxy.Pipeline.BuiltIn)
            {
                Graphics.Blit(src, dest);

                return;
            }


            if (_proxy != null) BuiltIn_Logic();
        }


        public virtual void URP_HDRP_Logic(bool internalCall = false) { }

        public virtual void BuiltIn_Logic() { }
        #endregion



        #region External Handling
        [ContextMenu("Reload Effect")]
        public void RefreshEffect_MenuCommand()
        {
            if (!this.enabled) return;


            if (_proxy != null) RefreshEffect(true);
        }


        public virtual void RefreshEffect(bool internalCall = false)
        {
            _UpdateRenderTexture(internalCall);
        }
        #endregion
    }
}
