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




        protected virtual void Awake() => previousInact = null;



        protected virtual void Update()
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
            

            CastRay();
        }

        
        protected virtual void CastRay() => HandyFunctions.InteractionRaycast(ref previousInact, cam, interactionRange, layerMask, modifyDynamicCursor, tagMask);

        protected virtual void Interact() => previousInact?.Interact();

        protected virtual void ReleaseInteract() => previousInact?.ReleaseInteract();


        protected virtual bool? InteractKey() => null;
    }
}
