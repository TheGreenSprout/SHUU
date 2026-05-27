using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Helpers.Interaction
{
    public abstract class InteractionRaycastLogic : MonoBehaviour
    {
        #region Variables
        [Header("Raycast Settings")]
        [SerializeField] protected Camera cam;


        [SerializeField] protected bool modifyDynamicCursor = true;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask layerMask;
        [SerializeField] protected string[] tagMask = new string[0];
        
        [Space(15)]



        protected IfaceInteractable previousInact;
        
        protected IfaceInteractable holdInact;
        #endregion




        #region Main
        protected virtual void Awake() => previousInact = null;



        protected virtual void Update()
        {
            InteractCheck();
            
            CastRay();
        }
        #endregion

        

        #region Logic
        protected virtual bool CastRay() => InteractionRaycast(ref previousInact, cam, interactionRange, layerMask, modifyDynamicCursor, tagMask);


        protected virtual void Interact()
        {
            previousInact?.Interact();

            if (previousInact != null && previousInact.HoldInteract()) holdInact = previousInact;
        }

        protected virtual void ReleaseInteract()
        {
            holdInact?.ReleaseInteract();
            previousInact?.ReleaseInteract();

            if (holdInact != null) holdInact = null;
        }


        protected virtual void InteractCheck()
        {
            if (previousInact != null)
            {
                InteractKeyState? inact = previousInact?.InteractKey();
                inact = inact != null && inact.Value != InteractKeyState.Undefined ? inact : InteractKey(previousInact);

                if (inact != null && inact.Value != InteractKeyState.Undefined)
                {
                    if (inact.Value == InteractKeyState.Press) Interact();
                    else if (inact.Value == InteractKeyState.Release) ReleaseInteract();
                }
            }
            else if (holdInact != null)
            {
                InteractKeyState? inact = holdInact?.InteractKey();
                inact = inact != null && inact.Value != InteractKeyState.Undefined ? inact : InteractKey(holdInact);

                if (inact != null && inact.Value != InteractKeyState.Undefined && inact.Value == InteractKeyState.Release)
                {
                    ReleaseInteract();

                    holdInact = null;
                }
            }
        }
        #endregion



        #region Override points
        protected abstract InteractKeyState InteractKey(IfaceInteractable target);
        #endregion
    }




    #region Helper Class
    public enum InteractKeyState
    {
        Undefined,
        Idle,
        Press,
        Release
    }
    #endregion
}
