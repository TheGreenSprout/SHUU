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
                CanBeInteracted_Changed();

                _canBeInteracted = value;
            }
        }




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

        #region XML doc
        /// <summary>
        /// Interaction logic.
        /// </summary>
        #endregion
        public virtual void Interact() { }


        #region XML doc
        /// <summary>
        /// This runs when the interactable starts being hovered over.
        /// </summary>
        #endregion
        public virtual void HoverStart() => DynamicCursorInteraction.AddCursorAffector(this.gameObject);

        #region XML doc
        /// <summary>
        /// This runs when the interactable stops being hovered over.
        /// </summary>
        #endregion
        public virtual void HoverEnd() => DynamicCursorInteraction.RemoveCursorAffector(this.gameObject);
    }

}
