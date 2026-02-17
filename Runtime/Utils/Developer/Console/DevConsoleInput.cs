using System;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{

    
    public class DevConsoleInput : MonoBehaviour
    {
        public static bool devConsole_On = false;
        
        public static bool canToggle_devConsole = true;



        [HideInInspector] public Action toggle;

        [HideInInspector] public Action previousCommand;
        [HideInInspector] public Action nextCommand;




        private void Awake() => devConsole_On = false;


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
