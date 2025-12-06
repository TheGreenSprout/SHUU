using SHUU.Utils.UI.Dialogue;
using UnityEngine;

namespace SHUU
{
    public class Simple_DialogueInputAddon : DialogueInputAddon
    {
        public KeyCode nextLine_key = KeyCode.Space;
        public KeyCode fastForward_key = KeyCode.LeftShift;
        public KeyCode skip_key = KeyCode.Return;




        private void Update()
        {
            if (Input.GetKeyDown(nextLine_key))
            {
                NextLine_Press();
            }

            if (Input.GetKeyDown(fastForward_key))
            {
                FastForward_Press();
            }

            if (Input.GetKeyDown(skip_key))
            {
                Skip_Press();
            }
        }
    }
}
