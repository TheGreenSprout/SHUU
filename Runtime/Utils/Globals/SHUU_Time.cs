using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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



        public static bool Paused => PauseOwners.Count > 0;

        public static bool Frozen { get; private set; }

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

        private static bool Stepping;


        private static readonly HashSet<object> PauseOwners = new HashSet<object>();
        private static readonly object ManualPauseOwner = new object();
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
        }


        protected override void Awake()
        {
            base.Awake();

            PauseOwners.Clear();
            CurrentTimeScale = 1f;
            NextFrameQueue = null;
            ExecuteQueue = null;
            FreezeCoroutine = null;
            Frozen = false;
            Stepping = false;

            ApplyTimeScale();
        }


        private void Update()
        {
            if (PauseOwners.Count > 0 && PauseOwners.RemoveWhere(IsDestroyed) > 0) ApplyTimeScale();

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

            SHUU_Timer timer = new SHUU_Timer { initialTime = seconds, remainingTime = seconds };
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

            SHUU_Timer timer = new SHUU_Timer { initialFrames = frames, remainingFrames = frames };
            timer.onComplete += onComplete;

            Instance.StartCoroutine(RunFrames(timer, ignoreTimeScale));

            return timer;
        }

        #region XML doc
        /// <summary>
        /// Freezes the game for some frames (a frame is a frame, so this isn't stretched by the time scale).
        /// </summary>
        #endregion
        public static void FreezeFrame(int frames)
        {
            if (frames <= 0) return;

            StartFreeze(RunFreezeFrame(frames));
        }
        #region XML doc
        /// <summary>
        /// Freezes the game for some seconds. Unless ignoreTimeScale is true these are game seconds.
        /// </summary>
        #endregion
        public static void FreezeSeconds(float seconds, bool ignoreTimeScale = false)
        {
            if (seconds <= 0f) return;

            if (!ignoreTimeScale)
            {
                float scale = Paused ? 0f : CurrentTimeScale;
                if (scale <= 0f) return;

                seconds /= scale;
            }

            StartFreeze(RunFreezeSeconds(seconds));
        }
        private static void StartFreeze(IEnumerator freeze)
        {
            if (FreezeCoroutine != null) Instance?.StopCoroutine(FreezeCoroutine);

            Frozen = true;
            ApplyTimeScale();

            FreezeCoroutine = StartCoroutineStatic(freeze);

            // There was no SHUU_Time to run it.
            if (FreezeCoroutine == null) EndFreeze();
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

        private static IEnumerator RunFreezeFrame(int frames)
        {
            for (int i = 0; i < frames; i++) yield return null;

            EndFreeze();
        }
        private static IEnumerator RunFreezeSeconds(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);

            EndFreeze();
        }
        private static void EndFreeze()
        {
            FreezeCoroutine = null;
            Frozen = false;

            ApplyTimeScale();
        }
        #endregion



        #region Time scale
        public static void SetTimeScale(float scale)
        {
            CurrentTimeScale = Mathf.Max(scale, 0f);

            ApplyTimeScale();
        }


        #region Pause
        #region XML doc
        /// <summary>
        /// Pauses the game on behalf of an owner (a component, a name...). The game stays paused until every owner that paused it has resumed, so systems can't unpause each other.
        /// A component that gets destroyed while pausing stops pausing by itself.
        /// </summary>
        /// <param name="owner">Whatever is pausing. Pass the same object to Resume.</param>
        /// <returns>False if this owner was already pausing.</returns>
        #endregion
        public static bool Pause(object owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            if (!PauseOwners.Add(owner)) return false;

            ApplyTimeScale();

            return true;
        }

        #region XML doc
        /// <summary>
        /// Takes back an owner's pause. The game only resumes once no owner is pausing.
        /// </summary>
        /// <returns>False if this owner wasn't pausing.</returns>
        #endregion
        public static bool Resume(object owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            if (!PauseOwners.Remove(owner)) return false;

            ApplyTimeScale();

            return true;
        }

        public static bool IsPausedBy(object owner) => owner != null && PauseOwners.Contains(owner);

        #region XML doc
        /// <summary>
        /// Takes back every owner's pause at once, for when something forgot to resume.
        /// </summary>
        #endregion
        public static void ResumeAll()
        {
            PauseOwners.Clear();

            ApplyTimeScale();
        }

        #region XML doc
        /// <summary>
        /// Who is pausing the game right now (for debugging a game that stays paused).
        /// </summary>
        #endregion
        public static string[] GetPauseOwners()
        {
            List<string> names = new List<string>();

            foreach (object owner in PauseOwners)
            {
                if (IsDestroyed(owner)) continue;

                if (owner == ManualPauseOwner) names.Add("Manual pause (the pause command)");
                else if (owner is UnityEngine.Object unityObject) names.Add($"{unityObject.name} ({unityObject.GetType().Name})");
                else names.Add(owner.ToString());
            }

            return names.ToArray();
        }


        #region XML doc
        /// <summary>
        /// The pause that has no owner of its own (used by the dev console). Other pauses are unaffected by it.
        /// </summary>
        #endregion
        public static bool Pause() => Pause(ManualPauseOwner);

        public static bool Resume() => Resume(ManualPauseOwner);

        public static bool TogglePause()
        {
            if (IsPausedBy(ManualPauseOwner)) Resume();
            else Pause();

            return Paused;
        }
        #endregion


        #region XML doc
        /// <summary>
        /// While paused, lets exactly one frame run and then pauses again (physics only advances if a fixed timestep falls inside that frame).
        /// </summary>
        /// <returns>False if there was nothing to step (the game isn't paused, or a step is already running).</returns>
        #endregion
        public static bool StepFrame()
        {
            if (!Paused || Stepping) return false;

            Stepping = true;

            if (StartCoroutineStatic(RunStepFrame()) != null) return true;

            Stepping = false;

            return false;
        }

        private static IEnumerator RunStepFrame()
        {
            ApplyTimeScale();

            yield return null;

            Stepping = false;

            ApplyTimeScale();
        }
        #endregion
    
    

        #region Helpers
        private static void ApplyTimeScale() => Time.timeScale = ((Paused && !Stepping) || Frozen) ? 0f : CurrentTimeScale;

        private static bool IsDestroyed(object owner) => owner is UnityEngine.Object unityObject && unityObject == null;


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
