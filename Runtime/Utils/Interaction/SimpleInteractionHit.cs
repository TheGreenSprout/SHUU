using UnityEngine;
using UnityEngine.Events;

using Alchemy.Inspector;

namespace SHUU.Utils.Interaction
{
    public sealed class SimpleHitInteractable : SimpleInteractable, IfaceHitInteractable
    {
        #region Variables
        [Space, Title("Hit Events")]
        [SerializeField] private UnityEvent<RaycastHit> onHit;
        #endregion




        #region Override points
        public void HoverHit(RaycastHit hit) => onHit?.Invoke(hit);
        #endregion
    }
}
