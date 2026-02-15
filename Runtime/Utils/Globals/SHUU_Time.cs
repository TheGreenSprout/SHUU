using UnityEngine;
using System;
using SHUU.Utils.Helpers;
using System.Collections;

namespace SHUU.Utils.Globals
{
    
    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Manages the creation and behaviour of timers.
    /// </summary>
    #endregion
    public class SHUU_Time : StaticInstance_Monobehaviour<SHUU_Time>
    {
        public static bool paused { get; private set; }

        public static float currentTimeScale { get; private set; } = 1f;



        private static Action _nextFrameQueue;
        private static Action _executeQueue;

        public static event Action onNextFrame
        {
            add => _nextFrameQueue += value;
            remove => _nextFrameQueue -= value;
        }


        public event Action onUpdate_Local;
        public static event Action onUpdate;

        public event Action onLateUpdate_Local;
        public static event Action onLateUpdate;

        public event Action onFixedUpdate_Local;
        public static event Action onFixedUpdate;




        private void Update()
        {
            onUpdate?.Invoke();
            onUpdate_Local?.Invoke();


            if (_executeQueue != null)
            {
                var callback = _executeQueue;
                _executeQueue = null;
                callback.Invoke();
            }
        }

        private void LateUpdate()
        {
            onLateUpdate?.Invoke();
            onLateUpdate_Local?.Invoke();

            
            if (_nextFrameQueue == null) return;

            _executeQueue = _nextFrameQueue;
            _nextFrameQueue = null;
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
            onFixedUpdate_Local?.Invoke();
        }



        #region Timers
        #region XML doc
        /// <summary>
        /// Creates a timer that, after the specified time, runs an Action.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        #endregion
        public static void Timer(float seconds, Action onComplete, bool ignoreTimeScale = false)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to create timer. Wait until instance is created.");

                return;
            }


            seconds = Mathf.Max(seconds, 0f);

            if (!ignoreTimeScale) instance.StartCoroutine(Run(seconds, onComplete));
            else instance.StartCoroutine(Run(seconds, onComplete, x => new WaitForSecondsRealtime(x)));
        }

        public static void Timer(int frames, Action onComplete, bool ignoreTimeScale = false)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to create timer. Wait until instance is created.");

                return;
            }


            frames = Mathf.Max(frames, 0);
            
            instance.StartCoroutine(RunFrames(frames, onComplete, ignoreTimeScale));
        }


        #region XML doc
        /// <summary>
        /// Creates a Courtine, runs an Action and destroys itself when done.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        /// <returns>Returns the IEnumerator.</returns>
        #endregion
        private static IEnumerator Run(float duration, Action onComplete, Func<float, IEnumerator> enumerator = null)
        {
            yield return enumerator != null ? enumerator(duration) : new WaitForSeconds(duration);

            onComplete?.Invoke();
        }

        private static IEnumerator RunFrames(int frames, Action onComplete, bool ignoreTimeScale = false)
        {
            while (frames > 0)
            {
                if (ignoreTimeScale || Time.timeScale > 0f) frames--;
                yield return null;
            }

            onComplete?.Invoke();
        }
        #endregion


        #region Time scale
        public static void SetTimeScale(float scale)
        {
            currentTimeScale = Mathf.Max(scale, 0f);
            if (!paused) Time.timeScale = currentTimeScale;
        }

        public static void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
        }

        public static void Resume()
        {
            paused = false;
            Time.timeScale = currentTimeScale;
        }

        public static bool TogglePause()
        {
            if (paused) Resume();
            else Pause();

            return paused;
        }

        public static void StepFrame()
        {
            Time.timeScale = 0f;
            Time.captureFramerate = 0;
        }
        #endregion
    
    
        #region Helpers
        public static Coroutine StartCoroutineStatic(IEnumerator routine)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to start coroutine. Wait until instance is created.");

                return null;
            }

            return instance.StartCoroutine(routine);
        }
        #endregion
    }
    
}
