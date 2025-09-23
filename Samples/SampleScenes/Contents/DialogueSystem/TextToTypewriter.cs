using TMPro;
using UnityEngine;
using System.Collections.Generic;
using SHUU.Utils.Helpers;


public class TextToTypewriter : MonoBehaviour
{
    private class DialogueLineData
    {
        public string characterName;
        public string line;


        public DialogueSounds textSounds = null;

        // {Float strength; Float speed}
        public float[] floatValues = new float[2] { 1f, 1f };
    }


    public DialogueTextManager dialogueTextManager;
    public DialoguePortraitManager dialoguePortraitManager;



    public TMP_InputField inputtedText;

    public TMP_InputField inputName;


    public TMP_InputField floatStrength;
    public TMP_InputField floatSpeed;


    public DialoguePortrait defaultPortrait;
    //public DialogueLineInstance defaultLine;
    private List<DialoguePortrait> portraits = new List<DialoguePortrait>();


    private int index = 0;

    //public DialogueInstance dialogueInstance;
    private List<DialogueLineData> dialogueLines = new List<DialogueLineData>();




    void Start()
    {
        ChangeIndex(0);
    }



    public void TypeWriterText()
    {
        GetCurrentInfo();


        DialogueInstance dialogueInstance = ScriptableObject.CreateInstance<DialogueInstance>();

        for (int i = 0; i < dialogueLines.Count; i++)
        {
            if (dialogueLines[i] != null)
            {
                DialogueLineInstance line = new DialogueLineInstance();
                line.portraitList[i] = portraits[i];
                line.characterName = dialogueLines[i].characterName;
                line.line = dialogueLines[i].line;

                line.floatValues = (float[])dialogueLines[i].floatValues.Clone();

                dialogueInstance.allDialogueLines.Add(line);
            }
        }


        dialogueTextManager.StartDialogue(dialogueInstance);
    }

    private void GetCurrentInfo()
    {
        if (!HandyFunctions.IndexIsValid(index, dialogueLines))
        {
            return;
        }
        if (!HandyFunctions.IndexIsValid(index, portraits))
        {
            dialogueLines[index] = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(inputtedText.text))
        {
            dialogueLines[index] = null;
            return;
        }

        // Try to parse float values
        if (!float.TryParse(floatStrength.text, out float strength))
        {
            //Debug.LogWarning("Invalid floatStrength input.");


            strength = 0f;
        }

        if (!float.TryParse(floatSpeed.text, out float speed))
        {
            //Debug.LogWarning("Invalid floatSpeed input.");


            speed = 0f;
        }

        dialogueLines[index] = new DialogueLineData();



        dialogueLines[index].characterName = inputName.text;
        dialogueLines[index].line = inputtedText.text;

        dialogueLines[index].floatValues = new float[] { strength, speed };
    }
    private void LoadInputFields()
    {
        dialoguePortraitManager.ChangeCurrentPortrait(portraits[index]);


        inputName.text = dialogueLines[index].characterName;
        inputtedText.text = dialogueLines[index].line;

        if (dialogueLines[index].floatValues[0] == 0f)
        {
            floatStrength.text = "";
        }
        else
        {
            floatStrength.text = "" + dialogueLines[index].floatValues[0];
        }

        if (dialogueLines[index].floatValues[1] == 0f)
        {
            floatSpeed.text = "";
        }
        else
        {
            floatSpeed.text = "" + dialogueLines[index].floatValues[1];
        }
    }
    private void ClearInputFields()
    {
        inputName.text = "";
        inputtedText.text = "";

        floatStrength.text = "";
        floatSpeed.text = "";
    }

    public void ChangeIndex(int dir)
    {
        if ((index + dir) < 0)
        {
            return;
        }
        else if (dir == 0)
        {
            while ((dialogueLines.Count - 1) < index || dialogueLines == null)
            {
                dialogueLines.Add(null);
            }
            while ((portraits.Count - 1) < index || portraits == null)
            {
                portraits.Add(defaultPortrait);
            }

            return;
        }

        GetCurrentInfo();
        ClearInputFields();

        index += dir;

        if (HandyFunctions.IndexIsValidAndNotNull(index, dialogueLines))
        {
            LoadInputFields();
        }

        while ((dialogueLines.Count - 1) < index || dialogueLines == null)
        {
            dialogueLines.Add(null);
        }
        while ((portraits.Count - 1) < index || portraits == null)
        {
            portraits.Add(defaultPortrait);
        }
    }


    public void SetPortrait(DialoguePortrait p)
    {
        portraits[index] = p;
    }
}
