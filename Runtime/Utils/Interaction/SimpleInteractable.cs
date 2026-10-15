using UnityEngine;
using UnityEngine.Events;

using Alchemy.Inspector;

namespace SHUU.Utils.Interaction
{
    public class SimpleInteractable : Interactable
    {
        #region Variables
        [Space, Title("Events")]
        [SerializeField] private UnityEvent onHoverStart;
        [SerializeField] private UnityEvent onHoverEnd;


        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private UnityEvent onHoldStart;
        [SerializeField] private UnityEvent onHoldEnd;

        [SerializeField] private UnityEvent onAltInteract;
        [SerializeField] private UnityEvent onAltHoldStart;
        [SerializeField] private UnityEvent onAltHoldEnd;
        #endregion




        #region Override points
        protected override void HoverStartLogic()
        {
            base.HoverStartLogic();

            onHoverStart?.Invoke();
        }

        protected override void HoverEndLogic()
        {
            base.HoverEndLogic();

            onHoverEnd?.Invoke();
        }


        protected override void InteractLogic()
        {
            base.InteractLogic();

            onInteract?.Invoke();
            if (HoldInteract()) onHoldStart?.Invoke();
        }
        protected override void ReleaseInteractLogic()
        {
            base.ReleaseInteractLogic();

            onHoldEnd?.Invoke();
        }

        protected override void AltInteractLogic()
        {
            base.AltInteractLogic();

            onAltInteract?.Invoke();
            if (HoldAltInteract()) onAltHoldStart?.Invoke();
        }
        protected override void ReleaseAltInteractLogic()
        {
            base.ReleaseAltInteractLogic();

            onAltHoldEnd?.Invoke();
        }
        #endregion
    }
}
