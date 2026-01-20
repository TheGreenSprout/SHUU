using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SHUU.Utils.Helpers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerToggle : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private List<string> allowedTags = new List<string>();

        [SerializeField] private bool useTrigger = true;
        [SerializeField] private bool useCollision = false;



        [Header("Trigger Toggles")]
        [SerializeField] private GameObject[] onEnter_gameobjects;
        [SerializeField] private GameObject[] onExit_gameobjects;


        [SerializeField] private MonoBehaviour[] onEnter_behaviours;
        [SerializeField] private MonoBehaviour[] onExit_behaviours;


        public UnityEvent onEnter;
        public UnityEvent onExit;




        private void OnTriggerEnter(Collider other)
        {
            if (!useTrigger || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(other.tag)) return;


            foreach (var obj in onEnter_gameobjects) obj.SetActive(true);

            foreach (var obj in onExit_gameobjects) obj.SetActive(false);

            foreach (var scr in onEnter_behaviours) scr.enabled = true;

            foreach (var scr in onExit_behaviours) scr.enabled = false;


            onEnter?.Invoke();
            
        }

        private void OnTriggerExit(Collider other)
        {
            if (!useTrigger || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(other.tag)) return;


            foreach (var obj in onEnter_gameobjects) obj.SetActive(false);

            foreach (var obj in onExit_gameobjects) obj.SetActive(true);

            foreach (var scr in onEnter_behaviours) scr.enabled = false;

            foreach (var scr in onExit_behaviours) scr.enabled = true;


            onExit?.Invoke();
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (!useCollision || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(collision.collider.tag)) return;


            foreach (var obj in onEnter_gameobjects) obj.SetActive(true);

            foreach (var obj in onExit_gameobjects) obj.SetActive(false);

            foreach (var scr in onEnter_behaviours) scr.enabled = true;

            foreach (var scr in onExit_behaviours) scr.enabled = false;


            onEnter?.Invoke();
            
        }

        private void OnCollisionExit(Collision collision)
        {
            if (!useCollision || allowedTags == null || allowedTags.Count == 0 || !allowedTags.Contains(collision.collider.tag)) return;


            foreach (var obj in onEnter_gameobjects) obj.SetActive(false);

            foreach (var obj in onExit_gameobjects) obj.SetActive(true);

            foreach (var scr in onEnter_behaviours) scr.enabled = false;

            foreach (var scr in onExit_behaviours) scr.enabled = true;


            onExit?.Invoke();
        }
    }
}
