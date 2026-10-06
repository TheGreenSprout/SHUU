using UnityEngine;

using Alchemy.Inspector;

namespace SHUU.Utils.Helpers
{
    public class GradualRotation : MonoBehaviour
    {
        #region Variable
        [Header("Rotation")]
        [SerializeField] private Vector3 axis = Vector3.up;

        [SerializeField] private float speed = 90f;


        [Header("Reset")]
        [SerializeField] private Vector3 originalRotation;



        public void Reset()
        {
            axis = Vector3.up;
            speed = 90f;
        }
        #endregion




        #region Main
        private void Update() => transform.Rotate(axis, speed * Time.deltaTime, Space.Self);


        public void ResetRotation() => transform.localEulerAngles = originalRotation;
        #endregion



        #region Editor
#if UNITY_EDITOR
        [Button]
        private void FetchOriginalRotation() => originalRotation = transform.localEulerAngles;


        private void OnDrawGizmosSelected()
        {
            if (axis.sqrMagnitude < 1e-8f) return;

            Vector3 position = transform.position;
            Vector3 direction = transform.TransformDirection(axis.normalized);
            float size = SHUU_Gizmos.Size(position, 1f);

            SHUU_Gizmos.Line(position - direction * size, position + direction * size, SHUU_Gizmos.Blue);
            SHUU_Gizmos.Head(position + direction * size, direction, SHUU_Gizmos.Blue);

            if (Mathf.Approximately(speed, 0f)) return;

            Vector3 reference = Vector3.Cross(direction, Mathf.Abs(direction.y) > 0.95f ? Vector3.right : Vector3.up).normalized * size * 0.6f;

            SHUU_Gizmos.Arc(position + direction * size * 0.5f, direction, reference, 150f * Mathf.Sign(speed), SHUU_Gizmos.Orange);
        }
#endif
        #endregion
    }
}
