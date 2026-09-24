using UnityEngine;

namespace SHUU.UserSide.Addons.CullingSystem
{
    public interface ICullable
    {
        #region Variables
        public Transform CullTransform { get; }

        public bool UseDistanceCulling { get; }
        public float MaxDistance { get; }

        public bool UseFrustumCulling { get; }


        public bool IsCulled { get; }
        #endregion




        #region Override points
        public void OnCulled();

        public void OnUnculled();
        #endregion
    }
}
