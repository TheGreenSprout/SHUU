using System;
using SHUU.UserSide;
using SHUU.Utils.Helpers;
using TMPro;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{
    public enum AutomaticSpacingStyles
    {
        None,
        Horizontal,
        Vertical,
        Grid_2Row,
        Grid_3Row
        
    }


    [DisallowMultipleComponent]

    [RequireComponent(typeof(DialogueBox_NameDisplayHandler))]
    [RequireComponent(typeof(DialogueBox_PortraitHandler))]
    [RequireComponent(typeof(DialogueBox_OptionsHandler))]
    public class DialogueBox_TextHandler : MonoBehaviour
    {
        // Internal
        private DialogueBox_NameDisplayHandler nameDisplayHandler;
        private DialogueBox_PortraitHandler portraitHandler;
        private DialogueBox_OptionsHandler optionsHandler;


        private CharacterPortrait_Reference currentPortrait;


        private DialogueInstance currentDialogue;


        private int currrentLineIndex = 0;

        private bool linePause = false;



        // External
        [SerializeField] private TypewriterText line_typewriterText;


        [SerializeField] private float startDialogueDelay = 0.25f;
        [SerializeField] private float startLineDelay = 0.25f;


        [Tooltip("Leave null if you don't want a default talking sounds")]
        [SerializeField] private TalkingSounds defaultTalkingSounds = null;


        [SerializeField] private bool allButtonsEndDialogueLine = true;




        private void Awake()
        {
            nameDisplayHandler = GetComponent<DialogueBox_NameDisplayHandler>();
            portraitHandler = GetComponent<DialogueBox_PortraitHandler>();
            optionsHandler = GetComponent<DialogueBox_OptionsHandler>();


            currrentLineIndex = 0;

            linePause = false;


            CustomInputManager.AddNextLinePressedCallback(NextLine);


            line_typewriterText.CompleteTextRevealed += EndLine;
            line_typewriterText.CompleteTextRevealed += optionsHandler.EndLine;
        }

        private void OnDestroy()
        {
            CustomInputManager.RemoveNextLinePressedCallback(NextLine);
            

            line_typewriterText.CompleteTextRevealed -= EndLine;
            line_typewriterText.CompleteTextRevealed -= optionsHandler.EndLine;
        }



        #region Dialogue logic
        public bool IsCurrentlyInDialogue()
        {
            return currentDialogue != null;
        }


        private void NextLine()
        {
            if (linePause && IsCurrentlyInDialogue())
            {
                EndLine();
            }
        }


        public void StartDialogue(DialogueInstance dialogue)
        {
            currentDialogue = dialogue;

            currentDialogue.endDialogueAction += portraitHandler.EndDialogue;
            currentDialogue.endDialogueAction += optionsHandler.EndDialogue;


            SHUU_Timer.CreateAt(this.transform, startDialogueDelay, DialogueStepLogic_Call);
        }
        private void DialogueStepLogic()
        {
            if (currrentLineIndex < currentDialogue.allDialogueLines.Count)
            {
                currentPortrait = portraitHandler.CharacterTalks(currentDialogue.allDialogueLines[currrentLineIndex].characterPortrait);

                SHUU_Timer.CreateAt(this.transform, startLineDelay, DialogueStepLogic_Call);
            }
        }
        private void DialogueStepLogic_Call()
        {
            DialogueLineInstance currentLine = currentDialogue.allDialogueLines[currrentLineIndex];

            WriteLine(currentLine);


            var sounds = defaultTalkingSounds;
            if (currentLine.textSounds != null)
            {
                sounds = currentLine.textSounds;
            }
            line_typewriterText.SetTextSounds(sounds);
            optionsHandler.SetTextSounds(sounds);
        }

        private void DialogueEndLogic()
        {
            currentDialogue.endDialogueAction?.Invoke();

            currentDialogue = null;
        }
        #endregion


        #region Line logic
        public void WriteLine(DialogueLineInstance line)
        {
            if (line.variables == null)
            {
                return;
            }



            DialogueLineInstance processedLine = new DialogueLineInstance(line);


            if (currentPortrait != null) if (currentPortrait != null) currentPortrait.StartTalking();
            nameDisplayHandler.DisplayName(processedLine.characterName);

            if (processedLine.variables is DialogueLineInstance.DialogueLine_Text text)
            {
                Text_Logic(text);
            }
            else if (processedLine.variables is DialogueLineInstance.DialogueLine_Options options)
            {
                if (allButtonsEndDialogueLine) optionsHandler.DrawOptions(options, EndLine);
                else optionsHandler.DrawOptions(options);
            }
        }

        private void Text_Logic(DialogueLineInstance.DialogueLine_Text variables)
        {
            if (string.IsNullOrEmpty(variables.line))
            {
                line_typewriterText._textBox.text = variables.line;
                line_typewriterText.StartTypewriterEffect();
            }
        }
        

        private void EndLine()
        {
            DialogueLineInstance line = currentDialogue.allDialogueLines[currrentLineIndex];
            line.endLineAction?.Invoke();

            if (!linePause)
            {
                if (currentPortrait != null) currentPortrait.StopTalking();


                linePause = true;

                line.endLineAction?.Invoke();


                currrentLineIndex++;

                return;
            }
            else
            {
                int lineAmmount = currentDialogue.allDialogueLines.Count;


                if (currentPortrait != null)
                {
                    if (currrentLineIndex + 1 < lineAmmount)
                    {
                        if (!currentDialogue.allDialogueLines[currrentLineIndex + 1].characterPortrait.GetComponent<CharacterPortrait_Reference>().IDENTIFIER().Equals(currentPortrait.IDENTIFIER()))
                        {
                            currentPortrait.EndLine();
                        }
                    }
                    else
                    {
                        currentPortrait.EndLine();
                    }
                }


                linePause = false;


                if (currrentLineIndex < lineAmmount)
                {
                    line_typewriterText.ClearText();


                    DialogueStepLogic();
                }
                else
                {
                    line_typewriterText._textBox.text = "";


                    currentDialogue = null;

                    currrentLineIndex = 0;


                    currentDialogue.endDialogueAction -= portraitHandler.EndDialogue;
                    currentDialogue.endDialogueAction -= optionsHandler.EndDialogue;


                    DialogueEndLogic();
                }
            }
        }
        #endregion
    }
    
}
