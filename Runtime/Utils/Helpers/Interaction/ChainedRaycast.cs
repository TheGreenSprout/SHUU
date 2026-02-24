using System.Linq;
using UnityEngine;

namespace SHUU.Utils.Helpers.Interaction
{
    public class ChainedRaycast : InteractionRaycastLogic
    {
        [Header("Chained raycast Settings")]
        [SerializeField] private Camera renderTexture_cam;


        [SerializeField] private MeshRenderer rendererPlane;

        [SerializeField] private RenderTexture renderTexture;



        [SerializeField] protected float renderPlane_interactionRange;


        [SerializeField] protected LayerMask renderPlane_layerMask;

        [SerializeField] protected string[] renderPlane_tagMask = new string[0];


        [SerializeField] protected bool flipX = false;
        [SerializeField] protected bool flipY = false;




        protected override bool CastRay()
        {
            if (!cam || !renderTexture_cam || !rendererPlane || !renderTexture)
            {
                HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }



            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, renderPlane_interactionRange, renderPlane_layerMask) && (renderPlane_tagMask == null || renderPlane_tagMask.Length == 0 || renderPlane_tagMask.NonLINQ_Contains(hit.collider.tag)))
            {
                if (hit.collider.gameObject == rendererPlane.gameObject)
                {
                    Vector2 uv = hit.textureCoord;
                    Vector2 renderTexturePoint;

                    float x = flipX ? (1f - uv.x) : uv.x;
                    float y = flipY ? (1f - uv.y) : uv.y;
                    /*renderTexturePoint = new Vector2(x * renderTexture.width, y * renderTexture.height);*/
                    renderTexturePoint = new Vector2(x * renderTexture_cam.pixelWidth, y * renderTexture_cam.pixelHeight);


                    Ray renderTextureRay = renderTexture_cam.ScreenPointToRay(renderTexturePoint);
                    RaycastHit renderTextureHit;

                    if (Physics.Raycast(renderTextureRay, out renderTextureHit, interactionRange, layerMask) && renderTextureHit.InteractionRaycast_Check(out IfaceInteractable inact, tagMask))
                    {
                        if (previousInact != inact)
                        {
                            HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                            previousInact = inact;


                            inact.HoverStart(modifyDynamicCursor);
                        }
                    }
                    else
                    {
                        HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                        return false;
                    }
                }
                else
                {
                    HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                    return false;
                }
            }
            else
            {
                HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return false;
            }


            return true;
        }
    }
}
