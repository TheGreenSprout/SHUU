using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueTextManager dialogueTextManager;


    private DialogueInstance dialogueRunning = null;




    public void CreateDialogue(DialogueInstance dialogue)
    {
        dialogueRunning = dialogue;

        StartDialoge();
    }

    private void StartDialoge()
    {
        dialogueTextManager.StartDialogue(dialogueRunning);
    }
}
