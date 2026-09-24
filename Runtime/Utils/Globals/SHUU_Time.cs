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
    public class SHUU_Time : HiddenSingleton_MonoBehaviour<SHUU_Time>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        public static bool Paused { get; private set; }

        public static float CurrentTimeScale { get; private set; } = 1f;



        private static Action NextFrameQueue;
        private static Action ExecuteQueue;

        public static event Action OnNextFrame
        {
            add => NextFrameQueue += value;
            remove => NextFrameQueue -= value;
        }


        public event Action onUpdate_Local;
        public static event Action OnUpdate;

        public event Action onLateUpdate_Local;
        public static event Action OnLateUpdate;

        public event Action onFixedUpdate_Local;
        public static event Action OnFixedUpdate;



        private static Coroutine FreezeCoroutine;
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            Paused = false;
            CurrentTimeScale = 1f;
            NextFrameQueue = null;
            ExecuteQueue = null;
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
            FreezeCoroutine = null;
        }


        private void Update()
        {
            OnUpdate?.Invoke();
            onUpdate_Local?.Invoke();

            if (ExecuteQueue != null)
            {
                var callback = ExecuteQueue;
                ExecuteQueue = null;
                callback.Invoke();
            }
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
            onLateUpdate_Local?.Invoke();
            
            if (NextFrameQueue == null) return;

            ExecuteQueue = NextFrameQueue;
            NextFrameQueue = null;
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
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
            if (seconds <= 0) return null;

            if (Instance == null)
            {
                Debug.LogError("No SHUU_Time Instance found in the scene. Unable to create timer. Wait until Instance is created.");

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
            if (frames <= 0) return null;

            if (Instance == null)
            {
                Debug.LogError("No SHUU_Time Instance found in the scene. Unable to create timer. Wait until Instance is created.");

                return null;
            }


            frames = Mathf.Max(frames, 0);

            SHUU_Timer timer = new SHUU_Timer { remainingFrames = frames };
            timer.onComplete += onComplete;

            Instance.StartCoroutine(RunFrames(timer, ignoreTimeScale));

            return timer;
        }

        public static void FreezeFrame(float duration, bool ignoreTimeScale = false)
        {
            if (FreezeCoroutine != null) Instance.StopCoroutine(FreezeCoroutine);

            FreezeCoroutine = StartCoroutineStatic(RunFreezeFrame(duration, ignoreTimeScale));
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

        private static IEnumerator RunFreezeFrame(float duration, bool ignoreTimeScale)
        {
            float previousTimeScale = CurrentTimeScale;

            Time.timeScale = 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }

            FreezeCoroutine = null;

            if (!Paused) Time.timeScale = previousTimeScale;
        }
        #endregion



        #region Time scale
        public static void SetTimeScale(float scale)
        {
            CurrentTimeScale = Mathf.Max(scale, 0f);
            if (!Paused) Time.timeScale = CurrentTimeScale;
        }


        public static bool Pause()
        {
            if (Paused) return false;

            Paused = true;
            Time.timeScale = 0f;

            return true;
        }

        public static bool Resume()
        {
            if (!Paused) return false;

            Paused = false;
            Time.timeScale = CurrentTimeScale;

            return true;
        }

        public static bool TogglePause()
        {
            if (Paused) Resume();
            else Pause();

            return Paused;
        }


        public static void StepFrame() => FreezeFrame(Time.fixedDeltaTime);
        #endregion
    
    

        #region Helpers
        public static Coroutine StartCoroutineStatic(IEnumerator routine)
        {
            if (Instance == null)
            {
                Debug.LogError("No SHUU_Time Instance found in the scene. Unable to start coroutine. Wait until Instance is created.");

                return null;
            }

            return Instance.StartCoroutine(routine);
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


        public void Cancel(bool invokeComplete = false)
        {
            if (isCancelled || isCompleted) return;

            isCancelled = true;
            onCancelled?.Invoke();
            if (invokeComplete) onComplete?.Invoke();
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
