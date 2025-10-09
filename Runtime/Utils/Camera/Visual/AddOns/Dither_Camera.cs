using System.IO;
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


#if UNITY_EDITOR
            string path = "Assets/SHUU/Runtime/Utils/Camera/Visual/Resources/Dither_Texture.png";

            if (!File.Exists(path)) return;
            else
            {
                ditherTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
#endif
        }
        #endregion



        protected override bool ChangeMaterialValues(bool internalCall = false)
        {
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
