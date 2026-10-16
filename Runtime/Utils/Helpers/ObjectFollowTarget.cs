using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public class ObjectFollowTarget : MonoBehaviour
    {
        #region Variables
        [Header("Control variables")]
        [SerializeField] private bool positionFollow = true;
        [SerializeField] private bool rotationFollow = true;


        public Transform followingObject;

        [Tooltip("If this variable is 0, the object will instantly follow the target.")]
        [SerializeField] private float followSmoothTime = 0.3f;

        [Tooltip("If this variable is 0, there won't be a minimum distance.")]
        [SerializeField] private float minDistance = 0f;
        [Tooltip("This controls whether the object is kept at the minimum distance when closer or not.")]
        [SerializeField] private bool snapToMinDistance = false;

        [Tooltip("If this variable is 0, there won't be a maximum distance.")]
        [SerializeField] private float maxDistance = 0f;



        [Header("Position Smoothing")]
        private Vector3 _posVelocity;


        [SerializeField] private float positionSmoothTime = 0.3f;


        [Header("Snap Buffers")]
        [SerializeField] private float positionBuffer = 0.01f;
        [SerializeField] private float rotationBuffer = 0.5f;
        #endregion




        #region Main
        private void LateUpdate()
        {
            if (followingObject == null) return;

            if (positionFollow) FollowPosition();
            if (rotationFollow) FollowRotation();
        }
        #endregion



        #region Logic
        private void FollowPosition()
        {
            if (followSmoothTime == 0f) followingObject.transform.position = transform.position;
            else
            {
                Vector3 toTarget = transform.position - followingObject.transform.position;
                float distance = toTarget.magnitude;

                Vector3 desiredPos = transform.position - toTarget.normalized * minDistance;

                if (maxDistance > 0f && distance > maxDistance) followingObject.transform.position = transform.position - toTarget.normalized * maxDistance;

                if (distance > minDistance || snapToMinDistance)
                {
                    float remainingDistance = Vector3.Distance(followingObject.transform.position, desiredPos);

                    if (remainingDistance <= positionBuffer) followingObject.transform.position = desiredPos;
                    else
                    {
                        followingObject.transform.position = Vector3.SmoothDamp(
                            followingObject.transform.position,
                            desiredPos,
                            ref _posVelocity,
                            positionSmoothTime
                        );
                    }
                }
            }
        }

        private void FollowRotation()
        {
            if (followSmoothTime == 0f) followingObject.transform.rotation = transform.rotation;
            else
            {
                float angleDifference = Quaternion.Angle(
                    followingObject.transform.rotation,
                    transform.rotation
                );

                if (rotationBuffer > 0f && angleDifference <= rotationBuffer) followingObject.transform.rotation = transform.rotation;
                else
                {
                    float t = 1f - Mathf.Exp(-Time.deltaTime / followSmoothTime);

                    followingObject.transform.rotation = Quaternion.Slerp(
                        followingObject.transform.rotation,
                        transform.rotation,
                        t
                    );
                }
            }
        }
        #endregion



        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (followingObject == null) return;

            Vector3 center = transform.position;
            Vector3 position = followingObject.transform.position;

            SHUU_Gizmos.Line(position, center, SHUU_Gizmos.Grey.WithAlpha(0.6f));

            if (!positionFollow) return;

            if (followSmoothTime == 0f)
            {
                SHUU_Gizmos.Cross(center, SHUU_Gizmos.Size(center, 0.1f), SHUU_Gizmos.Blue);

                return;
            }

            bool maxIsLarger = maxDistance >= minDistance;

            SHUU_Gizmos.Sphere(center, maxIsLarger ? maxDistance : minDistance, (maxIsLarger ? SHUU_Gizmos.Orange : SHUU_Gizmos.Blue).WithAlpha(0.5f));
            SHUU_Gizmos.Sphere(center, maxIsLarger ? minDistance : maxDistance, maxIsLarger ? SHUU_Gizmos.Blue : SHUU_Gizmos.Orange);

            Vector3 away = position - center;
            Vector3 rest = away.sqrMagnitude > 1e-8f ? center + away.normalized * minDistance : center;

            SHUU_Gizmos.Cross(rest, SHUU_Gizmos.Size(rest, 0.08f), SHUU_Gizmos.Blue);
        }
#endif
        #endregion
    }
}
