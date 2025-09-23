using System.Collections.Generic;
using UnityEngine;



public class SetSamplePortraits : MonoBehaviour
{
    public TextToTypewriter buttonScr;
    public DialoguePortraitManager portraitManager;



    public enum SamplePortraitCharacters
    {
        Sprout = 0,
        Pebble = 3
    }
    public enum SamplePortraitEmotions
    {
        Happy,
        Meh,
        Angry
    }


    SamplePortraitCharacters character = SamplePortraitCharacters.Sprout;
    SamplePortraitEmotions emotion = SamplePortraitEmotions.Happy;



    public List<DialoguePortrait> allPortraits;




    public void SetPortrait(bool charactr, int index)
    {
        if (charactr)
        {
            index *= 3;
            character = (SamplePortraitCharacters)index;
        }
        else
        {
            emotion = (SamplePortraitEmotions)index;
        }


        portraitManager.ChangeCurrentPortrait(allPortraits[((int)character) + ((int)emotion)]);
        buttonScr.SetPortrait(allPortraits[((int)character) + ((int)emotion)]);
    }
}
