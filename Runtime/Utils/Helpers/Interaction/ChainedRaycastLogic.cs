using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using SHUU.Utils.UI;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Helpers.Interaction
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
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, layerMask))
            {
                if (GetSurface(hit, out ChainedUISurface surface) && surface.detectionTagMask.Contains_Tag(hit.collider.tag))
                {
                    Vector2 uv = hit.textureCoord;

                    float x = surface.flipX ? (1f - uv.x) : uv.x;
                    float y = surface.flipY ? (1f - uv.y) : uv.y;

                    Vector2 pointerPos = new Vector2(x * surface.renderTexture.width, y * surface.renderTexture.height);

                    inChain = true;

                    chainedInputModule?.SetExternalRaycast(true, pointerPos, surface.raycaster);


                    Ray renderTextureRay = surface.renderCamera.ScreenPointToRay(pointerPos);
                    
                    if (!InteractionRaycast(ref previousInact, renderTextureRay, surface.interactionRange, surface.interactionLayer, modifyDynamicCursor, surface.interactionTagMask))
                    {
                        ClearInteractHover(ref previousInact, modifyDynamicCursor);

                        return false;
                    }
                }
                else
                {
                    inChain = false;

                    chainedInputModule?.SetExternalRaycast(false, Vector2.zero);


                    if (hit.InteractionRaycast_Check(out IfaceInteractable inact, tagMask))
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


        private bool CleanSurfaces()
        {
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

        public string[] detectionTagMask;

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
        public LayerMask interactionLayer;
        public string[] interactionTagMask;
    }
    #endregion
}
