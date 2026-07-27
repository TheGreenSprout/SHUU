using UnityEngine;

using SHUU.Utils.Helpers;

using static SHUU.Utils.Interaction.Interaction_SubClasses;

namespace SHUU.Utils.Interaction
{
    public abstract class InteractionRaycastLogic : MonoBehaviour
    {
        #region Variables
        [Header("Raycast Settings")]
        [SerializeField] protected Camera cam;


        [SerializeField] protected bool modifyDynamicCursor = true;
        [SerializeField] protected bool tagMaskPenetrate = false;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask layerMask = ~0;
        [SerializeField] protected TagMask tagMask = TagMask.Everything;
        
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
        protected virtual bool CastRay() => InteractionRaycast(ref previousInact, cam, interactionRange, layerMask, tagMask, tagMaskPenetrate, modifyDynamicCursor);


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

        protected virtual void AltInteract()
        {
            previousInact?.AltInteract();

            if (previousInact != null && previousInact.HoldAltInteract()) holdInact = previousInact;
        }
        protected virtual void ReleaseAltInteract()
        {
            holdInact?.ReleaseAltInteract();
            previousInact?.ReleaseAltInteract();

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


                InteractKeyState? altInact = previousInact?.AltInteractKey();
                altInact = altInact != null && altInact.Value != InteractKeyState.Undefined ? altInact : AltInteractKey(previousInact);

                if (altInact != null && altInact.Value != InteractKeyState.Undefined)
                {
                    if (altInact.Value == InteractKeyState.Press) AltInteract();
                    else if (altInact.Value == InteractKeyState.Release) ReleaseAltInteract();
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


                InteractKeyState? altInact = holdInact?.AltInteractKey();
                altInact = altInact != null && altInact.Value != InteractKeyState.Undefined ? altInact : AltInteractKey(holdInact);

                if (altInact != null && altInact.Value != InteractKeyState.Undefined && altInact.Value == InteractKeyState.Release)
                {
                    ReleaseAltInteract();

                    holdInact = null;
                }
            }
        }
        #endregion



        #region Override points
        protected abstract InteractKeyState InteractKey(IfaceInteractable target);

        protected abstract InteractKeyState AltInteractKey(IfaceInteractable target);
        #endregion
    }
}
