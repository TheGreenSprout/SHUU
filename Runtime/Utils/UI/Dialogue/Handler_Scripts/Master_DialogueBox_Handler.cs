using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{
    [DisallowMultipleComponent]

    [RequireComponent(typeof(DialogueBox_TextHandler))]
    public class Master_DialogueBox_Handler : MonoBehaviour
    {
        // Internal
        private DialogueBox_TextHandler textHandler;




        private void Awake()
        {
            textHandler = GetComponent<DialogueBox_TextHandler>();
        }


        public bool IsCurrentlyInDialogue()
        {
            return textHandler.IsCurrentlyInDialogue();
        }


        public void StartDialogue(DialogueInstance dialogue, Action endDialogueLogic = null)
        {
            if (dialogue == null) return;



            dialogue.endDialogueAction += endDialogueLogic;


            StartDialoge_Call(dialogue);
        }

        private void StartDialoge_Call(DialogueInstance dialogue)
        {
            textHandler.StartDialogue(dialogue);
        }
    }
    
}
