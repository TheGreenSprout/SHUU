using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class InteractionRaycastLogic : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] protected Camera cam;


        [SerializeField] protected bool modifyDynamicCursor = true;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask layerMask;
        [SerializeField] protected string[] tagMask = new string[0];
        [Space(15)]



        protected IfaceInteractable previousInact;
        
        protected IfaceInteractable holdInact;




        protected virtual void Awake() => previousInact = null;



        protected virtual void Update()
        {
            InteractCheck();
            
            CastRay();
        }

        
        protected virtual bool CastRay() => HandyFunctions.InteractionRaycast(ref previousInact, cam, interactionRange, layerMask, modifyDynamicCursor, tagMask);

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
                inact = inact ?? InteractKey();

                if (inact != null)
                {
                    if (inact.Value) Interact();
                    else ReleaseInteract();
                }
            }
            else if (holdInact != null)
            {
                bool? inact = holdInact?.InteractKey();
                inact = inact ?? InteractKey();

                if (inact != null && !inact.Value)
                {
                    ReleaseInteract();

                    holdInact = null;
                }
            }
        }

        protected virtual bool? InteractKey() => null;
    }
}
