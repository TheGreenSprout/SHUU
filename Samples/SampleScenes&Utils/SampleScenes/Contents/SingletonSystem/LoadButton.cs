using SHUU.Utils.Globals;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfo();
    }
}
