using UnityEngine;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;

namespace SHUU.UserSide
{

    public class InputSystem_DevConsoleInput : DevConsoleInput
    {
        [SerializeField] private InputBindingMap map;

        [SerializeField] private string setName;




        private void Update()
        {
            if (SHUU_Input.GetInputDown(map, setName, true)) Toggle();
            
            if (SHUU_Input.GetInputDown(map, "PreviousCommand", true)) PreviousCommand();
            if (SHUU_Input.GetInputDown(map, "NextCommand", true)) NextCommand();
        }
    }
    
}
