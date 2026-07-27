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
        protected override bool CastRay()
        {
            if (!cam || !CleanSurfaces())
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }

            if (chainedInputModule != null && chainedInputModule.BlockingUI())
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                chainedInputModule.SetExternalRaycast(false, Vector2.zero);

                return false;
            }


            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            RaycastHit? hit = GetFirstHit(ray);

            if (hit.HasValue)
            {
                if (GetSurface(hit.Value, out ChainedUISurface surface) && surface.detectionTagMask.Contains(hit.Value.collider.tag))
                {
                    Vector2 uv = hit.Value.textureCoord;

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
                }
                else
                {
                    inChain = false;

                    chainedInputModule?.SetExternalRaycast(false, Vector2.zero);


                    if (hit.Value.InteractionRaycast_Check(out IfaceInteractable inact, tagMask))
                    {
                        if (previousInact != inact)
                        {
                            ClearInteractHover(ref previousInact, modifyDynamicCursor);

                            previousInact = inact;


                            inact.HoverStart(modifyDynamicCursor);
                        }
                    }
                    else
                    {
                        ClearInteractHover(ref previousInact, modifyDynamicCursor);
                    
                        return false;
                    }
                }
            }
            else
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                chainedInputModule?.SetExternalRaycast(false, Vector2.zero);

                return false;
            }


            return true;
        }

        private RaycastHit? GetFirstHit(Ray ray)
        {
            if (tagMaskPenetrate)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, interactionRange, layerMask);
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    if (GetSurface(hit, out _)) return hit;

                    if (tagMask.Contains(hit.collider.tag)) return hit;
                }

                return null;
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, layerMask)) return hit;

                return null;
            }
        }


        private bool CleanSurfaces()
        {
            if (surfaces == null || surfaces.Count == 0) return true;
            
            List<ChainedUISurface> ret = new();

            foreach (var surface in surfaces)
                if (surface.renderCamera != null && surface.rendererPlane != null && surface.renderTexture != null) ret.Add(surface);

            return ret.Count != 0;
        }

        private bool GetSurface(RaycastHit hit, out ChainedUISurface surface)
        {
            surface = null;

            foreach (var s in surfaces)
            {
                if (s.rendererPlane == null || hit.collider.gameObject != s.rendererPlane.gameObject) continue;

                surface = s;
                return true;
            }

            return false;
        }
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
