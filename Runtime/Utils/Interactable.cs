using SHUU.Utils.Helpers.Interaction;
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
        [SerializeField] private bool _canBeInteracted = true;
        public bool canBeInteracted
        {
            protected get => _canBeInteracted;
            set
            {
                _canBeInteracted = value;

                CanBeInteracted_Changed();
            }
        }

        [SerializeField] private bool holdInteract = false;

        [SerializeField] private bool stopHoldOnHoverEnd = false;


        [SerializeField] protected bool modifyDynamicCursor = true;


        protected bool beingHovered = false;

        protected bool beingInteracted = false;




        protected virtual void Awake()
        {
            canBeInteracted = true;
        }

        protected virtual void OnDestroy() => DynamicCursorInteraction.RemoveCursorAffector(this.gameObject);


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
        protected virtual void CanBeInteracted_Changed() { }

        public bool HoldInteract() => holdInteract;

        #region XML doc
        /// <summary>
        /// Interaction logic.
        /// </summary>
        #endregion
        public void Interact()
        {
            if (!CanBeInteracted()) return;


            InteractLogic();

            if (holdInteract) beingInteracted = true;
        }
        protected virtual void InteractLogic() { }

        public void ReleaseInteract()
        {
            if (!holdInteract || !beingInteracted) return;


            ReleaseInteractLogic();

            beingInteracted = false;
        }
        protected virtual void ReleaseInteractLogic() { }


        #region XML doc
        /// <summary>
        /// This runs when the interactable starts being hovered over.
        /// </summary>
        #endregion
        public void HoverStart(bool _modifyDynamicCursor = true)
        {
            if (modifyDynamicCursor && _modifyDynamicCursor) DynamicCursorInteraction.AddCursorAffector(this.gameObject);

            beingHovered = true;

            HoverStartLogic();
        }
        protected virtual void HoverStartLogic() { }

        #region XML doc
        /// <summary>
        /// This runs when the interactable stops being hovered over.
        /// </summary>
        #endregion
        public void HoverEnd(bool _modifyDynamicCursor = true)
        {
            if (modifyDynamicCursor && _modifyDynamicCursor) DynamicCursorInteraction.RemoveCursorAffector(this.gameObject);

            if (stopHoldOnHoverEnd) ReleaseInteract();

            beingHovered = false;

            HoverEndLogic();
        }
        protected virtual void HoverEndLogic() { }


        public virtual bool? InteractKey() => null;
    }

}
