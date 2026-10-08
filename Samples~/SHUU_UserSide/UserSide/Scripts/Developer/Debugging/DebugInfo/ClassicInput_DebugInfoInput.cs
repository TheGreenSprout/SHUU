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
                if (KeyDown(key)) atLeast_aKey_pressed = true;

                keys_pressed = keys_pressed && KeyHeld(key);
            }


            return keys_pressed && atLeast_aKey_pressed;
        }


        protected override bool ToggleFpsGraph_Key()
        {
            bool atLeast_aKey_pressed = false;
            bool keys_pressed = true;

            foreach (KeyCode key in toggleFpsGraphKeys)
            {
                if (KeyDown(key)) atLeast_aKey_pressed = true;

                keys_pressed = keys_pressed && KeyHeld(key);
            }


            return keys_pressed && atLeast_aKey_pressed;
        }
        protected override bool FpsGraphCorner_Key() => KeyDown(fpsGraphCornerKey);


        protected override bool ToggleAxis_Key() => KeyDown(toggleAxisKey);
        protected override bool AxisMode_Key() => KeyDown(axisModeKey);

        protected override bool ToggleCompass_Key() => KeyDown(toggleCompassKey);
        protected override bool CompassMode_Key() => KeyDown(compassModeKey);
        protected override bool CompassPosition_Key() => KeyDown(compassPositionKey);
        #endregion



        #region Input
        private static bool KeyDown(KeyCode key)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

        private static bool KeyHeld(KeyCode key)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }
        #endregion
    }
}
