using UnityEngine;
using System;
using System.Collections;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Globals
{
    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Manages the creation and behaviour of timers.
    /// </summary>
    #endregion
    public class SHUU_Time : Singleton_MonoBehaviour<SHUU_Time>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



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
        #endregion




        #region Main
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
        #endregion



        #region Logic
        
        #region Timers
        #region XML doc
        /// <summary>
        /// Creates a timer that, after the specified time, runs an Action.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        #endregion
        public static SHUU_Timer Timer(float seconds, Action onComplete, bool ignoreTimeScale = false)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to create timer. Wait until instance is created.");

                return null;
            }


            seconds = Mathf.Max(seconds, 0f);

            SHUU_Timer timer = new SHUU_Timer { remainingTime = seconds };
            timer.onComplete += onComplete;

            StartCoroutineStatic(Run(timer, ignoreTimeScale));

            return timer;
        }

        public static SHUU_Timer Timer(int frames, Action onComplete, bool ignoreTimeScale = false)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to create timer. Wait until instance is created.");

                return null;
            }


            frames = Mathf.Max(frames, 0);

            SHUU_Timer timer = new SHUU_Timer { remainingFrames = frames };
            timer.onComplete += onComplete;

            instance.StartCoroutine(RunFrames(timer, ignoreTimeScale));

            return timer;
        }


        #region XML doc
        /// <summary>
        /// Creates a Courtine, runs an Action and destroys itself when done.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        /// <returns>Returns the IEnumerator.</returns>
        #endregion
        private static IEnumerator Run(SHUU_Timer timer, bool ignoreTimeScale)
        {
            while (timer.remainingTime > 0f)
            {
                if (timer.isCancelled) yield break;

                if (!timer.isPaused)
                {
                    float delta = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

                    timer.remainingTime -= delta;
                }

                yield return null;
            }

            timer.Complete();
        }
        public static IEnumerator Run(float duration, Action onComplete, Func<float, IEnumerator> enumerator = null)
        {
            yield return enumerator != null ? enumerator(duration) : new WaitForSeconds(duration);

            onComplete?.Invoke();
        }

        private static IEnumerator RunFrames(SHUU_Timer timer, bool ignoreTimeScale)
        {
            while (timer.remainingFrames > 0)
            {
                if (timer.isCancelled) yield break;

                if (!timer.isPaused && (ignoreTimeScale || Time.timeScale > 0f)) timer.remainingFrames--;

                yield return null;
            }

            timer.Complete();
        }
        public static IEnumerator RunFrames(int frames, Action onComplete, bool ignoreTimeScale = false)
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


        public static bool Pause()
        {
            if (paused) return false;

            paused = true;
            Time.timeScale = 0f;

            return true;
        }

        public static bool Resume()
        {
            if (!paused) return false;

            paused = false;
            Time.timeScale = currentTimeScale;

            return true;
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
        
        #endregion
    }




    #region Helper class
    public class SHUU_Timer
    {
        #region Variables
        public bool isPaused { get; private set; }
        public bool isCancelled { get; private set; }
        public bool isCompleted { get; private set; }

        public bool isRunning => !isPaused && !isCancelled && !isCompleted;


        public float initialTime { get; internal set; }
        public int initialFrames { get; internal set; }

        public float remainingTime { get; internal set; }
        public int remainingFrames { get; internal set; }


        public float Progress01 => initialTime <= 0f ? 1f : 1f - (remainingTime / initialTime);
        public float FrameProgress01 => initialFrames <= 0 ? 1f : 1f - ((float)remainingFrames / initialFrames);



        public event Action onPaused;
        public event Action onResumed;
        public event Action onCancelled;

        public event Action onComplete;
        #endregion



        
        #region Logic
        public void Pause()
        {
            if (isCancelled || isCompleted) return;

            isPaused = true;
            onPaused?.Invoke();
        }

        public void Resume()
        {
            if (isCancelled || isCompleted) return;

            isPaused = false;
            onResumed?.Invoke();
        }

        public void Cancel()
        {
            if (isCompleted) return;

            isCancelled = true;
            onCancelled?.Invoke();
        }

        internal void Complete()
        {
            if (isCancelled || isCompleted) return;

            isCompleted = true;
            onComplete?.Invoke();
        }
        #endregion
    }
    #endregion
}
