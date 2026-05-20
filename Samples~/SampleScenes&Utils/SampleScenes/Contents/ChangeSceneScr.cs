using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Samples.SampleScenesAndUtils
{
    public class ChangeSceneScr : MonoBehaviour
    {
        [SerializeField] private string sceneToGoTo;




        public void ChangeScene() => SHUU_General.GoToScene(sceneToGoTo);
    }
}
