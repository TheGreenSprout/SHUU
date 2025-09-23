using SHUU.Utils;
using UnityEngine;

public class SaveToFileButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_Globals.SaveSingletonInfo();
        SHUU_Globals.SaveSingletonInfoToFile();
    }
}
