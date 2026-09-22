using UnityEngine;

using Alchemy.Inspector;

using SHUU.Utils.Developer.Debugging.Systems;

namespace SHUU.UserSide.Developer.Debugging.DebugInfo
{
    public class ClassicInput_DebugInfoInput : DebugInfoInput_EasyUse
    {
        #region Variables
        [SerializeField, BoxGroup("Action Paths")] private KeyCode[] toggleKeys = new KeyCode[] { KeyCode.F3 };


        [SerializeField, BoxGroup("Action Paths/FPS Graph")] private KeyCode[] toggleFpsGraphKeys = new KeyCode[] { KeyCode.F3 };
        [SerializeField, BoxGroup("Action Paths/FPS Graph")] private KeyCode fpsGraphCornerKey = KeyCode.P;


        [SerializeField, BoxGroup("Action Paths/Axis")] private KeyCode toggleAxisKey = KeyCode.Keypad1;
        [SerializeField, BoxGroup("Action Paths/Axis")] private KeyCode axisModeKey = KeyCode.X;

        [SerializeField, BoxGroup("Action Paths/Compass")] private KeyCode toggleCompassKey = KeyCode.Keypad2;
        [SerializeField, BoxGroup("Action Paths/Compass")] private KeyCode compassModeKey = KeyCode.C;
        [SerializeField, BoxGroup("Action Paths/Compass")] private KeyCode compassPositionKey = KeyCode.V;
        #endregion




        #region Override points
        protected override bool Toggle_Key()
        {
            bool atLeast_aKey_pressed = false;
            bool keys_pressed = true;

            foreach (KeyCode key in toggleKeys)
            {
                if (Input.GetKeyDown(key)) atLeast_aKey_pressed = true;

                keys_pressed = keys_pressed && Input.GetKey(key);
            }


            return keys_pressed && atLeast_aKey_pressed;
        }


        protected override bool ToggleFpsGraph_Key()
        {
            bool atLeast_aKey_pressed = false;
            bool keys_pressed = true;

            foreach (KeyCode key in toggleFpsGraphKeys)
            {
                if (Input.GetKeyDown(key)) atLeast_aKey_pressed = true;

                keys_pressed = keys_pressed && Input.GetKey(key);
            }


            return keys_pressed && atLeast_aKey_pressed;
        }
        protected override bool FpsGraphCorner_Key() => Input.GetKeyDown(fpsGraphCornerKey);


        protected override bool ToggleAxis_Key() => Input.GetKeyDown(toggleAxisKey);
        protected override bool AxisMode_Key() => Input.GetKeyDown(axisModeKey);

        protected override bool ToggleCompass_Key() => Input.GetKeyDown(toggleCompassKey);
        protected override bool CompassMode_Key() => Input.GetKeyDown(compassModeKey);
        protected override bool CompassPosition_Key() => Input.GetKeyDown(compassPositionKey);
        #endregion
    }
}
