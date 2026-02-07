using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class TimeController
    {
        public static bool paused { get; private set; }
        public static float currentTimeScale { get; private set; } = 1f;

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
