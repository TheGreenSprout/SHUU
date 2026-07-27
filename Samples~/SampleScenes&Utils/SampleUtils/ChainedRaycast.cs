using UnityEngine;

using SHUU.Utils.Interaction;

namespace SHUU.Samples
{
    public class ChainedRaycast : ChainedRaycastLogic
    {
        #region Variables
        [SerializeField] private KeyCode[] interactKeys = new KeyCode[] { KeyCode.E };

        [SerializeField] private int[] interactMouse = new int[] { 0 };


        [SerializeField] private KeyCode[] altInteractKeys = new KeyCode[] { KeyCode.Q };

        [SerializeField] private int[] altInteractMouse = new int[] { 1 };
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

        protected override InteractKeyState AltInteractKey(IfaceInteractable target)
        {
            foreach (KeyCode key in altInteractKeys)
            {
                if (Input.GetKeyDown(key)) return InteractKeyState.Press;
                else if (Input.GetKeyUp(key)) return InteractKeyState.Release;
            } 

            foreach (int mouse in altInteractMouse)
            {
                if (Input.GetMouseButtonDown(mouse)) return InteractKeyState.Press;
                else if (Input.GetMouseButtonUp(mouse)) return InteractKeyState.Release;
            }

            return InteractKeyState.Idle;
        }
        #endregion
    }
}
