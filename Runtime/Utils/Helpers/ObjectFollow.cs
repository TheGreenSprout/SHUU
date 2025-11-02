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
        [Tooltip("If this variable is 0, the object will instantly follow the target")]
        [SerializeField] private float followSmoothTime = 0.3f;

        [SerializeField] private float minimumDistance = 0f;
        [SerializeField] private bool alwaysStayAtMinimumDistance = false;



        [Header("Position Smoothing")]
        private Vector3 _posVelocity;


        [SerializeField] private float positionSmoothTime = 0.3f;




        void LateUpdate()
        {
            if (target == null) return;

            if (followSmoothTime == 0f)
            {
                if (positionFollow) transform.position = target.position;

                if (rotationFollow) transform.rotation = target.rotation;
            }
            else
            {
                float t = 1f - Mathf.Exp(-Time.deltaTime / followSmoothTime);


                if (positionFollow)
                {
                    Vector3 toTarget = target.position - transform.position;
                    float distance = toTarget.magnitude;

                    Vector3 desiredPos = target.position - toTarget.normalized * minimumDistance;

                    if (distance > minimumDistance || alwaysStayAtMinimumDistance)
                    {
                        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVelocity, positionSmoothTime);
                    }
                }

                if (rotationFollow) transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, t);
            }
        }
    }

}