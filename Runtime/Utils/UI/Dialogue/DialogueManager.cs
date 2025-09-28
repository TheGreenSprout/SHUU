using System;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueTextManager dialogueTextManager;


    private DialogueInstance dialogueRunning = null;




    public void CreateDialogue(DialogueInstance dialogue, Action endDialogueLogic = null)
    {
        dialogueRunning = dialogue;

        StartDialoge(endDialogueLogic);
    }

    private void StartDialoge(Action endDialogueLogic = null)
    {
        dialogueTextManager.StartDialogue(dialogueRunning, endDialogueLogic);
    }
}
