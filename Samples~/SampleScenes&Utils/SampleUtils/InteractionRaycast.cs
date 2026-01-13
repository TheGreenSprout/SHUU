using UnityEngine;
using SHUU.Utils.Helpers.Interaction;

#region XML doc
/// <summary>
/// Example script of how to code a basic interaction raycast using the SproutsHUU's interaction system.
/// </summary>
#endregion
public class InteractionRaycast : InteractionRaycastLogic
{
    [SerializeField] private KeyCode[] interactKeys = new KeyCode[] { KeyCode.E };

    [SerializeField] private int[] interactMouse = new int[] { 0 };




    protected override void Update()
    {
        base.Update();


        if (InputCheck()) Interact();
    }


    private bool InputCheck()
    {
        foreach (KeyCode key in interactKeys)
        {
            if (Input.GetKeyDown(key)) return true;
        }

        foreach (int mouse in interactMouse)
        {
            if (Input.GetMouseButtonDown(mouse)) return true;
        }


        return false;
    }
}
