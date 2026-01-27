/*using System;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{

    public class SHUU_DialogueBox
    {
        [SerializeField] private GameObject dialogueBoxInstance;

        private Master_DialogueBox_Handler dialogueBoxHandler;



        public bool currentlyInDialogue
        {
            get => dialogueBoxHandler.IsCurrentlyInDialogue();
        }




        public SHUU_DialogueBox(GameObject dialogueBoxPrefab, Transform dialogueBoxTransform)
        {
            dialogueBoxInstance = MonoBehaviour.Instantiate(dialogueBoxPrefab, dialogueBoxTransform);

            dialogueBoxHandler = dialogueBoxInstance.GetComponent<Master_DialogueBox_Handler>();
        }



        #region Dialogue Handler communication

        public void StartDialogue(DialogueInstance dialogue, Action endDialogueLogic = null)
        {
            if (currentlyInDialogue) return;


            
            dialogueBoxHandler.StartDialogue(dialogue, endDialogueLogic);
        }

        public void DeleteDialogue()
        {
            MonoBehaviour.Destroy(dialogueBoxInstance);

            dialogueBoxInstance = null;
        }

        #endregion
    }

}*/