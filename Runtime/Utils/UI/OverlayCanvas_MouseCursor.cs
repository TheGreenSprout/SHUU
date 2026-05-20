using UnityEngine;

using SHUU.Utils.Helpers;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.UI
{
    public class OverlayCanvas_MouseCursor : MonoBehaviour
    {
        #region Variables
        private Canvas canvas;
        private RectTransform canvasRect;
        private Camera canvasCam;

        private RectTransform rectTransform;


        [SerializeField] private bool manageCursorVisibility = true;



        private static bool debugLogEmission => SHUU_Preferences.instance.ui_debugLogEmission;
        #endregion




        #region Main
        private void Awake()
        {
            if (!canvas) canvas = transform.SearchComponent_InSelfAndParents<Canvas>();
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                if (debugLogEmission) Debug.LogError("OverlayCanvas_MouseCursor can't be used in a canvas that isn't RenderMode 'Screen Space - Overlay'.");

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


            if (manageCursorVisibility) HandyFunctions.ChangeMouseVisibility(false);
        }

        private void OnDisable()
        {
            HandyFunctions.OnCursorStateChange += CursorStateChanged;


            transform.localPosition = Vector3.zero;

            if (manageCursorVisibility) HandyFunctions.ChangeMouseVisibility(true);
        }


        private void Update()
        {
            Vector2 mousePos = HandyFunctions.GetMouseScreenCoords(canvasRect, canvasCam);

            rectTransform.anchoredPosition = mousePos;
        }
        #endregion


        
        #region Logic
        private void CursorStateChanged(CursorLockMode state)
        {
            if (state == CursorLockMode.Locked) this.enabled = false;
            else this.enabled = true;
        }
        #endregion
    }
}
