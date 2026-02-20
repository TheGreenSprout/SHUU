using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class InteractionRaycastLogic : MonoBehaviour
    {
        [SerializeField] protected Camera cam;


        [SerializeField] protected bool modifyDynamicCursor = true;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask interactablesMask;
        [SerializeField] protected string[] tagMask = new string[0];



        protected IfaceInteractable previousInact;




        protected virtual void Awake() => previousInact = null;



        protected virtual void Update() => CastRay();

        
        protected virtual void CastRay() => HandyFunctions.InteractionRaycast(ref previousInact, cam, interactionRange, interactablesMask, modifyDynamicCursor, tagMask);

        protected virtual void Interact() => previousInact?.Interact();
    }
}
