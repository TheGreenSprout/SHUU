using UnityEngine;

using SHUU.Utils.Developer.Console;
using SHUU.Utils.InputSystem;

namespace SHUU.UserSide
{
    public class InputSystem_DevConsoleInput : DevConsoleInput
    {
        #region Variables
        [SerializeField] private string mapName = "Developer";


        [SerializeField] private string toggleBind = "Console_Toggle";
        private string toggle_actionPath => $"{mapName}/{toggleBind}";

        [SerializeField] private string previousCommandBind = "Console_PreviousCommand";
        private string previousCommand_actionPath => $"{mapName}/{previousCommandBind}";
        [SerializeField] private string nextCommandBind = "Console_NextCommand";
        private string nextCommand_actionPath => $"{mapName}/{nextCommandBind}";
        #endregion




        #region Main
        private void OnEnable()
        {
            SHUU_Input.EnableMap(mapName);

            SHUU_Input.RegisterListener_Down(toggle_actionPath, Toggle);
            SHUU_Input.RegisterListener_Down(previousCommand_actionPath, PreviousCommand);
            SHUU_Input.RegisterListener_Down(nextCommand_actionPath, NextCommand);
        }

        private void OnDisable()
        {
            SHUU_Input.UnregisterListener_Down(toggle_actionPath, Toggle);
            SHUU_Input.UnregisterListener_Down(previousCommand_actionPath, PreviousCommand);
            SHUU_Input.UnregisterListener_Down(nextCommand_actionPath, NextCommand);

            SHUU_Input.DisableMap(mapName);
        }
        #endregion
    }
}