using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Samples.SampleScenesAndUtils.SavingSystemAndInteraction
{
    public class LoadFromFileButton : MonoBehaviour
    {
        public void Press() => SHUU_Saving.FullLoad();
    }
}
