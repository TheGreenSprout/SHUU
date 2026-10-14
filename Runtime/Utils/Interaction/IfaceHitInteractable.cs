using UnityEngine;

namespace SHUU.Utils.Interaction
{
    public interface IfaceHitInteractable : IfaceInteractable
    {
        public void HoverHit(RaycastHit hit);
    }
}
