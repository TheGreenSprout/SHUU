using SHUU.Utils.Globals;
using UnityEngine;

public class LoadFromFileButton : MonoBehaviour
{
    public void Press()
    {
        if (SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfoFromFile())
        {
            SHUU_GlobalsProxy.savingSystemManager.LoadSingletonInfo();
        }
    }
}
