using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueLineInstance
{
    public GameObject characterPortrait = null;

    public TalkingSounds textSounds = null;


    public string characterName = "NullName";



    [SerializeReference] public DialogueLineVariables variables = null;



    public Action endLineAction
    {
        get => endLineAction;
        set
        {
            endLineAction = value;
            endLineEvent = null;
            endLineEvent.AddListener(new UnityAction(value));
        }
    }

    [SerializeField]
    private UnityEvent endLineEvent
    {
        get => endLineEvent;
        set
        {
            endLineEvent = value;
            TransformEventsToActions();
        }
    }
    public UnityEvent Get_EndDialogueEvent() { return endLineEvent; }




    private void TransformEventsToActions()
    {
        if (variables is DialogueLine_Options options)
        {
            foreach (OptionButton option in options.optionList)
            {
                option.onClickAction = () => option.GetClickEvent()?.Invoke();
            }
        }
    }


    public DialogueLineInstance()
    {
        TransformEventsToActions();
    }

    public DialogueLineInstance(DialogueLineInstance other)
    {
        characterPortrait = other.characterPortrait;
        textSounds = other.textSounds;

        characterName = other.characterName;


        if (other.variables is DialogueLine_Text text)
        {
            variables = new DialogueLine_Text
            {
                line = text.line
            };
        }
        else if (other.variables is DialogueLine_Options options)
        {
            variables = new DialogueLine_Options
            {
                question = options.question,
                optionList = new List<OptionButton>(options.optionList)
            };

            TransformEventsToActions();
        }
    }
    



    #region Variable Classes
    public class DialogueLineVariables { }


    [Serializable]
    public class DialogueLine_Text : DialogueLineVariables
    {
        [TextArea] public string line = "";
    }

    [Serializable]
    public class DialogueLine_Options : DialogueLineVariables
    {
        public string question = "";
        public List<OptionButton> optionList = new List<OptionButton>();
    }

    
    [Serializable]
    public class OptionButton
    {
        public string optionText = "Option";
        public Action onClickAction = null;


        [SerializeField] private UnityEvent onClickEvent
        {
            get => onClickEvent;
            set
            {
                onClickEvent = value;
                TransformEventsToActions();
            }
        }
        public UnityEvent GetClickEvent() { return onClickEvent; }
        private void TransformEventsToActions() { onClickAction = () => GetClickEvent()?.Invoke(); }
    }
    #endregion
}
