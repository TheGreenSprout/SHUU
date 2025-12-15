using UnityEngine;
using SHUU.Utils.Developer.Console;

namespace SHUU.UserSide
{

    public class Basic_ToggleDevConsole : ToggleDevConsole
    {
        [SerializeField] private KeyCode[] toggleKeys = new KeyCode[] { KeyCode.LeftControl, KeyCode.F2 };




        private void Update()
        {
            bool atLeast_aKey_pressed = false;
            bool keys_pressed = true;

            foreach (KeyCode key in toggleKeys)
            {
                if (Input.GetKeyDown(key)) atLeast_aKey_pressed = true;

                keys_pressed = keys_pressed && Input.GetKey(key);
            }


            if (keys_pressed && atLeast_aKey_pressed) Toggle();
        }
    }
    
}
