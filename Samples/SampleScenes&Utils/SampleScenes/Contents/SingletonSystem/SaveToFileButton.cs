using SHUU.Utils.Globals;
using UnityEngine;

public class SaveToFileButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfo();
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfoToFile();
    }
}
