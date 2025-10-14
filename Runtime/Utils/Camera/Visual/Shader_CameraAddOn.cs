using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SHUU.Utils.Cameras.Visual.AddOns
{
    [ExecuteInEditMode]
    public class Shader_CameraAddOn : CameraAddOn
    {
        [SerializeField] private bool enableEffect = true;




        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!instantiated) return;


            if (_proxy != null && _proxy._combinedMaterial) RemoveMaterialValues();
        }


        public override void URP_HDRP_Logic(bool internalCall = false)
        {
            if (_proxy._combinedMaterial) ChangeMaterialValues(internalCall);
        }

        public override void BuiltIn_Logic()
        {
            if (_proxy._combinedMaterial) ChangeMaterialValues();
        }


        protected virtual bool ChangeMaterialValues(bool internalCall = false)
        {
            return enableEffect;
        }
        protected virtual void RemoveMaterialValues() { }
    }
}