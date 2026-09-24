using UnityEngine;
using UnityEngine.Events;

using Alchemy.Inspector;

namespace SHUU.UserSide.Addons.CullingSystem
{
    public class CullableObject : MonoBehaviour, ICullable
    {
        #region Variables
        [SerializeField] private bool useDistanceCulling = true;
        [SerializeField, ShowIf(nameof(useDistanceCulling))] private float maxDistance = 50f;

        [SerializeField] private bool useFrustumCulling = true;


        [FoldoutGroup("Events")]
        [SerializeField] private UnityEvent<CullableObject> onCulled;
        [FoldoutGroup("Events")]
        [SerializeField] private UnityEvent<CullableObject> onUnculled;



        private bool isCulled;
        #endregion




        #region ICullable
        public Transform CullTransform => transform;

        public bool UseDistanceCulling => useDistanceCulling;
        public float MaxDistance => maxDistance;

        public bool UseFrustumCulling => useFrustumCulling;

        public bool IsCulled => isCulled;



        public void OnCulled()
        {
            isCulled = true;

            onCulled?.Invoke(this);
        }

        public void OnUnculled()
        {
            isCulled = false;

            onUnculled?.Invoke(this);
        }
        #endregion




        #region Main
        protected virtual void OnEnable() => SHUU_Culling.Instance?.Register(this);

        protected virtual void OnDisable() => SHUU_Culling.Instance?.Unregister(this);
        #endregion
    }
}
