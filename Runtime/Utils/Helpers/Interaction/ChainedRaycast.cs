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


        [SerializeField] protected bool flipY = false;




        protected override void CastRay()
        {
            if (!cam || !renderTexture_cam || !rendererPlane || !renderTexture)
            {
                HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);

                return;
            }



            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, renderPlane_interactionRange, renderPlane_layerMask) && (renderPlane_tagMask == null || renderPlane_tagMask.Length == 0 || renderPlane_tagMask.NonLINQ_Contains(hit.collider.tag)))
            {
                if (hit.collider.gameObject == rendererPlane.gameObject)
                {
                    Vector2 uv = hit.textureCoord;

                    Vector2 renderTexturePoint;
                    /*if (flipY) renderTexturePoint = new Vector2(uv.x * renderTexture.width, (1f - uv.y) * renderTexture.height);
                    else  renderTexturePoint = new Vector2(uv.x * renderTexture.width, uv.y * renderTexture.height);*/
                    if (flipY) renderTexturePoint = new Vector2(uv.x * renderTexture_cam.pixelWidth, (1f - uv.y) * renderTexture_cam.pixelHeight);
                    else renderTexturePoint = new Vector2(uv.x * renderTexture_cam.pixelWidth, uv.y * renderTexture_cam.pixelHeight);


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
                    else HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);
                }
                else HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);
            }
            else HandyFunctions.ClearInteractHover(ref previousInact, modifyDynamicCursor);
        }
    }
}
