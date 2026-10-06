using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using SHUU.Utils.UI;
using SHUU.Utils.Helpers;

using static SHUU.Utils.Interaction.Interaction_SubClasses;

namespace SHUU.Utils.Interaction
{
    public abstract class ChainedRaycastLogic : InteractionRaycastLogic
    {
        #region Variables
        [Header("Chained Raycast Settings")]
        [SerializeField] private List<ChainedUISurface> surfaces;


        [Tooltip("If not null, the input module will use this raycast.")]
        [SerializeField] protected ChainedInputModule chainedInputModule;



        protected bool inChain = false;



        private const int MaxPenetrateHits = 64;
        private static readonly RaycastHit[] HitBuffer = new RaycastHit[MaxPenetrateHits];
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            if (chainedInputModule == null) chainedInputModule = EventSystem.current.gameObject.GetComponent<ChainedInputModule>();

            foreach (var surface in surfaces)
                if (surface.raycaster != null) surface.raycaster.enabled = false;
        }
        #endregion



        #region Logic
        private void ExitChain()
        {
            if (!inChain) return;
            inChain = false;

            chainedInputModule?.SetExternalRaycast(false, Vector2.zero);
        }

        private bool GetFirstHit(Ray ray, out RaycastHit result)
        {
            result = default;

            if (!tagMaskPenetrate) return Physics.Raycast(ray, out result, interactionRange, layerMask);


            int count = Physics.RaycastNonAlloc(ray, HitBuffer, interactionRange, layerMask);

            bool found = false;
            float closest = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = HitBuffer[i];

                if (hit.distance >= closest) continue;
                if (!GetSurface(hit, out _) && !tagMask.Contains(hit.collider.tag)) continue;

                closest = hit.distance;
                result = hit;
                found = true;
            }

            return found;
        }


        #region Surfaces
        private bool AnySurfaceUsable()
        {
            if (surfaces == null) return false;

            for (int i = 0; i < surfaces.Count; i++)
                if (IsUsable(surfaces[i])) return true;

            return false;
        }

        private static bool IsUsable(ChainedUISurface s)
            => s != null && s.renderCamera != null && s.renderTexture != null && s.rendererPlane != null && s.rendererPlane.gameObject.activeInHierarchy;

        private bool GetSurface(RaycastHit hit, out ChainedUISurface surface)
        {
            GameObject hitObject = hit.collider.gameObject;

            for (int i = 0; i < surfaces.Count; i++)
            {
                ChainedUISurface s = surfaces[i];

                if (!IsUsable(s) || hitObject != s.rendererPlane.gameObject) continue;

                surface = s;
                return true;
            }

            surface = null;
            return false;
        }
        #endregion

        #endregion



        #region Override points
        protected override bool SupportsCastShapes => false;


        protected override bool CastRay()
        {
            if (!cam)
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);
                ExitChain();

                return false;
            }

            if (chainedInputModule != null && chainedInputModule.BlockingUI())
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);
                ExitChain();

                return false;
            }


            if (!AnySurfaceUsable())
            {
                ExitChain();

                return base.CastRay();
            }


            Ray ray = cam.ScreenPointToRay(PointerPosition());

            if (!GetFirstHit(ray, out RaycastHit hit))
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);
                ExitChain();

                return false;
            }


            if (GetSurface(hit, out ChainedUISurface surface) && surface.detectionTagMask.Contains(hit.collider.tag))
            {
                Vector2 uv = hit.textureCoord;

                float x = surface.flipX ? (1f - uv.x) : uv.x;
                float y = surface.flipY ? (1f - uv.y) : uv.y;

                Vector2 pointerPos = new Vector2(x * surface.renderTexture.width, y * surface.renderTexture.height);

                inChain = true;

                chainedInputModule?.SetExternalRaycast(true, pointerPos, surface.raycaster);


                Ray renderTextureRay = surface.renderCamera.ScreenPointToRay(pointerPos);

                if (!InteractionRaycast(ref previousInact, renderTextureRay, surface.interactionRange, surface.layerMask, surface.tagMask, surface.tagMaskPenetrate, surface.modifyDynamicCursor))
                {
                    ClearInteractHover(ref previousInact, surface.modifyDynamicCursor);

                    return false;
                }

                return true;
            }


            ExitChain();

            if (!hit.InteractionRaycast_Check(out IfaceInteractable inact, tagMask))
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }

            if (previousInact != inact)
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                previousInact = inact;

                inact.HoverStart(modifyDynamicCursor);
            }

            return true;
        }
        #endregion



        #region Gizmos
#if UNITY_EDITOR
        private static readonly Color[] SurfaceColors = { SHUU_Gizmos.Purple, SHUU_Gizmos.Yellow, SHUU_Gizmos.Green, SHUU_Gizmos.Orange };


        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (surfaces == null) return;

            for (int i = 0; i < surfaces.Count; i++)
            {
                ChainedUISurface surface = surfaces[i];

                if (surface == null || surface.renderCamera == null) continue;

                Color color = SurfaceColors[i % SurfaceColors.Length];

                if (surface.rendererPlane != null)
                {
                    Bounds bounds = surface.rendererPlane.bounds;

                    Gizmos.color = color.WithAlpha(0.6f);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }

                DrawInteractionRay(SurfaceRay(surface), surface.interactionRange, surface.layerMask, color);
            }
        }

        private Ray SurfaceRay(ChainedUISurface surface)
        {
            if (Application.isPlaying && inChain && cam != null && surface.renderTexture != null
                && Physics.Raycast(cam.ScreenPointToRay(PointerPosition()), out RaycastHit hit, interactionRange, layerMask)
                && GetSurface(hit, out ChainedUISurface hitSurface) && hitSurface == surface)
            {
                float x = surface.flipX ? 1f - hit.textureCoord.x : hit.textureCoord.x;
                float y = surface.flipY ? 1f - hit.textureCoord.y : hit.textureCoord.y;

                return surface.renderCamera.ScreenPointToRay(new Vector2(x * surface.renderTexture.width, y * surface.renderTexture.height));
            }

            return surface.renderCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
#endif
        #endregion
    }




    #region Helper class
    [Serializable]
    public class ChainedUISurface
    {
        [Header("World Detection")]
        public MeshRenderer rendererPlane;
        public TagMask detectionTagMask = TagMask.Everything;


        [Header("Render Texture")]
        public Camera renderCamera;
        public RenderTexture renderTexture;


        [Header("UI")]
        public GraphicRaycaster raycaster;


        [Header("Options")]
        public bool flipX;
        public bool flipY;


        [Header("3D Interaction")]
        public float interactionRange = 100f;
        public LayerMask layerMask = ~0;
        public TagMask tagMask = TagMask.Everything;
        public bool modifyDynamicCursor = false;
        public bool tagMaskPenetrate = false;
    }
    #endregion
}
