using UnityEngine;

public class DialogueBox
{
    public GameObject dialogueBoxInstance;

    private DialogueManager dialogueManager;




    public DialogueBox(GameObject dialogueBoxPrefab, Transform dialogueBoxTransform)
    {
        dialogueBoxInstance = Object.Instantiate(dialogueBoxPrefab, dialogueBoxTransform);

        dialogueManager = dialogueBoxInstance.GetComponent<DialogueManager>();
    }



    #region Dialogue Manager communication

    public void CreateDialogue(DialogueInstance dialogue)
    {
        dialogueManager.CreateDialogue(dialogue);
    }

    #endregion
}