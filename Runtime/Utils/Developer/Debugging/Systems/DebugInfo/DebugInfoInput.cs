using UnityEngine;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    [RequireComponent(typeof(Debug_Info))]
    public class DebugInfoInput : MonoBehaviour
    {
        #region Variables
        public static bool CanToggle_debugInfo = true;


        private Debug_Info target = null;
        #endregion




        #region Main
        protected virtual void Awake() => target = GetComponent<Debug_Info>();
        #endregion



        #region Logic
        protected void Toggle()
        {
            if (!CanToggle_debugInfo) return;

            target?.Toggle();
        }


        protected void ToggleFpsGraph()
        {
            if (!CanToggle_debugInfo) return;

            target?.ToggleFpsGraph();
        }
        protected void FpsGraphCorner() => target?.FpsGraphCorner();


        protected void ToggleAxis() => target?.ToggleAxis();
        protected void AxisMode() => target?.AxisMode();

        protected void ToggleCompass() => target?.ToggleCompass();
        protected void CompassMode() => target?.CompassMode();
        protected void CompassPosition() => target?.CompassPosition();
        #endregion
    }
}
