using UnityEngine;

namespace SHUU.Utils.Helpers
{

    public class LookAtObj : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Instead of looking at an object, the object looks in the direction it is moving. Target will not be used if this is true.")]
        [SerializeField] private bool lookAtMovementDirection = false;


        private Quaternion? cacheRotation;

        private Vector3 lastPosition;


        [SerializeField] private float rotationSpeed = 5f;


        [SerializeField] private bool lockX = false;
        [SerializeField] private bool lockY = false;
        [SerializeField] private bool lockZ = true;

        [SerializeField] private bool twoDimensions = false;




        private void Awake()
        {
            cacheRotation = null;

            lastPosition = transform.position;
        }


        private void Update()
        {
            if (!lookAtMovementDirection)
            {
                if (target == null)
                {
                    if (transform.rotation != cacheRotation && cacheRotation != null)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion)cacheRotation, rotationSpeed * Time.deltaTime);
                    }
                    else
                    {
                        cacheRotation = null;
                    }


                    return;
                }


                if (cacheRotation == null)
                {
                    cacheRotation = this.transform.rotation;
                }


                Vector3 direction = target.position - transform.position;

                if (direction.magnitude == 0f) return;


                Quaternion targetRotation;
                if (!twoDimensions)
                {
                    targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
                }
                else
                {
                    Vector3 dir = direction.normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    targetRotation = Quaternion.Euler(0, 0, angle);
                }


                Vector3 euler = targetRotation.eulerAngles;
                Vector3 currentEuler = transform.rotation.eulerAngles;

                if (lockX) euler.x = currentEuler.x;
                if (lockY) euler.y = currentEuler.y;
                if (lockZ) euler.z = currentEuler.z;

                targetRotation = Quaternion.Euler(euler);


                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 movement = transform.position - lastPosition;

                if (movement.magnitude == 0f) return;


                Quaternion targetRotation;
                if (!twoDimensions)
                {
                    targetRotation = Quaternion.LookRotation(movement.normalized, transform.up);
                }
                else
                {
                    Vector3 dir = movement.normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    targetRotation = Quaternion.Euler(0, 0, angle);
                }


                Vector3 euler = targetRotation.eulerAngles;
                Vector3 currentEuler = transform.rotation.eulerAngles;

                if (lockX) euler.x = currentEuler.x;
                if (lockY) euler.y = currentEuler.y;
                if (lockZ) euler.z = currentEuler.z;

                targetRotation = Quaternion.Euler(euler);


                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                lastPosition = transform.position;
            }
        }
    }

}