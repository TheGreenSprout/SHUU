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
            HoldInteractCheck();
            
            CastRay();
        }

        
        protected virtual void CastRay() => HandyFunctions.InteractionRaycast(ref previousInact, cam, interactionRange, layerMask, modifyDynamicCursor, tagMask);

        protected virtual void Interact() => previousInact?.Interact();

        protected virtual void ReleaseInteract() => previousInact?.ReleaseInteract();


        protected virtual void HoldInteractCheck()
        {
            if (previousInact != null && previousInact.HoldInteract()) holdInact = previousInact;

            if (holdInact != null)
            {
                bool? inact = previousInact?.InteractKey();
                inact = inact ?? InteractKey();

                if (inact != null)
                {
                    if (inact.Value) Interact();
                    else ReleaseInteract();
                }
            }
        }

        protected virtual bool? InteractKey() => null;
    }
}
