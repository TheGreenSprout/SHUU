using SHUU.Utils.PersistantInfo.SavingLoading;
using TMPro;
using UnityEngine;

public class ChangeSaveFile : MonoBehaviour
{
    public TMP_Text saveFileText;




    public void ChangeSaveFileIndex()
    {
        int newIndex = SaveFilesManager.currentSaveFileIndex;


        newIndex++;

        if (newIndex >= 3)
        {
            newIndex = 0;
        }


        SaveFilesManager.currentSaveFileIndex = newIndex;



        saveFileText.text = "" + (newIndex + 1);
    }
}
