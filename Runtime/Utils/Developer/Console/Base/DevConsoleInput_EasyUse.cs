using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    public abstract class DevConsoleInput_EasyUse : DevConsoleInput
    {
        #region Main
        protected virtual void Update()
        {
            if (Toggle_Key()) Toggle();

            if (PreviousCommand_Key()) PreviousCommand();
            if (NextCommand_Key()) NextCommand();
        }
        #endregion



        #region Logic
        protected abstract bool Toggle_Key();


        protected abstract bool PreviousCommand_Key();

        protected abstract bool NextCommand_Key();
        #endregion
    }
}
