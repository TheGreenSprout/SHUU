using SHUU.Utils;
using UnityEngine;

public class DeleteFileInfoButton : MonoBehaviour
{
    public void Press()
    {
        SHUU_Globals.DeleteSaveInfo();
    }
}
