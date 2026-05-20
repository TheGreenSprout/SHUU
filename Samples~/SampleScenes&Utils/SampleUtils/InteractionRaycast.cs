using UnityEngine;

using SHUU.Utils.Helpers.Interaction;

namespace SHUU.Samples.SampleScenesAndUtils.SampleUtils
{
    #region XML doc
    /// <summary>
    /// Example script of how to code a basic interaction raycast using the SproutsHUU's interaction system.
    /// </summary>
    #endregion
    public class InteractionRaycast : InteractionRaycastLogic
    {
        #region Variables
        [SerializeField] private KeyCode[] interactKeys = new KeyCode[] { KeyCode.E };

        [SerializeField] private int[] interactMouse = new int[] { 0 };
        #endregion




        #region Main
        protected override void Update()
        {
            base.Update();

            if (InputCheck()) Interact();
        }
        #endregion



        #region Logic
        private bool InputCheck()
        {
            foreach (KeyCode key in interactKeys)
                if (Input.GetKeyDown(key)) return true;

            foreach (int mouse in interactMouse)
                if (Input.GetMouseButtonDown(mouse)) return true;


            return false;
        }
        #endregion
    }
}
