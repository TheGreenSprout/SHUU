/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

using static SHUU.Utils.Helpers.HandyFunctions;

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
        [SerializeField] private List<GraphicRaycaster> externalCanvasRaycasters;
        private GraphicRaycaster activeCanvasRaycaster;



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




        #region Main
        protected override void Awake()
        {
            base.Awake();

            externalCanvasRaycasters.Clean();
            LayerChange(false);
        }


        public override void Process()
        {
            if (raycastHit && (Time.unscaledTime - lastExternalTime > externalTimeout)) raycastHit = false;

            usingChainedInput = raycastHit;

            if (usingChainedInput) chainedMousePosition = externalPosition;


            if (!usingChainedInput && !hybrid) return;


            base.Process();

            if (Input.GetMouseButtonUp(0) && EventSystem.current != null)
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;

                if (selected == null || selected.GetComponent<InputField>() == null && selected.GetComponent<TMP_InputField>() == null)
                    EventSystem.current.SetSelectedGameObject(null);
            }

            _blockMouseInputThisFrame = false;
        }
        #endregion



        #region Logic

        #region Raycast Logic
        private void SetRaycast(bool isValid, Vector2 uiPosition)
        {
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
        
        public bool BlockingUI()
        {
            PointerEventData pointerData = new PointerEventData(eventSystem) { position = Input.mousePosition };

            var results = new List<RaycastResult>();

            foreach (var raycaster in externalCanvasRaycasters)
            {
                if (raycaster == null) continue;

                raycaster.Raycast(pointerData, results);
            }

            return results.Count > 0;
        }


        public void SetExternalRaycast(bool isValid, Vector2 uiPosition, GraphicRaycaster raycaster = null)
        {
            if (!isValid || raycaster == null)
            {
                ClearExternalRaycast();
                return;
            }


            if (activeCanvasRaycaster != raycaster) ForceReleaseAllMouseButtons();

            activeCanvasRaycaster = raycaster;

            SetRaycast(true, uiPosition);
        }

        public void ClearExternalRaycast()
        {
            SetRaycast(false, Vector2.zero);

            activeCanvasRaycaster = null;
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

            Vector2 previousLeftPos = leftData.position;

            leftData.position = position;
            leftData.delta = position - previousLeftPos;
            leftData.scrollDelta = input.mouseScrollDelta;
            leftData.button = PointerEventData.InputButton.Left;

            m_RaycastResultCache.Clear();

            if (activeCanvasRaycaster != null) activeCanvasRaycaster.Raycast(leftData, m_RaycastResultCache);

            leftData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

            GameObject currentOverGo = leftData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(leftData, currentOverGo);

            m_RaycastResultCache.Clear();

            mouseState.SetButtonState(
                PointerEventData.InputButton.Left,
                StateForMouseButton(0),
                leftData
            );
            #endregion

            #region Right button
            PointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);

            Vector2 previousRightPos = rightData.position;

            rightData.position = position;
            rightData.delta = position - previousRightPos;
            rightData.scrollDelta = input.mouseScrollDelta;
            rightData.button = PointerEventData.InputButton.Right;

            m_RaycastResultCache.Clear();

            if (activeCanvasRaycaster != null)activeCanvasRaycaster.Raycast(rightData, m_RaycastResultCache);

            rightData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

            currentOverGo = rightData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(rightData, currentOverGo);

            m_RaycastResultCache.Clear();

            mouseState.SetButtonState(
                PointerEventData.InputButton.Right,
                StateForMouseButton(1),
                rightData
            );
            #endregion

            #region Middle button
            PointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);

            Vector2 previousMiddlePos = middleData.position;

            middleData.position = position;
            middleData.delta = position - previousMiddlePos;
            middleData.scrollDelta = input.mouseScrollDelta;
            middleData.button = PointerEventData.InputButton.Middle;

            m_RaycastResultCache.Clear();

            if (activeCanvasRaycaster != null)activeCanvasRaycaster.Raycast(middleData, m_RaycastResultCache);

            middleData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

            currentOverGo = middleData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(middleData, currentOverGo);

            m_RaycastResultCache.Clear();

            mouseState.SetButtonState(
                PointerEventData.InputButton.Middle,
                StateForMouseButton(2),
                middleData
            );
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



        #region Misc
        private void LayerChange(bool active)
        {
            if (releaseClickHoldOnLayerChange) ForceReleaseAllMouseButtons();

            if (activeCanvasRaycaster != null)
            {
                activeCanvasRaycaster.enabled = active;

                if (!active && eventSystem != null) eventSystem.SetSelectedGameObject(null);
            }
        }
        #endregion

        #endregion
    }
}
