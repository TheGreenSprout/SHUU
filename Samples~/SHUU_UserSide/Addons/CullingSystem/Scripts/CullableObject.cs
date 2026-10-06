using UnityEngine;
using UnityEngine.Events;

using Alchemy.Inspector;

using SHUU.Utils.Helpers;

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
        protected virtual void Awake() => SHUU_Culling.Instance?.Register(this);

        protected virtual void OnDestroy() => SHUU_Culling.Instance?.Unregister(this);
        #endregion



        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Camera cullingCamera = CullingCamera();
            Vector3 position = transform.position;

            Color state = SHUU_Gizmos.Blue;

            if (Application.isPlaying) state = isCulled ? SHUU_Gizmos.Red : SHUU_Gizmos.Green;
            else if (cullingCamera != null)
            {
                bool inRange = !useDistanceCulling || (position - cullingCamera.transform.position).sqrMagnitude <= maxDistance * maxDistance;

                state = inRange ? SHUU_Gizmos.Green : SHUU_Gizmos.Red;
            }

            if (useDistanceCulling) SHUU_Gizmos.Sphere(position, maxDistance, state.WithAlpha(0.6f));
            if (cullingCamera != null) SHUU_Gizmos.Line(position, cullingCamera.transform.position, state);
        }

        private static Camera CullingCamera()
        {
            if (Application.isPlaying && SHUU_Culling.Instance != null && SHUU_Culling.Instance.targetCamera != null) return SHUU_Culling.Instance.targetCamera;

            return Camera.main;
        }
#endif
        #endregion
    }
}
