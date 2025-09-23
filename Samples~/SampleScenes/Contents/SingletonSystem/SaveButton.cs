using SHUU.Utils;
using UnityEngine;

public class SaveButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_Globals.SaveSingletonInfo();
    }
}
