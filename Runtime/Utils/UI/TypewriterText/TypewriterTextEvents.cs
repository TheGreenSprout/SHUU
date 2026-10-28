using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Alchemy.Inspector;

namespace SHUU.Utils.UI
{
    [DisallowMultipleComponent]
    public class TypewriterTextEvents : MonoBehaviour
    {
        #region Variables
        [Title("Events")]
        [SerializeField] private UnityEvent<string> genericEvents;

        [Tooltip("Called with (id, argument) for <event=id(argument)> tags that have no labeled argument event.")]
        [SerializeField] private UnityEvent<string, string> genericArgumentEvents;


        [SerializeField] private List<LabeledEvent> labeledEvents = new();

        [SerializeField] private List<LabeledArgumentEvent> labeledArgumentEvents = new();
        #endregion




        #region Main
        private void OnValidate()
        {
            WarnRepeatedIds(labeledEvents, e => e.id, "Labeled Events");
            WarnRepeatedIds(labeledArgumentEvents, e => e.id, "Labeled Argument Events");
        }

        private void WarnRepeatedIds<T>(List<T> list, Func<T, string> getId, string listName)
        {
            if (list == null) return;

            var seen = new HashSet<string>();
            var repeated = new HashSet<string>();

            foreach (T entry in list)
            {
                string id = entry == null ? null : getId(entry);
                if (string.IsNullOrEmpty(id)) continue;

                if (!seen.Add(id)) repeated.Add(id);
            }

            if (repeated.Count > 0) Debug.LogWarning($"[TypewriterTextEvents] '{name}' has repeated ids in {listName}: {string.Join(", ", repeated)}. Only the first entry of each id will be called.", this);
        }
        #endregion



        #region Logic
        public void CallEvent(string id)
        {
            LabeledEvent labeled = labeledEvents.Find(e => e != null && e.id == id);

            if (labeled != null) labeled.onEvent?.Invoke();
            else genericEvents?.Invoke(id);
        }

        public void CallEvent(string id, string argument)
        {
            LabeledArgumentEvent labeled = labeledArgumentEvents.Find(e => e != null && e.id == id);

            if (labeled != null) labeled.onEvent?.Invoke(argument);
            else genericArgumentEvents?.Invoke(id, argument);
        }
        #endregion





        #region Data classes
        [Serializable]
        public class LabeledEvent
        {
            public string id;
            public UnityEvent onEvent;
        }


        [Serializable]
        public class LabeledArgumentEvent
        {
            public string id;
            public UnityEvent<string> onEvent;
        }
        #endregion
    }
}
