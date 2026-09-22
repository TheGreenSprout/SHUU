using UnityEngine;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    public abstract class DebugInfoInput_EasyUse : DebugInfoInput
    {
        #region Main
        protected virtual void Update()
        {
            if (Toggle_Key()) Toggle();


            if (ToggleFpsGraph_Key()) ToggleFpsGraph();
            if (FpsGraphCorner_Key()) FpsGraphCorner();


            if (ToggleAxis_Key()) ToggleAxis();
            if (AxisMode_Key()) AxisMode();
            
            if (ToggleCompass_Key()) ToggleCompass();
            if (CompassMode_Key()) CompassMode();
            if (CompassPosition_Key()) CompassPosition();
        }
        #endregion



        #region Logic
        protected abstract bool Toggle_Key();


        protected abstract bool ToggleFpsGraph_Key();
        protected abstract bool FpsGraphCorner_Key();


        protected abstract bool ToggleAxis_Key();
        protected abstract bool AxisMode_Key();

        protected abstract bool ToggleCompass_Key();
        protected abstract bool CompassMode_Key();
        protected abstract bool CompassPosition_Key();
        #endregion
    }
}
