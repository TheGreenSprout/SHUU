/*using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SHUU.Utils.UI.Dialogue
{

    [Serializable]
    public class DialogueLineInstance
    {
        public GameObject characterPortrait = null;

        public TalkingSounds textSounds = null;


        public string characterName = "NullName";



        [SerializeReference] public DialogueLineVariables variables = null;



        private Action _endLineAction;
        public Action endLineAction
        {
            get => _endLineAction;
            set
            {
                endLineEvent = null;
                endLineEvent.AddListener(new UnityAction(value));

                _endLineAction = () => endLineEvent?.Invoke();
            }
        }

        [SerializeField] private UnityEvent endLineEvent;




        public DialogueLineInstance()
        {
            
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


            private Action _onClickAction;
            public Action onClickAction
            {
                get => _onClickAction;
                set
                {
                    onClickEvent = null;
                    onClickEvent.AddListener(new UnityAction(value));

                    _onClickAction = () => onClickEvent?.Invoke();
                }
            }

            [SerializeField] private UnityEvent onClickEvent;
        }
        #endregion
    }

}*/