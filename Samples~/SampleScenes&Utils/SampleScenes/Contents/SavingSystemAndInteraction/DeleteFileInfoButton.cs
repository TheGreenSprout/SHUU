using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Samples.SampleScenesAndUtils.SavingSystemAndInteraction
{
    public class DeleteFileInfoButton : MonoBehaviour
    {
        public void Press() => SHUU_Saving.DeleteSaveInfo();
    }
}
