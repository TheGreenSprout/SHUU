using SHUU.Utils;
using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_Globals.LoadSingletonInfo();
    }
}
