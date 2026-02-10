using SHUU.Utils.PersistantInfo;
using SHUU.Utils.PersistantInfo.SavingLoading;
using TMPro;
using UnityEngine;

public class ChangeSaveFile : MonoBehaviour
{
    public TMP_Text saveFileText;




    public void ChangeSaveFileIndex()
    {
        Persistant_Globals.saveFilesManager.currentSaveFileIndex++;


        saveFileText.text = "" + (Persistant_Globals.saveFilesManager.currentSaveFileIndex + 1);
    }
}
