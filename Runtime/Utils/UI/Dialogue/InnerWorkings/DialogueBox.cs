using System;
using UnityEngine;

public class DialogueBox
{
    public GameObject dialogueBoxInstance;

    private DialogueManager dialogueManager;




    public DialogueBox(GameObject dialogueBoxPrefab, Transform dialogueBoxTransform)
    {
        dialogueBoxInstance = UnityEngine.Object.Instantiate(dialogueBoxPrefab, dialogueBoxTransform);

        dialogueManager = dialogueBoxInstance.GetComponent<DialogueManager>();
    }



    #region Dialogue Manager communication

    public void CreateDialogue(DialogueInstance dialogue, Action endDialogueLogic = null)
    {
        dialogueManager.CreateDialogue(dialogue, endDialogueLogic);
    }

    #endregion
}