using UnityEngine;

using SHUU.Utils.UI;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Helpers.Interaction
{
    public class ChainedRaycast : InteractionRaycastLogic
    {
        #region Variables
        [Header("Chained Raycast Settings")]
        [SerializeField] private Camera renderTexture_cam;


        [SerializeField] private MeshRenderer rendererPlane;

        [SerializeField] private RenderTexture renderTexture;



        [SerializeField] protected float chained_interactionRange;


        [SerializeField] protected LayerMask chained_layerMask;

        [SerializeField] protected string[] chained_tagMask = new string[0];


        [SerializeField] protected bool flipX = false;
        [SerializeField] protected bool flipY = false;



        [Tooltip("If not null, the input module will use this raycast.")]
        [SerializeField] protected ChainedInputModule chainedInputModule;



        protected bool inChain = false;
        #endregion




        #region Logic
        protected override bool CastRay()
        {
            if (!cam || !renderTexture_cam || !rendererPlane || !renderTexture)
            {
                ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }


            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, layerMask))
            {
                if ((tagMask == null || tagMask.Length == 0 || tagMask.NonLINQ_Contains(hit.collider.tag)) && hit.collider.gameObject == rendererPlane.gameObject)
                {
                    Vector2 uv = hit.textureCoord;
                    Vector2 renderTexturePoint;

                    float x = flipX ? (1f - uv.x) : uv.x;
                    float y = flipY ? (1f - uv.y) : uv.y;

                    renderTexturePoint = new Vector2(x * renderTexture.width, y * renderTexture.height);


                    inChain = true;

                    chainedInputModule?.SetExternalRaycast(true, renderTexturePoint);


                    Ray renderTextureRay = renderTexture_cam.ScreenPointToRay(renderTexturePoint);
                    
                    if (!InteractionRaycast(ref previousInact, renderTextureRay, interactionRange, layerMask, modifyDynamicCursor, tagMask))
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
        #endregion
    }
}
