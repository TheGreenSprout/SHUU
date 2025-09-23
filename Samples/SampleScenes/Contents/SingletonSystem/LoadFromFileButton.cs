using SHUU.Utils;
using UnityEngine;

public class LoadFromFileButton : MonoBehaviour
{
    public void Press()
    {
        if (SHUU_Globals.LoadSingletonInfoFromFile())
        {
            SHUU_Globals.LoadSingletonInfo();
        }
    }
}
