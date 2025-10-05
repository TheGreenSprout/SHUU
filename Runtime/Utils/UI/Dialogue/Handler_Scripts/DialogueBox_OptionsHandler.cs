using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{

    [DisallowMultipleComponent]
    public class DialogueBox_OptionsHandler : MonoBehaviour
    {
        // Internal
        private List<OptionButton_Reference> optionButtonList = new List<OptionButton_Reference>();


        private bool optionsDrawn = false;



        // External
        [SerializeField] private Transform optionSpawnParent = null;

        [SerializeField] private TypewriterText question_typewriterText;


        [SerializeField] private GameObject optionButtonPrefab;




        private void Awake()
        {
            optionsDrawn = false;


            question_typewriterText.CompleteTextRevealed += EndLine;
        }


        private void OnDestroy()
        {
            question_typewriterText.CompleteTextRevealed -= EndLine;
        }
    


        public void SetTextSounds(TalkingSounds sounds)
        {
            question_typewriterText.SetTextSounds(sounds);
        }


        public void DrawOptions(DialogueLineInstance.DialogueLine_Options variables, Action txtHandler_endLine = null)
        {
            if (optionsDrawn) return;
            optionsDrawn = true;



            if (string.IsNullOrEmpty(variables.question))
            {
                if (!question_typewriterText.gameObject.activeInHierarchy) question_typewriterText.gameObject.SetActive(true);

                question_typewriterText._textBox.text = variables.question;
                question_typewriterText.StartTypewriterEffect();

            }
            else
            {
                if (question_typewriterText.gameObject.activeInHierarchy) question_typewriterText.gameObject.SetActive(false);

                question_typewriterText._textBox.text = "";
            }



            foreach (DialogueLineInstance.OptionButton option in variables.optionList)
            {
                OptionButton_Reference button_Ref = Instantiate(optionButtonPrefab, optionSpawnParent).GetComponent<OptionButton_Reference>();

                Action onClick = option.onClickAction + txtHandler_endLine;


                foreach (OptionButton_Reference button in optionButtonList)
                {
                    button.ButtonWasAdded();
                }

                button_Ref.Appear(option.optionText, onClick, new Vector2Int(optionButtonList.Count+1, optionButtonList.Count+1));


                optionButtonList.Add(button_Ref);
            }
        }


        public void EndLine()
        {
            if (!optionsDrawn) return;
            optionsDrawn = false;



            optionButtonList.Clear();

            question_typewriterText.ClearText();
        }

        public void EndDialogue()
        {
            if (!optionsDrawn) return;
            optionsDrawn = false;


            
            optionButtonList.Clear();

            question_typewriterText.ClearText();
        }
    }

}
