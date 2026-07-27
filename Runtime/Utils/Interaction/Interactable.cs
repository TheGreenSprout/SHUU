using UnityEngine;

namespace SHUU.Utils.Interaction
{
    #region XML doc
    /// <summary>
    /// Script holding some static variables used by the package, must be in all scenes.
    /// </summary>
    #endregion
    public abstract class Interactable : MonoBehaviour, IfaceInteractable
    {
        #region Variables
        [SerializeField] private bool synced_canBeInteracted = true;

        [SerializeField] private bool _canBeInteracted = true;
        public bool canBeInteracted
        {
            protected get => _canBeInteracted;
            set
            {
                _canBeInteracted = value;

                CanBeInteracted_Changed();


                if (synced_canBeInteracted)
                {
                    CanBeAltInteracted_Changed();

                    _canBeAltInteracted = value;
                }
            }
        }

        [SerializeField] private bool _canBeAltInteracted = true;
        public bool canBeAltInteracted
        {
            protected get => _canBeAltInteracted;
            set
            {
                _canBeAltInteracted = value;

                CanBeAltInteracted_Changed();


                if (synced_canBeInteracted)
                {
                    CanBeInteracted_Changed();
                    
                    _canBeInteracted = value;
                }
            }
        }


        [SerializeField] private bool holdInteract = false;
        [SerializeField] private bool holdAltInteract = false;

        [SerializeField] private bool stopHoldOnHoverEnd = false;


        [SerializeField] protected bool modifyDynamicCursor = true;


        protected bool beingHovered = false;

        protected bool beingInteracted = false;
        protected bool beingAltInteracted = false;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            canBeInteracted = true;
        }


        protected virtual void OnDestroy() => DynamicCursorInteraction.RemoveCursorAffector(this.gameObject);
        #endregion



        #region Logic

        #region Toggles
        #region XML doc
        /// <summary>
        /// Function used to see if an interactable script is, in fact, interactable.
        /// </summary>
        /// <returns>Returns wether the interactable can be interacted with or not.</returns>
        #endregion
        public virtual bool CanBeInteracted() => canBeInteracted || canBeAltInteracted;

        protected virtual void CanBeInteracted_Changed() { }
        protected virtual void CanBeAltInteracted_Changed() { }


        public bool HoldInteract() => holdInteract;
        public bool HoldAltInteract() => holdAltInteract;
        #endregion



        #region Interact
        #region XML doc
        /// <summary>
        /// Interaction logic.
        /// </summary>
        #endregion
        public void Interact()
        {
            if (!CanBeInteracted() || !canBeInteracted) return;


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


        public void AltInteract()
        {
            if (!CanBeInteracted() || !canBeAltInteracted) return;


            AltInteractLogic();

            if (holdAltInteract) beingAltInteracted = true;
        }
        protected virtual void AltInteractLogic() { }

        public void ReleaseAltInteract()
        {
            if (!holdAltInteract || !beingAltInteracted) return;


            ReleaseAltInteractLogic();

            beingAltInteracted = false;
        }
        protected virtual void ReleaseAltInteractLogic() { }
        #endregion



        #region Hover
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
        #endregion



        #region Keys
        public virtual InteractKeyState InteractKey() => InteractKeyState.Undefined;

        public virtual InteractKeyState AltInteractKey() => InteractKeyState.Undefined;
        #endregion
        
        #endregion
    }
}
