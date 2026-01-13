using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.UI
{
    public class Mouse_Cursor : MonoBehaviour
    {
        private Canvas canvas;

        private RectTransform canvasRect;
        private Camera canvasCam;


        private RectTransform rectTransform;



        [SerializeField] private bool manageCursorVisibility = true;




        private void Awake()
        {
            if (!canvas) canvas = transform.SearchComponent_InSelfAndParents<Canvas>();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError("OverlayCanvas_MouseCursor can't be used in a canvas that isn't RenderMode 'Screen Space - Overlay'.");

                Destroy(this);
            }


            rectTransform = GetComponent<RectTransform>();


            canvasRect = canvas.gameObject.GetComponent<RectTransform>();

            canvasCam = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) canvasCam = canvas.worldCamera;
        }


        private void OnEnable()
        {
            HandyFunctions.OnCursorStateChange += CursorStateChanged;



            if (manageCursorVisibility) HandyFunctions.ChangeCursorVisibility(false);
        }

        private void OnDisable()
        {
            HandyFunctions.OnCursorStateChange += CursorStateChanged;


            
            transform.localPosition = Vector3.zero;


            if (manageCursorVisibility) HandyFunctions.ChangeCursorVisibility(true);
        }

        private void CursorStateChanged(CursorLockMode state)
        {
            if (state == CursorLockMode.Locked) this.enabled = false;
            else this.enabled = true;
        }



        private void Update()
        {
            Vector2 mousePos = HandyFunctions.GetMouseScreenCoords(canvasRect, canvasCam);

            rectTransform.anchoredPosition = mousePos;
        }
    }
}
