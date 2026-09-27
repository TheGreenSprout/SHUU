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

            if (Autocomplete_Key()) Autocomplete();
        }
        #endregion



        #region Logic
        protected abstract bool Toggle_Key();


        protected abstract bool PreviousCommand_Key();

        protected abstract bool NextCommand_Key();

        // Not abstract, so an input class written before Tab completion existed still works (it just doesn't have it).
        protected virtual bool Autocomplete_Key() => false;
        #endregion
    }
}
