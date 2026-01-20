using UnityEngine;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;

namespace SHUU.UserSide
{

    public class InputSystem_DevConsoleInput : DevConsoleInput
    {
        [SerializeField] private InputBindingMap map;

        [SerializeField] private string toggle_setName = "Toggle";

        [SerializeField] private string previousCommand_setName = "PreviousCommand";
        [SerializeField] private string nextCommand_setName = "NextCommand";




        private void OnEnable()
        {
            map.RegisterListener_Down(toggle_setName, Toggle, true);
            map.RegisterListener_Down(previousCommand_setName, PreviousCommand);
            map.RegisterListener_Down(nextCommand_setName, NextCommand);
        }


        private void OnDisable()
        {
            SHUU_Input.UnregisterListener_Down(Toggle);
            SHUU_Input.UnregisterListener_Down(PreviousCommand);
            SHUU_Input.UnregisterListener_Down(NextCommand);
        }
    }
    
}
