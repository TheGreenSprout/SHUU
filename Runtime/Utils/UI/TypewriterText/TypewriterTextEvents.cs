using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

using Alchemy.Serialization;
using System;

namespace SHUU.Utils.UI
{
    [DisallowMultipleComponent, AlchemySerialize]
    public partial class TypewriterTextEvents : MonoBehaviour
    {
        #region Variables
        [SerializeField] private UnityEvent<string> genericEvents;

        [AlchemySerializeField, NonSerialized] private Dictionary<string, UnityEvent> labeledEvents = new();
        #endregion




        #region Logic
        public void CallEvent(string id)
        {
            if (labeledEvents.ContainsKey(id)) labeledEvents[id]?.Invoke();
            else genericEvents?.Invoke(id);
        }
        #endregion
    }
}
