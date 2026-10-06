using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public class LookAtObj : MonoBehaviour
    {
        #region Variables
        public Transform target;

        [Tooltip("Instead of looking at an object, the object looks in the direction it is moving. Target will not be used if this is true.")]
        [SerializeField] private bool lookAtMovementDirection = false;


        private Quaternion? cacheRotation;

        private Vector3 lastPosition;


        [SerializeField] private float rotationSpeed = 5f;


        [SerializeField] private bool lockX = false;
        [SerializeField] private bool lockY = false;
        [SerializeField] private bool lockZ = true;

        [SerializeField] private bool twoDimensions = false;


        [Tooltip("For when lookAtMovementDirection is true. Makes it so that if the object is still, the rotation won't reset.")]
        [SerializeField] private bool whileNotMoving = false;
        #endregion




        #region Main
        private void Awake()
        {
            cacheRotation = null;

            lastPosition = transform.position;
        }


        private void Update()
        {
            if (!lookAtMovementDirection) LookAtMovementDirection();
            else if (transform.position != lastPosition || whileNotMoving) Not_LookAtMovementDirection();
        }
        #endregion



        #region Logic
        private void LookAtMovementDirection()
        {
            if (target == null)
            {
                if (transform.rotation != cacheRotation && cacheRotation != null) RotateTowards((Quaternion)cacheRotation, false);
                else cacheRotation = null;

                return;
            }


            if (cacheRotation == null) cacheRotation = transform.rotation;


            Vector3 direction = target.position - transform.position;

            if (direction.magnitude == 0f) return;


            Quaternion targetRotation;
            if (!twoDimensions) targetRotation = Quaternion.LookRotation(direction.normalized, transform.up);
            else
            {
                Vector3 dir = direction.normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                targetRotation = Quaternion.Euler(0, 0, angle);
            }


            RotateTowards(targetRotation, false);
        }

        private void Not_LookAtMovementDirection()
        {
            Vector3 movement = transform.position - lastPosition;

            if (movement.magnitude == 0f) return;


            Quaternion targetRotation;
            if (!twoDimensions) targetRotation = Quaternion.LookRotation(movement.normalized, transform.up);
            else
            {
                Vector3 dir = movement.normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                targetRotation = Quaternion.Euler(0, 0, angle);
            }


            RotateTowards(targetRotation, true);

            lastPosition = transform.position;
        }


        private void RotateTowards(Quaternion goal, bool lerp)
        {
            float t = rotationSpeed * Time.deltaTime;

            if (!lockX && !lockY && !lockZ)
            {
                transform.rotation = lerp ? Quaternion.Lerp(transform.rotation, goal, t) : Quaternion.Slerp(transform.rotation, goal, t);

                return;
            }


            Vector3 current = transform.eulerAngles;
            Vector3 target = goal.eulerAngles;

            transform.rotation = Quaternion.Euler
            (
                lockX ? current.x : Mathf.LerpAngle(current.x, target.x, t),
                lockY ? current.y : Mathf.LerpAngle(current.y, target.y, t),
                lockZ ? current.z : Mathf.LerpAngle(current.z, target.z, t)
            );
        }
        #endregion



        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 position = transform.position;
            float size = SHUU_Gizmos.Size(position, 0.7f);

            SHUU_Gizmos.Line(position, position + transform.right * size, lockX ? SHUU_Gizmos.Red : SHUU_Gizmos.Green);
            SHUU_Gizmos.Line(position, position + transform.up * size, lockY ? SHUU_Gizmos.Red : SHUU_Gizmos.Green);
            SHUU_Gizmos.Line(position, position + transform.forward * size, lockZ ? SHUU_Gizmos.Red : SHUU_Gizmos.Green);

            Vector3 facing = twoDimensions ? transform.right : transform.forward;

            SHUU_Gizmos.Head(position + facing * size, facing, SHUU_Gizmos.Blue);

            if (lookAtMovementDirection || target == null) return;

            SHUU_Gizmos.Line(position, target.position, SHUU_Gizmos.Blue.WithAlpha(0.6f));
            SHUU_Gizmos.Sphere(target.position, SHUU_Gizmos.Size(target.position, 0.05f), SHUU_Gizmos.Blue);
        }
#endif
        #endregion
    }
}