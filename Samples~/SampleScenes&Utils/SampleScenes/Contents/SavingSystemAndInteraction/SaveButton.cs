using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Samples.SampleScenesAndUtils.SavingSystemAndInteraction
{
    public class SaveButton : MonoBehaviour
    {
        public void Press() => SHUU_Saving.SaveInfo();
    }
}
