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




        #region XML doc
        /// <summary>
        /// Creates a timer that, after the specified time, runs an Action.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        #endregion
        public static void Timer(float duration, Action onComplete)
        {
            if (instance == null)
            {
                Debug.LogError("No SHUU_Time instance found in the scene. Unable to create timer. Wait until instance is created.");

                return;
            }

            instance.StartCoroutine(Run(duration, onComplete));
        }

        #region XML doc
        /// <summary>
        /// Creates a Courtine, runs an Action and destroys itself when done.
        /// </summary>
        /// <param name="duration">The time the Action will be delayed by.</param>
        /// <param name="onComplete">The Action that will be performed.</param>
        /// <returns>Returns the IEnumerator.</returns>
        #endregion
        private static IEnumerator Run(float duration, Action onComplete)
        {
            yield return new WaitForSeconds(duration);

            onComplete?.Invoke();
        }


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
    }
    
}
