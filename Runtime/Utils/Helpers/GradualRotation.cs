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

        [Button]
        private void FetchOriginalRotation() => originalRotation = transform.localEulerAngles;
        #endregion
    }
}
