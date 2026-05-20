using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Samples.SampleScenesAndUtils.SavingSystemAndInteraction
{
    public class LoadButton : MonoBehaviour
    {
        public void Press() => SHUU_Saving.LoadInfo();
    }
}
