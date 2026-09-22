using UnityEngine;

using Alchemy.Inspector;

using SHUU.Utils.Developer.Debugging.Systems;
using SHUU.Utils.InputSystem;

namespace SHUU.UserSide.Developer.Debugging.DebugInfo
{
    public class InputSystem_DebugInfoInput : DebugInfoInput
    {
        #region Variables
        [SerializeField, BoxGroup("Action Paths")] private string toggleInfo_ActionPath = "Developer/Info_Toggle";


        [SerializeField, BoxGroup("Action Paths/FPS Graph")] private string toggleFpsGraph_ActionPath = "Developer/Info_ToggleFpsGraph";
        [SerializeField, BoxGroup("Action Paths/FPS Graph")] private string fpsGraphCorner_ActionPath = "Developer/Info_FpsGraphCorner";


        [SerializeField, BoxGroup("Action Paths/Axis")] private string toggleAxis_ActionPath = "Developer/Info_ToggleAxis";
        [SerializeField, BoxGroup("Action Paths/Axis")] private string axisMode_ActionPath = "Developer/Info_AxisMode";

        [SerializeField, BoxGroup("Action Paths/Compass")] private string toggleCompass_ActionPath = "Developer/Info_ToggleCompass";
        [SerializeField, BoxGroup("Action Paths/Compass")] private string compassMode_ActionPath = "Developer/Info_CompassMode";
        [SerializeField, BoxGroup("Action Paths/Compass")] private string compassPosition_ActionPath = "Developer/Info_CompassPosition";
        #endregion




        #region Main
        private void OnEnable()
        {
            SHUU_Input.RegisterListener_Down(toggleInfo_ActionPath, Toggle);
            
            
            SHUU_Input.RegisterListener_Down(toggleFpsGraph_ActionPath, ToggleFpsGraph);
            SHUU_Input.RegisterListener_Down(fpsGraphCorner_ActionPath, FpsGraphCorner);

            
            SHUU_Input.RegisterListener_Down(toggleAxis_ActionPath, ToggleAxis);
            SHUU_Input.RegisterListener_Down(axisMode_ActionPath, AxisMode);

            SHUU_Input.RegisterListener_Down(toggleCompass_ActionPath, ToggleCompass);
            SHUU_Input.RegisterListener_Down(compassMode_ActionPath, CompassMode);
            SHUU_Input.RegisterListener_Down(compassPosition_ActionPath, CompassPosition);
        }

        private void OnDisable()
        {
            SHUU_Input.UnregisterListener_Down(toggleInfo_ActionPath, Toggle);
            
            
            SHUU_Input.UnregisterListener_Down(toggleFpsGraph_ActionPath, ToggleFpsGraph);
            SHUU_Input.UnregisterListener_Down(fpsGraphCorner_ActionPath, FpsGraphCorner);


            SHUU_Input.UnregisterListener_Down(toggleAxis_ActionPath, ToggleAxis);
            SHUU_Input.UnregisterListener_Down(axisMode_ActionPath, AxisMode);

            SHUU_Input.UnregisterListener_Down(toggleCompass_ActionPath, ToggleCompass);
            SHUU_Input.UnregisterListener_Down(compassMode_ActionPath, CompassMode);
            SHUU_Input.UnregisterListener_Down(compassPosition_ActionPath, CompassPosition);
        }
        #endregion
    }
}
