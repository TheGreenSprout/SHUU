using UnityEngine;
using SHUU.Utils;
using SHUU.ForUser;

#region XML doc
/// <summary>
/// Example script of how to code a basic interaction raycast using the SproutsHUU's interaction system.
/// </summary>
#endregion
public class InteractionRaycast : MonoBehaviour
{
    public float interactionRange;


    public LayerMask interactablesMask;



    public IfaceInteractable previousInact;




    private void OnEnable()
    {
        CustomInputManager.AddInteractPressedCallback(OnInteractionPressed);


        previousInact = null;
    }

    private void OnDisable()
    {
        CustomInputManager.RemoveInteractPressedCallback(OnInteractionPressed);
    }


    private void Update()
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
        }
        else if (previousInact != null)
        {
            previousInact.HoverEnd();

            previousInact = null;
        }
    }

    private void OnInteractionPressed()
    {
        if (previousInact != null && previousInact.CanBeInteracted())
        {
            previousInact.Interact();
        }
    }
}
