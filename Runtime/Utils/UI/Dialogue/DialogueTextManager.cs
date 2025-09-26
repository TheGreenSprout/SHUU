using System.Collections.Generic;
using SHUU.Utils.Helpers;
using TMPro;
using UnityEngine;

public class DialogueTextManager : MonoBehaviour
{
    public TypewriterText typewriterText;

    public DialoguePortraitManager portraitManager;
    //public List<DialoguePortraitManager> portraitManagerList;

    public UIfloat portraitFloat;


    public TMP_Text lineNameBox;



    public float startTypewriterDelay = 0.25f;


    private bool currentlyInDialogue { get; set; }


    private bool linePause;

    private bool actionAdded;


    private int currrentLineIndex = 0;
    private DialogueInstance currentDialogue;



    public DialogueSounds defaultDialogueSounds;




    private void Awake()
    {
        //typewriterText = GetComponent<TypewriterText>();


        actionAdded = false;
        currentDialogue = null;

        currentlyInDialogue = false;
    }



    private void Update()
    {
        if (linePause && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            EndLine();
        }
    }



    public void StartDialogue(DialogueInstance dialogue)
    {
        /*foreach (DialogueLineInstance line in dialogue.allDialogueLines)
        {
            if (!HandyFunctions.IndexIsValid(line.talkingPortraitIndex, line.portraitList) || portraitManagerList.Count < line.portraitList.Count)
            {
                Debug.LogError("DialogueLineInstance talking portrait index out of bounds.");
                return;
            }
        }*/


        if (!actionAdded)
        {
            actionAdded = true;
            TypewriterText.CompleteTextRevealed += EndLine;
        }


        currentlyInDialogue = true;

        currentDialogue = dialogue;


        Invoke(nameof(DelayedDialogueStart), startTypewriterDelay);
    }

    private void DelayedDialogueStart()
    {
        int lineAmmount = currentDialogue.allDialogueLines.Count;


        if (currrentLineIndex < lineAmmount)
        {
            DialogueLineInstance currentLine = currentDialogue.allDialogueLines[currrentLineIndex];

            WriteLine(currentLine, currentLine.portrait, currentLine.talkingPortraitIndex);


            var sounds = defaultDialogueSounds;
            if (currentLine.textSounds != null)
            {
                sounds = currentLine.textSounds;
            }
            typewriterText.SetTextSounds(sounds);


            currrentLineIndex++;
        }
    }


    //private void WriteLine(DialogueLineInstance lineToWrite, List<DialoguePortrait> portraitList, int talkingPortraitIndex)
    private void WriteLine(DialogueLineInstance lineToWrite, DialoguePortrait portrait, int talkingPortraitIndex)
    {
        lineNameBox.text = lineToWrite.characterName;


        portraitFloat.floatStrength = lineToWrite.floatValues[0];
        portraitFloat.floatSpeed = lineToWrite.floatValues[1];

        //portraitManager.ChangeCurrentPortrait(portraitList[talkingPortraitIndex]);
        portraitManager.ChangeCurrentPortrait(portrait);
        portraitManager.StartTalking();

        typewriterText._textBox.text = lineToWrite.line;
        typewriterText.StartTypewriterEffect();
    }

    private void EndLine()
    {
        portraitFloat.floatStrength = 0f;
        portraitFloat.floatSpeed = 0f;

        if (!linePause)
        {
            //foreach (var portraitManager in portraitManagerList)
            //{
                portraitManager.StopTalking();
            //}

            linePause = true;
            return;
        }
        else
        {
            linePause = false;

            int lineAmmount = currentDialogue.allDialogueLines.Count;

            if (currrentLineIndex < lineAmmount)
            {
                DialogueLineInstance currentLine = currentDialogue.allDialogueLines[currrentLineIndex];

                WriteLine(currentLine, currentLine.portrait, currentLine.talkingPortraitIndex);

                currrentLineIndex++;
            }
            else
            {
                typewriterText._textBox.text = "";
                currentDialogue = null;
                currrentLineIndex = 0;


                DialogueEndLogic();
            }
        }
    }



    private void DialogueEndLogic()
    {
        currentlyInDialogue = false;
    }
}
