using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SHUU.Utils.Helpers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerToggle : MonoBehaviour
    {
        #region Variables
        [Header("Settings")]
        [SerializeField] private List<string> allowedTags = new List<string>();

        [SerializeField] private bool useTrigger = true;
        [SerializeField] private bool useCollision = false;



        [Header("Trigger Toggles")]
        [SerializeField] private UnityEvent onTriggerEnter;
        [SerializeField] private UnityEvent onTriggerExit;
        [SerializeField] private UnityEvent onColliderEnter;
        [SerializeField] private UnityEvent onColliderExit;
        #endregion




        #region Main
        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(other.tag)) return;

            onTriggerEnter?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTrigger || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(other.tag)) return;

            onTriggerExit?.Invoke();
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (!useCollision || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(collision.collider.tag)) return;

            onColliderEnter?.Invoke();
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!useCollision || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(collision.collider.tag)) return;

            onColliderExit?.Invoke();
        }
        #endregion
    }
}
