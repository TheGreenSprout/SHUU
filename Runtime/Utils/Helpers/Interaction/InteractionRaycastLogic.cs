using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class InteractionRaycastLogic : MonoBehaviour
    {
        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask interactablesMask;



        protected IfaceInteractable previousInact;




        protected virtual void Awake() => previousInact = null;



        protected virtual void Update() => CastRay();

        
        protected virtual void CastRay()
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out RaycastHit hitInfo, interactionRange, interactablesMask))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out IfaceInteractable inact))
                {
                    if (previousInact != inact)
                    {
                        if (previousInact != null)
                        {
                            previousInact.HoverEnd();
                        }

                        previousInact = inact;


                        inact.HoverStart();
                    }
                }
                else if (previousInact != null)
                {
                    previousInact.HoverEnd();

                    previousInact = null;
                }
            }
            else if (previousInact != null)
            {
                previousInact.HoverEnd();

                previousInact = null;
            }
        }

        protected virtual void Interact()
        {
            if (previousInact != null && previousInact.CanBeInteracted()) previousInact.Interact();
        }
    }
}
