namespace SHUU.Utils
{

#region XML doc
/// <summary>
/// Interface implemented by all interactable scripts.
/// </summary>
#endregion
public interface IfaceInteractable
{
    #region XML doc
    /// <summary>
    /// Function used to see if an interactable script is, in fact, interactable.
    /// </summary>
    /// <returns>Returns wether the interactable can be interacted with or not.</returns>
    #endregion
    public bool CanBeInteracted() => true;
    
    #region XML doc
    /// <summary>
    /// Interaction logic.
    /// </summary>
    #endregion
    public void Interact();
    
    #region XML doc
    /// <summary>
    /// This runs when the interactable starts being hovered over.
    /// </summary>
    #endregion
    public void HoverStart(bool _modifyDynamicCursor = true);
    #region XML doc
    /// <summary>
    /// This runs when the interactable stops being hovered over.
    /// </summary>
    #endregion
    public void HoverEnd(bool _modifyDynamicCursor = true);
}

}
