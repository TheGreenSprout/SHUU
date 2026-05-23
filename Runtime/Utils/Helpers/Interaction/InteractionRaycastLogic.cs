using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Helpers.Interaction
{
    public class InteractionRaycastLogic : MonoBehaviour
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
            if (holdInact != null && previousInact == null) holdInact?.ReleaseInteract();
            else previousInact?.ReleaseInteract();

            if (holdInact != null) holdInact = null;
        }


        protected virtual void InteractCheck()
        {
            if (previousInact != null)
            {
                bool? inact = previousInact?.InteractKey();
                inact = inact ?? InteractKey(previousInact);

                if (inact != null)
                {
                    if (inact.Value) Interact();
                    else ReleaseInteract();
                }
            }
            else if (holdInact != null)
            {
                bool? inact = holdInact?.InteractKey();
                inact = inact ?? InteractKey(holdInact);

                if (inact != null && !inact.Value)
                {
                    ReleaseInteract();

                    holdInact = null;
                }
            }
        }
        #endregion



        #region Override points
        protected virtual bool? InteractKey(IfaceInteractable target) => null;
        #endregion
    }
}
