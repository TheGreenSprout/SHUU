using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    //[CreateAssetMenu(fileName = "DevConsole_Options", menuName = "SHUU/InnerWorkings/DevConsole/DevConsole_Options")]
    public class DevConsole_Options : ScriptableObject
    {
        [HideInInspector]
        public bool devConsole_On = false;

        public bool canToggle_devConsole = false;
    }
}
