using TMPro;
using UnityEngine;

public class ChangeLineInput : MonoBehaviour
{
    public TextToTypewriter textToTypewriter;
    public TMP_Text indexDisplay;


    public int currentIndexDisplay = 1;




    public void NextLine()
    {
        currentIndexDisplay++;

        textToTypewriter.ChangeIndex(1);

        indexDisplay.text = "" + currentIndexDisplay;
    }
    public void PreviousLine()
    {
        if (currentIndexDisplay <= 1)
        {
            currentIndexDisplay = 1;

            return;
        }



        currentIndexDisplay--;

        textToTypewriter.ChangeIndex(-1);

        indexDisplay.text = ""+currentIndexDisplay;
    }
}
