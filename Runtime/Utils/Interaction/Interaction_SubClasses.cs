using System;
using System.Collections.Generic;
using UnityEngine;
using Alchemy.Inspector;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Interaction
{
    public static class Interaction_SubClasses
    {
        #region Interaction Logic
        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Ray ray, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
            => InteractionCast(ref previousInact, ray, Quaternion.identity, null, interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);

        public static bool InteractionRaycast(ref IfaceInteractable previousInact, Camera camera, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
        {
            if (camera == null) return InteractionRaycast(ref previousInact, interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);

            return InteractionRaycast(ref previousInact, camera.ScreenPointToRay(PointerPosition()), interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);
        }

        public static bool InteractionRaycast(ref IfaceInteractable previousInact, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
            => InteractionRaycast(ref previousInact, Camera.main.ScreenPointToRay(PointerPosition()), interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);


        public static bool InteractionCast(ref IfaceInteractable previousInact, Ray ray, Quaternion view, CastShapeSettings shape, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
        {
            IfaceInteractable inact = null;

            int layers = interactionLayers ?? Physics.DefaultRaycastLayers;

            if (tagMaskPenetrate && tagMask != null && tagMask.Value.mask != TagMask.Everything.mask)
            {
                if (shape != null && shape.IsShaped)
                {
                    int count = CastAll(ray, view, shape, interactionRange, layers);
                    float closest = float.MaxValue;

                    for (int i = 0; i < count; i++)
                    {
                        RaycastHit hit = CastBuffer[i];

                        if (hit.distance >= closest || !hit.InteractionRaycast_Check(out IfaceInteractable candidate, tagMask)) continue;

                        closest = hit.distance;
                        inact = candidate;
                    }
                }
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(ray, interactionRange, layers, shape != null ? shape.triggers : QueryTriggerInteraction.UseGlobal);
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
            }
            else
            {
                bool cast = CastFirst(ray, view, shape, interactionRange, layers, out RaycastHit hitInfo);
                if (cast) hitInfo.InteractionRaycast_Check(out inact, tagMask);
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

        public static bool InteractionCast(ref IfaceInteractable previousInact, Camera camera, CastShapeSettings shape, float interactionRange, LayerMask? interactionLayers = null, TagMask? tagMask = null, bool tagMaskPenetrate = false, bool modifyDynamicCursor = true)
        {
            if (camera == null) camera = Camera.main;

            if (camera == null)
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }

            return InteractionCast(ref previousInact, camera.ScreenPointToRay(PointerPosition()), camera.transform.rotation, shape, interactionRange, interactionLayers, tagMask, tagMaskPenetrate, modifyDynamicCursor);
        }


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


        private const int MaxCastHits = 64;
        private static readonly RaycastHit[] CastBuffer = new RaycastHit[MaxCastHits];


        internal static bool CastFirst(Ray ray, Quaternion view, CastShapeSettings shape, float range, int layers, out RaycastHit hit)
        {
            if (shape == null || !shape.IsShaped) return Physics.Raycast(ray, out hit, range, layers, shape != null ? shape.triggers : QueryTriggerInteraction.UseGlobal);

            Vector3 origin = shape.CastOrigin(ray, range, out float distance);
            Quaternion rotation = shape.Orientation(view);

            switch (shape.shape)
            {
                case CastShape.Sphere:
                    return Physics.SphereCast(origin, shape.CastRadius, ray.direction, out hit, distance, layers, shape.triggers);

                case CastShape.Box:
                    return Physics.BoxCast(origin, shape.CastHalfExtents, ray.direction, out hit, rotation, distance, layers, shape.triggers);

                default:
                    shape.CapsuleEnds(origin, rotation, out Vector3 a, out Vector3 b);
                    return Physics.CapsuleCast(a, b, shape.CastRadius, ray.direction, out hit, distance, layers, shape.triggers);
            }
        }

        private static int CastAll(Ray ray, Quaternion view, CastShapeSettings shape, float range, int layers)
        {
            Vector3 origin = shape.CastOrigin(ray, range, out float distance);
            Quaternion rotation = shape.Orientation(view);

            switch (shape.shape)
            {
                case CastShape.Sphere:
                    return Physics.SphereCastNonAlloc(origin, shape.CastRadius, ray.direction, CastBuffer, distance, layers, shape.triggers);

                case CastShape.Box:
                    return Physics.BoxCastNonAlloc(origin, shape.CastHalfExtents, ray.direction, CastBuffer, rotation, distance, layers, shape.triggers);

                default:
                    shape.CapsuleEnds(origin, rotation, out Vector3 a, out Vector3 b);
                    return Physics.CapsuleCastNonAlloc(a, b, shape.CastRadius, ray.direction, CastBuffer, distance, layers, shape.triggers);
            }
        }


        public static Vector2 PointerPosition()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
            #elif ENABLE_INPUT_SYSTEM
            return UnityEngine.InputSystem.Mouse.current != null ? UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Vector2.zero;
            #else
            return Vector2.zero;
            #endif
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



    public enum CastShape
    {
        Ray,
        Sphere,
        Box,
        Capsule
    }

    [Serializable]
    public class CastShapeSettings
    {
        #region Variables
        public CastShape shape = CastShape.Ray;


        [ShowIf(nameof(UsesRadius)), Min(0f)] public float radius = 0.1f;

        [ShowIf(nameof(IsBox))] public Vector3 halfExtents = new Vector3(0.1f, 0.1f, 0.1f);

        [ShowIf(nameof(IsCapsule)), Min(0f), Tooltip("Total length of the capsule, caps included.")] public float height = 0.5f;

        [ShowIf(nameof(UsesRotation)), Tooltip("Extra rotation in degrees, applied on top of the camera's rotation. A capsule's length runs along its local up axis.")] public Vector3 rotation = Vector3.zero;


        [ShowIf(nameof(IsShaped)), Tooltip("Moves where the cast starts along the ray, the cast always ends at the interaction range. Casts ignore colliders they already overlap at the start: pull it back (negative) to catch things right in front of the camera, or push it forward to skip what is around the player.")]
        public float startOffset = 0f;

        public QueryTriggerInteraction triggers = QueryTriggerInteraction.UseGlobal;


        private const float MinSize = 0.0005f;
        #endregion




        #region Logic
        public bool IsShaped => shape != CastShape.Ray;

        private bool UsesRadius => shape == CastShape.Sphere || shape == CastShape.Capsule;
        private bool IsBox => shape == CastShape.Box;
        private bool IsCapsule => shape == CastShape.Capsule;
        private bool UsesRotation => shape == CastShape.Box || shape == CastShape.Capsule;


        internal float CastRadius => Mathf.Max(radius, MinSize);

        internal Vector3 CastHalfExtents => new Vector3(Mathf.Max(Mathf.Abs(halfExtents.x), MinSize), Mathf.Max(Mathf.Abs(halfExtents.y), MinSize), Mathf.Max(Mathf.Abs(halfExtents.z), MinSize));


        internal Quaternion Orientation(Quaternion view) => view * Quaternion.Euler(rotation);

        internal Vector3 CastOrigin(Ray ray, float range, out float distance)
        {
            float start = Mathf.Min(startOffset, range);

            distance = Mathf.Max(range - start, 0f);

            return ray.origin + ray.direction * start;
        }

        internal void CapsuleEnds(Vector3 center, Quaternion orientation, out Vector3 a, out Vector3 b)
        {
            Vector3 axis = orientation * Vector3.up * Mathf.Max(height * 0.5f - CastRadius, 0f);

            a = center + axis;
            b = center - axis;
        }
        #endregion




        #region Gizmos
#if UNITY_EDITOR
        internal void DrawAt(Vector3 center, Quaternion orientation, Color color)
        {
            switch (shape)
            {
                case CastShape.Sphere:
                    SHUU_Gizmos.Sphere(center, CastRadius, color);
                    break;

                case CastShape.Box:
                    SHUU_Gizmos.Box(center, orientation, CastHalfExtents, color);
                    break;

                case CastShape.Capsule:
                    CapsuleEnds(center, orientation, out Vector3 a, out Vector3 b);
                    SHUU_Gizmos.Capsule(a, b, CastRadius, color);
                    break;
            }
        }

        internal void DrawSweep(Vector3 from, Vector3 to, Quaternion orientation, Color color)
        {
            DrawAt(from, orientation, color);
            DrawAt(to, orientation, color);

            Vector3 direction = to - from;

            if (direction.sqrMagnitude < 1e-8f) return;

            foreach (Vector3 edge in SweepEdges(orientation, direction.normalized))
                SHUU_Gizmos.Line(from + edge, to + edge, color);
        }

        private IEnumerable<Vector3> SweepEdges(Quaternion orientation, Vector3 direction)
        {
            SHUU_Gizmos.Perpendiculars(direction, out Vector3 side, out Vector3 other);

            float radius = CastRadius;

            switch (shape)
            {
                case CastShape.Sphere:
                    yield return side * radius;
                    yield return -side * radius;
                    yield return other * radius;
                    yield return -other * radius;
                    break;

                case CastShape.Box:
                    Vector3 extents = CastHalfExtents;

                    for (int i = 0; i < 8; i++)
                        yield return orientation * new Vector3((i & 1) == 0 ? -extents.x : extents.x, (i & 2) == 0 ? -extents.y : extents.y, (i & 4) == 0 ? -extents.z : extents.z);

                    break;

                case CastShape.Capsule:
                    CapsuleEnds(Vector3.zero, orientation, out Vector3 a, out Vector3 b);

                    foreach (Vector3 end in new[] { a, b })
                    {
                        yield return end + side * radius;
                        yield return end - side * radius;
                        yield return end + other * radius;
                        yield return end - other * radius;
                    }

                    break;
            }
        }
#endif
        #endregion
    }
    #endregion
}
