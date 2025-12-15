using System;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{

    public class ToggleDevConsole : MonoBehaviour
    {
        public bool devConsole_On = false;
        public bool canToggle_devConsole = false;


        [HideInInspector] public Action toggle;




        private void Awake()
        {
            devConsole_On = false;

            canToggle_devConsole = true;
        }


        protected void Toggle()
        {
            if (!canToggle_devConsole) return;

            
            devConsole_On = !devConsole_On;

            toggle?.Invoke();
        }
    }
    
}
