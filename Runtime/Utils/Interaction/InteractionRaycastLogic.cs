using UnityEngine;
using Alchemy.Inspector;

using SHUU.Utils.Helpers;

using static SHUU.Utils.Interaction.Interaction_SubClasses;

namespace SHUU.Utils.Interaction
{
    public abstract class InteractionRaycastLogic : MonoBehaviour
    {
        #region Variables
        [Header("Raycast Settings")]
        [SerializeField] protected Camera cam;


        [SerializeField] protected bool modifyDynamicCursor = true;
        [SerializeField] protected bool tagMaskPenetrate = false;


        [SerializeField] protected float interactionRange;

        [SerializeField] protected LayerMask layerMask = ~0;
        [SerializeField] protected TagMask tagMask = TagMask.Everything;

        [SerializeField, ShowIf(nameof(SupportsCastShapes))] protected CastShapeSettings castShape = new();
        
        [Space(15)]



        protected IfaceInteractable previousInact;
        
        protected IfaceInteractable holdInact;
        #endregion




        #region Main
        protected virtual void Awake() => previousInact = null;



        protected virtual void Update()
        {
            InteractCheck();
            
            CastRay();
        }
        #endregion

        

        #region Logic
        protected virtual bool SupportsCastShapes => true;

        protected virtual bool CastRay() => InteractionCast(ref previousInact, cam, SupportsCastShapes ? castShape : null, interactionRange, layerMask, tagMask, tagMaskPenetrate, modifyDynamicCursor);


        protected virtual void Interact()
        {
            previousInact?.Interact();

            if (previousInact != null && previousInact.HoldInteract()) holdInact = previousInact;
        }
        protected virtual void ReleaseInteract()
        {
            holdInact?.ReleaseInteract();
            previousInact?.ReleaseInteract();

            if (holdInact != null) holdInact = null;
        }

        protected virtual void AltInteract()
        {
            previousInact?.AltInteract();

            if (previousInact != null && previousInact.HoldAltInteract()) holdInact = previousInact;
        }
        protected virtual void ReleaseAltInteract()
        {
            holdInact?.ReleaseAltInteract();
            previousInact?.ReleaseAltInteract();

            if (holdInact != null) holdInact = null;
        }


        protected virtual void InteractCheck()
        {
            if (previousInact != null)
            {
                InteractKeyState? inact = previousInact?.InteractKey();
                inact = inact != null && inact.Value != InteractKeyState.Undefined ? inact : InteractKey(previousInact);

                if (inact != null && inact.Value != InteractKeyState.Undefined)
                {
                    if (inact.Value == InteractKeyState.Press) Interact();
                    else if (inact.Value == InteractKeyState.Release) ReleaseInteract();
                }


                InteractKeyState? altInact = previousInact?.AltInteractKey();
                altInact = altInact != null && altInact.Value != InteractKeyState.Undefined ? altInact : AltInteractKey(previousInact);

                if (altInact != null && altInact.Value != InteractKeyState.Undefined)
                {
                    if (altInact.Value == InteractKeyState.Press) AltInteract();
                    else if (altInact.Value == InteractKeyState.Release) ReleaseAltInteract();
                }
            }
            else if (holdInact != null)
            {
                InteractKeyState? inact = holdInact?.InteractKey();
                inact = inact != null && inact.Value != InteractKeyState.Undefined ? inact : InteractKey(holdInact);

                if (inact != null && inact.Value != InteractKeyState.Undefined && inact.Value == InteractKeyState.Release)
                {
                    ReleaseInteract();

                    holdInact = null;
                }


                InteractKeyState? altInact = holdInact?.AltInteractKey();
                altInact = altInact != null && altInact.Value != InteractKeyState.Undefined ? altInact : AltInteractKey(holdInact);

                if (altInact != null && altInact.Value != InteractKeyState.Undefined && altInact.Value == InteractKeyState.Release)
                {
                    ReleaseAltInteract();

                    holdInact = null;
                }
            }
        }
        #endregion



        #region Override points
        protected abstract InteractKeyState InteractKey(IfaceInteractable target);

        protected abstract InteractKeyState AltInteractKey(IfaceInteractable target);
        #endregion



        #region Gizmos
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Camera view = cam != null ? cam : Camera.main;

            if (view == null || interactionRange <= 0f) return;

            Ray ray = Application.isPlaying ? view.ScreenPointToRay(PointerPosition()) : view.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            CastShapeSettings shape = SupportsCastShapes ? castShape : null;

            if (shape != null && shape.IsShaped) DrawInteractionCast(ray, view.transform.rotation, shape, interactionRange, layerMask, SHUU_Gizmos.Blue);
            else DrawInteractionRay(ray, interactionRange, layerMask, SHUU_Gizmos.Blue, shape != null ? shape.triggers : QueryTriggerInteraction.UseGlobal);
        }

        protected static void DrawInteractionCast(Ray ray, Quaternion view, CastShapeSettings shape, float range, LayerMask mask, Color color)
        {
            Vector3 from = shape.CastOrigin(ray, range, out _);
            Vector3 to = ray.origin + ray.direction * range;
            Quaternion orientation = shape.Orientation(view);

            if (Application.isPlaying && CastFirst(ray, view, shape, range, mask, out RaycastHit hit))
            {
                Vector3 at = from + ray.direction * hit.distance;

                shape.DrawSweep(from, at, orientation, SHUU_Gizmos.Green);

                SHUU_Gizmos.Line(at, to, color.WithAlpha(0.45f));
                shape.DrawAt(to, orientation, color.WithAlpha(0.45f));

                SHUU_Gizmos.Sphere(hit.point, SHUU_Gizmos.Size(hit.point, 0.04f), SHUU_Gizmos.Green);

                return;
            }

            shape.DrawSweep(from, to, orientation, color);
        }

        protected static void DrawInteractionRay(Ray ray, float range, LayerMask mask, Color color, QueryTriggerInteraction triggers = QueryTriggerInteraction.UseGlobal)
        {
            if (Application.isPlaying && Physics.Raycast(ray, out RaycastHit hit, range, mask, triggers))
            {
                SHUU_Gizmos.Line(ray.origin, hit.point, SHUU_Gizmos.Green);
                SHUU_Gizmos.Line(hit.point, ray.origin + ray.direction * range, color.WithAlpha(0.45f));
                SHUU_Gizmos.Sphere(hit.point, 0.04f, SHUU_Gizmos.Green);

                return;
            }

            SHUU_Gizmos.Beam(ray.origin, ray.direction, range, color);
        }
#endif
        #endregion
    }
}
