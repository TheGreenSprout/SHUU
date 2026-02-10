using SHUU.Utils.Globals;
using UnityEngine;

public class ChangeSceneScr : MonoBehaviour
{
    public string sceneToGoTo;




    public void ChangeScene() => SHUU_General.GoToScene(sceneToGoTo);
}
