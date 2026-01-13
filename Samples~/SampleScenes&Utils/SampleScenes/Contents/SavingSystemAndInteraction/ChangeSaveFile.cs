using SHUU.Utils.PersistantInfo.SavingLoading;
using TMPro;
using UnityEngine;

public class ChangeSaveFile : MonoBehaviour
{
    public TMP_Text saveFileText;




    public void ChangeSaveFileIndex()
    {
        SaveFilesManager.currentSaveFileIndex++;


        saveFileText.text = "" + (SaveFilesManager.currentSaveFileIndex + 1);
    }
}
