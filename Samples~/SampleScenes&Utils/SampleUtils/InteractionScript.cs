using UnityEngine;

using SHUU.Utils;

namespace SHUU.Samples.SampleScenesAndUtils.SampleUtils
{
    #region XML doc
    /// <summary>
    /// Example script of how to code basic interaction logic using SproutsHUU's interaction system.
    /// </summary>
    #endregion
    public class InteractionScript : Interactable
    {
        #region Logic
        public Material mat1;
        public Material mat2;


        private MeshRenderer meshRenderer;
        #endregion




        #region Logic
        protected override void Awake()
        {
            base.Awake();

            meshRenderer = GetComponent<MeshRenderer>();
        }
        #endregion



        #region Logic
        protected override void InteractLogic()
        {
            if (meshRenderer.sharedMaterial == mat1) meshRenderer.material = mat2;
            else meshRenderer.material = mat1;
        }


        public void SetMaterialFromBool(bool b)
        {
            if (b) meshRenderer.material = mat1;
            else meshRenderer.material = mat2;
        }

        public bool GetBooleanFromCurrentMaterial()
        {
            if (meshRenderer.sharedMaterial == mat1) return true;
            else return false;
        }
        #endregion
    }
}
