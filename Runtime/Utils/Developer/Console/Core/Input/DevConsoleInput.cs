using System;
using UnityEngine;

using Alchemy.Inspector;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Developer.Console
{
    public abstract class DevConsoleInput : MonoBehaviour
    {
        #region Variables
        public static DevConsoleInput Instance;



        [Title("General")]
        [SerializeField] private DevConsole_Options options = null;

        public static bool DevConsole_On => Instance.options.devConsole_On;

        public static bool CanToggle_devConsole => Instance.options.canToggle_devConsole;



        [HideInInspector] public Action toggle;

        [HideInInspector] public Action previousCommand;
        [HideInInspector] public Action nextCommand;


        [SerializeField] private bool changeCursorVisivility = true;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (options != null) options.devConsole_On = false;
        }
        #endregion



        #region Logic
        protected void Toggle()
        {
            if (!CanToggle_devConsole) return;

            
            if (options != null) options.devConsole_On = !options.devConsole_On;

            toggle?.Invoke();


            if (!changeCursorVisivility) return;
            
            if (options != null)
            {
                if (options.devConsole_On) ChangeMouseVisibility_Temporary(true);
                else ReturnMouseVisibility_FromTemporary();
            }
        }


        protected void PreviousCommand() => previousCommand?.Invoke();

        protected void NextCommand() => nextCommand?.Invoke();
        #endregion
    }
}
