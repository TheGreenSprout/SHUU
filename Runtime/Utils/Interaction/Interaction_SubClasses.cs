using System;
using UnityEngine;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Interaction
{
    public static class Interaction_SubClasses
    {
        #region Interaction Logic
        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Ray ray, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
        {
            IfaceInteractable inact = null;

            if (tagMaskPenetrate && tagMask != null && !tagMask.Value.Equals(TagMask.Everything))
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, interactionRange, interactionLayers ?? Physics.DefaultRaycastLayers);
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    if (hit.InteractionRaycast_Check(out IfaceInteractable candidate, tagMask))
                    {
                        inact = candidate;
                        break;
                    }
                }
            }
            else
            {
                bool raycast = Physics.Raycast(ray, out RaycastHit hitInfo, interactionRange, interactionLayers ?? Physics.DefaultRaycastLayers);
                if (raycast) hitInfo.InteractionRaycast_Check(out inact, tagMask);
            }


            if (inact != null)
            {
                if (previousInact != inact)
                {
                    ClearInteractHover(ref previousInact, modifyDynamicCursor);
                    previousInact = inact;
                    inact.HoverStart(modifyDynamicCursor);
                }
            }
            else ClearInteractHover(ref previousInact, modifyDynamicCursor);


            return inact != null;
        }

        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Camera camera, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
        {
            if (camera == null) return InteractionRaycast(ref previousInact, interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);

            return InteractionRaycast(ref previousInact, camera.ScreenPointToRay(Input.mousePosition), interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);
        }

        public static bool InteractionRaycast(ref IfaceInteractable previousInact, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
            => InteractionRaycast(ref previousInact, Camera.main.ScreenPointToRay(Input.mousePosition), interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);


        public static bool InteractionRaycast_Check(this RaycastHit hit, out IfaceInteractable inactScript, TagMask? tagMask = null)
        {
            inactScript = null;

            if (!hit.collider.gameObject.TryGetComponent(out IfaceInteractable inact)) return false;
            inactScript = inact;

            if (!inact.CanBeInteracted()) return false;

            if (tagMask != null && !tagMask.Value.Contains(hit.collider.tag)) return false;

            return true;
        }


        public static void ClearInteractHover(ref IfaceInteractable previousInact, bool modifyDynamicCursor)
        {
            if (previousInact == null) return;

            previousInact.HoverEnd(modifyDynamicCursor);
            previousInact = null;
        }
        #endregion
    }




    #region Helper classes
    public enum InteractKeyState
    {
        Undefined,
        Idle,
        Press,
        Release
    }
    #endregion
}
