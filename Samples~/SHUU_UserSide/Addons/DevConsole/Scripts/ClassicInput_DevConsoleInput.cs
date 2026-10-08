using UnityEngine;

using Alchemy.Inspector;

using SHUU.Utils.Developer.Console;

namespace SHUU.UserSide.Addons.DevConsole
{
    public class ClassicInput_DevConsoleInput : DevConsoleInput_EasyUse
    {
        #region Variables
        [Title("Input")]
        [SerializeField] private KeyCode[] toggleKeys = new KeyCode[] { KeyCode.LeftControl, KeyCode.F2 };


        [SerializeField] private KeyCode previousCommandKey = KeyCode.UpArrow;

        [SerializeField] private KeyCode nextCommandKey = KeyCode.DownArrow;

        [SerializeField] private KeyCode autocompleteKey = KeyCode.Tab;
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


        protected override bool PreviousCommand_Key() => KeyDown(previousCommandKey);

        protected override bool NextCommand_Key() => KeyDown(nextCommandKey);

        protected override bool Autocomplete_Key() => KeyDown(autocompleteKey);
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
