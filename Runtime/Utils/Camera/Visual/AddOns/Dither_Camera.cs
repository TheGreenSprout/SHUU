using System.IO;
using SHUU.Utils.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class Dither_Camera : Shader_CameraAddOn
    {
        [Header("Dither Settings")]
        [Min(1)] [SerializeField] private float ditherScale = 5.0f;


        private Texture2D ditherTexture;




        #region Setup
        protected override void Reset()
        {
            base.Reset();


            ditherTexture = HandyFunctions.FindFile<Texture2D>(new string[]
            {
                "Packages/com.sproutinggames.sprouts.huu/Runtime/Utils/Camera/Visual/Resources/Dither_Texture.png",
                "Assets/SHUU/Runtime/Utils/Camera/Visual/Resources/Dither_Texture.png"
            });
        }
        #endregion



        protected override bool ChangeMaterialValues(bool internalCall = false)
        {
            if (!ditherTexture) ditherTexture = HandyFunctions.FindFile<Texture2D>(new string[]
            {
                "Packages/com.sproutinggames.sprouts.huu/Runtime/Utils/Camera/Visual/Resources/Dither_Texture.png",
                "Assets/SHUU/Runtime/Utils/Camera/Visual/Resources/Dither_Texture.png"
            });


            _proxy._combinedMaterial.SetFloat("_EnableDither", base.ChangeMaterialValues() ? 1f : 0f);
            _proxy._combinedMaterial.SetFloat("_DitherScale", ditherScale);

            if (_proxy._combinedMaterial && ditherTexture) _proxy._combinedMaterial.SetTexture("_DitherTex", ditherTexture);

            
            return false;
        }

        protected override void RemoveMaterialValues()
        {
            _proxy._combinedMaterial.SetFloat("_EnableDither", 0f);
        }
    }
}
