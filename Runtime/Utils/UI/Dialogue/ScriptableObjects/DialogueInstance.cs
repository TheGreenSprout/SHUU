using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace SHUU.Utils.UI.Dialogue
{

    [ExecuteInEditMode]
    [CreateAssetMenu(fileName = "DialogueInstance", menuName = "SHUU/Dialogue/DialogueInstance")]
    public class DialogueInstance : ScriptableObject
    {
        public List<DialogueLineInstance> allDialogueLines = new List<DialogueLineInstance>();


        [SerializeField] private Action _endDialogueAction;
        private Action _assign_endDialogueAction = null;
        public Action endDialogueAction
        {
            get => _endDialogueAction;
            set
            {
                _endDialogueAction = value;
                endDialogueEvent = null;
                endDialogueEvent.AddListener(new UnityAction(value));
            }
        }

        private UnityEvent _endDialogueEvent;
        private UnityEvent _assign_endDialogueEvent = null;
        [SerializeField] private UnityEvent endDialogueEvent
        {
            get => _endDialogueEvent;
            set
            {
                _endDialogueEvent = value;
                TransformEventsToActions();
            }
        }
        public UnityEvent Get_EndDialogueEvent() { return endDialogueEvent; }



#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_endDialogueAction != null && _endDialogueAction != _assign_endDialogueAction)
            {
                _assign_endDialogueAction = _endDialogueAction;
                
                endDialogueAction = _endDialogueAction;
            }
            if (_endDialogueEvent != null && _endDialogueEvent != _assign_endDialogueEvent)
            {
                _assign_endDialogueEvent = _endDialogueEvent;
                
                endDialogueEvent = _endDialogueEvent;
            }
        }
#endif

        private void TransformEventsToActions()
        {
            endDialogueAction = () => endDialogueEvent?.Invoke();
        }
    }

}