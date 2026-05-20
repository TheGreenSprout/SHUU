using TMPro;
using UnityEngine;

using SHUU.Utils.PersistantInfo.SavingLoading;

namespace SHUU.Samples.SampleScenesAndUtils.SavingSystemAndInteraction
{
    public class ChangeSaveFile : MonoBehaviour
    {
        [SerializeField] private TMP_Text saveFileText;




        public void ChangeSaveFileIndex()
        {
            SavingManager.instance.currentSaveFileIndex++;

            saveFileText.text = "" + (SavingManager.instance.currentSaveFileIndex + 1);
        }
    }
}
