using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SHUU.Utils.InputSystem
{
    #region Enums
    public enum CompositeBindPart
    {
        binding,
        modifier,
        modifier1,
        modifier2
    }
    #endregion




    #region BufferedInputs
    public class BufferedInput
    {
        #region Variables
        public readonly float bufferDuration;
        public readonly bool ignoreTimeScale;

        public float remainingTime;


        public bool isActive => remainingTime > 0f;
        #endregion



        #region Main
        public BufferedInput(float bufferDuration, bool ignoreTimeScale)
        {
            this.bufferDuration = bufferDuration;
            this.ignoreTimeScale = ignoreTimeScale;
            remainingTime = 0f;
        }
        #endregion

        
        #region Logic
        public void ResetBuffer() => remainingTime = bufferDuration;

        public void Consume() => remainingTime = 0f;
        #endregion
    }
    #endregion




    #region ActionHooks
    public class ActionHooks
    {
        #region Variables
        public readonly InputAction action;

        public BufferedInput downBuffer;
        public BufferedInput upBuffer;

        public readonly List<Action> downListeners = new();
        public readonly List<Action> upListeners = new();

        private bool subscribed;


        public bool hasAnyHooks => downBuffer != null || upBuffer != null || downListeners.Count > 0 || upListeners.Count > 0;
        #endregion



        #region Main
        public ActionHooks(InputAction action) => this.action = action;
        
        public void Tick()
        {
            if (downBuffer != null && downBuffer.remainingTime > 0f)
            {
                float delta = downBuffer.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                downBuffer.remainingTime -= delta;
            }

            if (upBuffer != null && upBuffer.remainingTime > 0f)
            {
                float delta = upBuffer.ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                upBuffer.remainingTime -= delta;
            }
        }
        #endregion


        #region Logic
        public void EnsureSubscribed()
        {
            if (subscribed) return;
            subscribed = true;

            action.performed += OnPerformed;
            action.canceled += OnCanceled;
        }
        public void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;

            action.performed -= OnPerformed;
            action.canceled -= OnCanceled;
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            downBuffer?.ResetBuffer();

            foreach (var listener in downListeners)
                listener?.Invoke();
        }
        private void OnCanceled(InputAction.CallbackContext ctx)
        {
            upBuffer?.ResetBuffer();

            foreach (var listener in upListeners)
                listener?.Invoke();
        }
        #endregion
    }
    #endregion
}
