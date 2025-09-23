using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueInstance", menuName = "SHUU/Dialogue/DialogueInstance")]
public class DialogueInstance : ScriptableObject
{
    public List<DialogueLineInstance> allDialogueLines = new List<DialogueLineInstance>();
}