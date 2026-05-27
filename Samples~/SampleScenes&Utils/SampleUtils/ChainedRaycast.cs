using UnityEngine;

using SHUU.Utils;
using SHUU.Utils.Helpers.Interaction;

namespace SHUU.Samples
{
    public class ChainedRaycast : ChainedRaycastLogic
    {
        #region Variables
        [SerializeField] private KeyCode[] interactKeys = new KeyCode[] { KeyCode.E };

        [SerializeField] private int[] interactMouse = new int[] { 0 };
        #endregion




        #region Logic
        protected override InteractKeyState InteractKey(IfaceInteractable target)
        {
            foreach (KeyCode key in interactKeys)
            {
                if (Input.GetKeyDown(key)) return InteractKeyState.Press;
                else if (Input.GetKeyUp(key)) return InteractKeyState.Release;
            } 

            foreach (int mouse in interactMouse)
            {
                if (Input.GetMouseButtonDown(mouse)) return InteractKeyState.Press;
                else if (Input.GetMouseButtonUp(mouse)) return InteractKeyState.Release;
            }

            return InteractKeyState.Idle;
        }
        #endregion
    }
}
