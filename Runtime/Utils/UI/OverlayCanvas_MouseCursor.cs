using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.UI
{
    public class Mouse_Cursor : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private RectTransform canvasRect;
        private Camera canvasCam;


        private RectTransform rectTransform;




        void Start()
        {
            rectTransform = GetComponent<RectTransform>();


            canvasRect = canvas.gameObject.GetComponent<RectTransform>();

            canvasCam = null;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) canvasCam = canvas.worldCamera;
        }


        private void OnDisable()
        {
            transform.localPosition = Vector3.zero;
        }


        void Update()
        {
            Vector2 mousePos = HandyFunctions.GetMouseScreenCoords(canvasRect, canvasCam);

            rectTransform.anchoredPosition = mousePos;
        }
    }
}
