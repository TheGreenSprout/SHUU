using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class InteractionRaycastLogic : MonoBehaviour
    {
        [SerializeField] protected Camera cam;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask interactablesMask;



        protected IfaceInteractable previousInact;




        protected virtual void Awake() => previousInact = null;



        protected virtual void Update() => CastRay();

        
        protected virtual void CastRay() => HandyFunctions.InteractionRaycast(ref previousInact, cam, interactionRange, interactablesMask);

        protected virtual void Interact()
        {
            if (previousInact != null && previousInact.CanBeInteracted()) previousInact.Interact();
        }
    }
}
