using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLineInstance
{
    //public DialoguePortrait portrait;
    public List<DialoguePortrait> portraitList;

    public string characterName;
    [TextArea]
    public string line;

    public int talkingPortraitIndex = 0;


    public DialogueSounds textSounds = null;

    // {Float strength; Float speed}
    public float[] floatValues = new float[2] { 1f, 1f };
}
