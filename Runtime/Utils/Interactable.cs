using UnityEngine;

namespace SHUU.Utils
{

#region XML doc
/// <summary>
/// Script holding some static variables used by the package, must be in all scenes.
/// </summary>
#endregion
public class Interactable : MonoBehaviour, IfaceInteractable
{
    private bool canBeInteracted;




    protected virtual void Awake()
    {
        canBeInteracted = true;
    }


    #region XML doc
    /// <summary>
    /// Function used to see if an interactable script is, in fact, interactable.
    /// </summary>
    /// <returns>Returns wether the interactable can be interacted with or not.</returns>
    #endregion
    public virtual bool CanBeInteracted()
    {
        return canBeInteracted;
    }

    #region XML doc
    /// <summary>
    /// Interaction logic.
    /// </summary>
    #endregion
    public virtual void Interact()
    {
        Debug.LogWarning("Interaction void [Interact()] not set up for object: " + this.name);
    }


    #region XML doc
    /// <summary>
    /// This runs when the interactable starts being hovered over.
    /// </summary>
    #endregion
    public virtual void HoverStart()
    {
        Debug.LogWarning("Interaction void [HoverStart()] not set up for object: " + this.name);
    }

    #region XML doc
    /// <summary>
    /// This runs when the interactable stops being hovered over.
    /// </summary>
    #endregion
    public virtual void HoverEnd()
    {
        Debug.LogWarning("Interaction void [HoverEnd()] not set up for object: " + this.name);
    }
}

}
