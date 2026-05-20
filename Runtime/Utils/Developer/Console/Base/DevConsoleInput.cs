using System;
using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Developer.Console
{
    public abstract class DevConsoleInput : MonoBehaviour
    {
        #region Variables
        public static bool devConsole_On = false;
        
        public static bool canToggle_devConsole = true;



        [HideInInspector] public Action toggle;

        [HideInInspector] public Action previousCommand;
        [HideInInspector] public Action nextCommand;


        [SerializeField] private bool changeCursorVisivility = true;
        #endregion




        #region Main
        protected virtual void Awake() => devConsole_On = false;
        #endregion



        #region Logic
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
        #endregion
    }
}
