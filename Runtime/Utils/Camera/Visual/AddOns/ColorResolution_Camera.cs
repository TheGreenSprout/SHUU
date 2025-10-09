using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class ColorResolution_Camera : Shader_CameraAddOn
    {
        [Header("Color Resolution Settings")]
        [Min(2)] [SerializeField] private float colorResolution = 8;




        protected override bool ChangeMaterialValues(bool internalCall = false)
        {
            _proxy._combinedMaterial.SetFloat("_EnableColorRes", base.ChangeMaterialValues() ? 1f : 0f);
            _proxy._combinedMaterial.SetFloat("_ColorResolution", colorResolution);

            
            return false;
        }

        protected override void RemoveMaterialValues()
        {
            _proxy._combinedMaterial.SetFloat("_EnableColorRes", 0f);
        }
    }
}
