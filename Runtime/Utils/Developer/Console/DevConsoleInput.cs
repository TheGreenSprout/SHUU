using System;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{

    
    public class DevConsoleInput : MonoBehaviour
    {
        public bool devConsole_On = false;
        public bool canToggle_devConsole = false;


        [HideInInspector] public Action toggle;

        [HideInInspector] public Action previousCommand;
        [HideInInspector] public Action nextCommand;




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

        protected void PreviousCommand()
        {
            previousCommand?.Invoke();
        }
        protected void NextCommand()
        {
            nextCommand?.Invoke();
        }
    }
    
}
