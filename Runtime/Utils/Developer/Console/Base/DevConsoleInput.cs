using System;
using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Developer.Console
{
    public class DevConsoleInput : MonoBehaviour
    {
        public static bool devConsole_On = false;
        
        public static bool canToggle_devConsole = true;



        [HideInInspector] public Action toggle;

        [HideInInspector] public Action previousCommand;
        [HideInInspector] public Action nextCommand;



        [SerializeField] private bool changeCursorVisivility = true;




        private void Awake() => devConsole_On = false;



        protected void Toggle()
        {
            if (!canToggle_devConsole) return;

            
            devConsole_On = !devConsole_On;

            toggle?.Invoke();


            if (!changeCursorVisivility) return;
            
            if (devConsole_On) ChangeMouseVisibility_Temporary(true);
            else ReturnMouseVisibility_FromTemporary();
        }


        protected void PreviousCommand() => previousCommand?.Invoke();

        protected void NextCommand() => nextCommand?.Invoke();
    }
}
