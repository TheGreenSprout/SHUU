using UnityEngine;

using SHUU.Utils.Developer.Console;

namespace SHUU.UserSide
{
    public class Basic_DevConsoleInput : DevConsoleInput_EasyUse
    {
        #region Variables
        [SerializeField] private KeyCode[] toggleKeys = new KeyCode[] { KeyCode.LeftControl, KeyCode.F2 };


        [SerializeField] private KeyCode previousCommandKey = KeyCode.UpArrow;

        [SerializeField] private KeyCode nextCommandKey = KeyCode.DownArrow;
        #endregion




        #region Logic
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


        protected override bool PreviousCommand_Key() => Input.GetKeyDown(previousCommandKey);

        protected override bool NextCommand_Key() => Input.GetKeyDown(nextCommandKey);
        #endregion
    }
}
