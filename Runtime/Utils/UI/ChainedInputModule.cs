/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using SHUU.Utils.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SHUU.Utils.UI
{
    public class ChainedInputModule : StandaloneInputModule
    {
        #region Variables
        [Header("Settings")]
        [Tooltip("If true, normal UI works when not hovering render mesh.")]
        [SerializeField] private bool hybrid = true;

        [SerializeField] private bool releaseClickHoldOnLayerChange = true;

        [Tooltip("How long (seconds) external input stays valid without refresh.")]
        [SerializeField] private float externalTimeout = 0.05f;


        [Header("References")]
        [SerializeField] private GraphicRaycaster mainUICanvasRaycaster;
        [SerializeField] private GraphicRaycaster renderTextureRaycaster;


        [Header("Internal Raycast Settings")]
        [Tooltip("If true, module performs its own raycast. If false, waits for external injection.")]
        [SerializeField] private bool useInternalRaycast = true;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Collider renderPlaneCollider;
        [SerializeField] private LayerMask renderPlane_layerMask = -1;
        [SerializeField] private string[] renderPlane_tagMask;
        [SerializeField] private float internalRayDistance = 100f;

        [SerializeField] private bool flipX = false;
        [SerializeField] private bool flipY = false;



        private bool usingChainedInput;
        private Vector2 chainedMousePosition;


        private bool _raycastHit = false;
        private bool raycastHit
        {
            get => _raycastHit;
            set
            {
                if (_raycastHit != value) LayerChange(value);

                _raycastHit = value;
            }
        }


        private Vector2 externalPosition;
        private float lastExternalTime;

        private bool _blockMouseInputThisFrame = false;
        #endregion




        #region Variable methods
        private void LayerChange(bool active)
        {
            if (releaseClickHoldOnLayerChange) ForceReleaseAllMouseButtons();


            if (renderTextureRaycaster != null)
            {
                renderTextureRaycaster.enabled = active;

                if (!active && eventSystem != null) eventSystem.SetSelectedGameObject(null);
            }
        }
        #endregion



        protected override void Awake()
        {
            base.Awake();

            LayerChange(false);
        }



        public override void Process()
        {
            if (useInternalRaycast) InternalRaycast();
            

            if (raycastHit && (Time.unscaledTime - lastExternalTime > externalTimeout)) raycastHit = false;

            usingChainedInput = raycastHit;

            if (usingChainedInput) chainedMousePosition = externalPosition;


            if (!hybrid || !usingChainedInput) return;


            base.Process();


            _blockMouseInputThisFrame = false;
        }



        #region Raycast Logic
        private void SetRaycast(bool isValid, Vector2 uiPosition, bool internalCast)
        {
            if (internalCast && !useInternalRaycast) return;
            if (!internalCast && useInternalRaycast) return;


            if (isValid && BlockingUI())
            {
                raycastHit = false;

                return;
            }

            raycastHit = isValid;

            if (isValid)
            {
                externalPosition = uiPosition;
                lastExternalTime = Time.unscaledTime;
            }
        }

        private bool BlockingUI()
        {
            if (mainUICanvasRaycaster == null) return false;

            PointerEventData pointerData = new PointerEventData(eventSystem) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            mainUICanvasRaycaster.Raycast(pointerData, results);

            return results.Count > 0;
        }


        public void SetExternalRaycast(bool isValid, Vector2 uiPosition) => SetRaycast(isValid, uiPosition, false);

        protected virtual void InternalRaycast()
        {
            if (!useInternalRaycast) return;


            if (!mainCamera || !renderPlaneCollider || !renderTexture) SetRaycast(false, Vector2.zero, true);



            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, internalRayDistance, renderPlane_layerMask) && (renderPlane_tagMask == null || renderPlane_tagMask.Length == 0 || renderPlane_tagMask.NonLINQ_Contains(hit.collider.tag)))
            {
                if (hit.collider == renderPlaneCollider)
                {
                    Vector2 uv = hit.textureCoord;
                    Vector2 renderTexturePoint;

                    float x = flipX ? (1f - uv.x) : uv.x;
                    float y = flipY ? (1f - uv.y) : uv.y;

                    renderTexturePoint = new Vector2(x * renderTexture.width, y * renderTexture.height);


                    SetRaycast(true, renderTexturePoint, true);
                }
                else SetRaycast(false, Vector2.zero, true);
            }
            else SetRaycast(false, Vector2.zero, true);
        }
        #endregion



        #region UI pointer handling
        protected override MouseState GetMousePointerEventData(int id)
        {
            if (_blockMouseInputThisFrame)
            {
                var _mouseState = new MouseState();

                #region Left button
                PointerEventData _leftData;
                GetPointerData(kMouseLeftId, out _leftData, true);
                _leftData.Reset();
                _leftData.delta = Vector2.zero;
                _leftData.scrollDelta = Vector2.zero;
                _leftData.pointerPress = null;
                _leftData.rawPointerPress = null;
                _leftData.pointerDrag = null;
                _leftData.dragging = false;
                _leftData.eligibleForClick = false;
                _mouseState.SetButtonState(PointerEventData.InputButton.Left, PointerEventData.FramePressState.Released, _leftData);
                #endregion

                #region Right button
                PointerEventData _rightData;
                GetPointerData(kMouseRightId, out _rightData, true);
                _rightData.Reset();
                _rightData.delta = Vector2.zero;
                _rightData.scrollDelta = Vector2.zero;
                _rightData.pointerPress = null;
                _rightData.rawPointerPress = null;
                _rightData.pointerDrag = null;
                _rightData.dragging = false;
                _rightData.eligibleForClick = false;
                _mouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.Released, _rightData);
                #endregion

                #region Middle button
                PointerEventData _middleData;
                GetPointerData(kMouseMiddleId, out _middleData, true);
                _middleData.Reset();
                _middleData.delta = Vector2.zero;
                _middleData.scrollDelta = Vector2.zero;
                _middleData.pointerPress = null;
                _middleData.rawPointerPress = null;
                _middleData.pointerDrag = null;
                _middleData.dragging = false;
                _middleData.eligibleForClick = false;
                _mouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.Released, _middleData);
                #endregion
                

                _blockMouseInputThisFrame = false;
                return _mouseState;
            }


            if (!usingChainedInput) return base.GetMousePointerEventData(id);


            Vector2 position = chainedMousePosition;
            var mouseState = new MouseState();

            #region Left button
            PointerEventData leftData;
            GetPointerData(kMouseLeftId, out leftData, true);
            leftData.Reset();
            leftData.delta = position - leftData.position;
            leftData.position = position;
            leftData.scrollDelta = input.mouseScrollDelta;
            leftData.button = PointerEventData.InputButton.Left;

            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            leftData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            mouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
            #endregion

            #region Right button
            PointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);
            rightData.Reset();
            rightData.delta = position - rightData.position;
            rightData.position = position;
            rightData.scrollDelta = input.mouseScrollDelta;
            rightData.button = PointerEventData.InputButton.Right;

            eventSystem.RaycastAll(rightData, m_RaycastResultCache);
            rightData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            mouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            #endregion

            #region Middle button
            PointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);
            middleData.Reset();
            middleData.delta = position - middleData.position;
            middleData.position = position;
            middleData.scrollDelta = input.mouseScrollDelta;
            middleData.button = PointerEventData.InputButton.Middle;

            eventSystem.RaycastAll(middleData, m_RaycastResultCache);
            middleData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
            m_RaycastResultCache.Clear();
            mouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);
            #endregion


            return mouseState;
        }


        private void ForceReleaseAllMouseButtons()
        {
            ReleasePointer(kMouseLeftId);
            ReleasePointer(kMouseRightId);
            ReleasePointer(kMouseMiddleId);


            _blockMouseInputThisFrame = true;


            foreach (var pointerPair in m_PointerData)
            {
                var data = pointerPair.Value;

                if (data.pointerPress != null)
                {
                    var selectable = data.pointerPress.GetComponent<Selectable>();
                    if (selectable != null)
                    {
                        var dummyEventData = new PointerEventData(eventSystem)
                        {
                            pointerId = data.pointerId,
                            button = data.button,
                            position = data.position,
                            delta = Vector2.zero,
                            pressPosition = data.pressPosition,
                            pointerPress = data.pointerPress,
                            pointerDrag = data.pointerDrag
                        };

                        selectable.OnPointerUp(dummyEventData);

                        selectable.OnDeselect(dummyEventData);
                    }
                }
            }
        }

        private void ReleasePointer(int pointerId)
        {
            if (!GetPointerData(pointerId, out PointerEventData data, false)) return;

            if (data.pointerPress != null) ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

            if (data.pointerDrag != null && data.dragging) ExecuteEvents.Execute(data.pointerDrag, data, ExecuteEvents.endDragHandler);

            data.eligibleForClick = false;
            data.pointerPress = null;
            data.rawPointerPress = null;
            data.pointerDrag = null;
            data.dragging = false;
            data.useDragThreshold = true;

            HandlePointerExitAndEnter(data, null);
        }
        #endregion
    }
}
