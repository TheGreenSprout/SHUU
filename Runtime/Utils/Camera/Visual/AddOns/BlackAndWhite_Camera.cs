using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class BlackAndWhite_Camera : Shader_CameraAddOn
    {
        protected override bool ChangeMaterialValues(bool internalCall = false)
        {
            _proxy._combinedMaterial.SetFloat("_EnableBW", base.ChangeMaterialValues() ? 1f : 0f);


            return false;
        }

        protected override void RemoveMaterialValues()
        {
            _proxy._combinedMaterial.SetFloat("_EnableBW", 0f);
        }
    }
}
