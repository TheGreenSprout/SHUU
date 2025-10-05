using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

[CreateAssetMenu(fileName = "DialogueInstance", menuName = "SHUU/Dialogue/DialogueInstance")]
public class DialogueInstance : ScriptableObject
{
    public List<DialogueLineInstance> allDialogueLines = new List<DialogueLineInstance>();


    public Action endDialogueAction
    {
        get => endDialogueAction;
        set
        {
            endDialogueAction = value;
            endDialogueEvent = null;
            endDialogueEvent.AddListener(new UnityAction(value));
        }
    }

    [SerializeField] private UnityEvent endDialogueEvent
    {
        get => endDialogueEvent;
        set
        {
            endDialogueEvent = value;
            TransformEventsToActions();
        }
    }
    public UnityEvent Get_EndDialogueEvent() { return endDialogueEvent; }




    private void OnValidate()
    {
        TransformEventsToActions();
    }

    private void TransformEventsToActions()
    {
        endDialogueAction = () => endDialogueEvent?.Invoke();
    }
}