using SHUU.Utils.Globals;
using UnityEngine;

public class SaveButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_GlobalsProxy.savingSystemManager.SaveSingletonInfo();
    }
}
