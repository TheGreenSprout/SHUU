using UnityEngine;

namespace SHUU.Utils.Helpers
{
    
    public class ObjectFollow : MonoBehaviour
    {
        [Header("Control variables")]
        [SerializeField] private bool positionFollow = true;
        [SerializeField] private bool rotationFollow = true;


        [SerializeField] private Transform target;

        // how long (in seconds) it takes to get about 63% of the way there
        [Tooltip("If this variable is 0, the object will instantly follow the target.")]
        [SerializeField] private float followSmoothTime = 0.3f;

        [Tooltip("If this variable is 0, there won't be a minimum distance.")]
        [SerializeField] private float minDistance = 0f;
        [Tooltip("This controls whether the object is kept at the minimum distance when Closer or not.")]
        [SerializeField] private bool snapToMinDistance = false;

        [Tooltip("If this variable is 0, there won't be a maximum distance.")]
        [SerializeField] private float maxDistance = 0f;



        [Header("Position Smoothing")]
        private Vector3 _posVelocity;


        [SerializeField] private float positionSmoothTime = 0.3f;




        private void LateUpdate()
        {
            if (target == null) return;

            if (followSmoothTime == 0f)
            {
                if (positionFollow) transform.position = target.position;
            }
            else
            {
                if (positionFollow)
                {
                    Vector3 toTarget = target.position - transform.position;
                    float distance = toTarget.magnitude;

                    Vector3 desiredPos = target.position - toTarget.normalized * minDistance;

                    if (maxDistance > 0f && distance > maxDistance) transform.position = target.position - toTarget.normalized * maxDistance;

                    if (distance > minDistance || snapToMinDistance)
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVelocity, positionSmoothTime);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (target == null) return;

            if (followSmoothTime == 0f)
            {
                if (rotationFollow) transform.rotation = target.rotation;
            }
            else
            {
                float t = 1f - Mathf.Exp(-Time.deltaTime / followSmoothTime);


                if (rotationFollow) transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, t);
            }
        }
    }

}