using SHUU.Utils;
using UnityEngine;

public class ChangeSceneScr : MonoBehaviour
{
    public string sceneToGoTo;




    public void ChangeScene()
    {
        SHUU_Globals.GoToScene(sceneToGoTo);
    }
}
