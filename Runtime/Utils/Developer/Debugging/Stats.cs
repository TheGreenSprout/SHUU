using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace SHUU.Utils.Developer.Debugging
{
    public class Stats : MonoBehaviour
    {
        #region Variables
        private static float FpsTimer;
        private static int FrameCount;
        private static float CurrentFps;
        #endregion




        #region Main
        public void Update()
        {
            FrameCount++;
            FpsTimer += Time.unscaledDeltaTime;

            if (FpsTimer >= 0.5f)
            {
                CurrentFps = FrameCount / FpsTimer;
                FpsTimer = 0f;
                FrameCount = 0;
            }
        }
        #endregion



        #region Logic
        
        #region Frames
        public static float Fps => CurrentFps;
        public static float Frametimems => CurrentFps > 0 ? 1000f / CurrentFps : 0f;
        #endregion



        #region Memory
        public static long Monoused => Profiler.GetMonoUsedSizeLong();
        public static long Monoheap => Profiler.GetMonoHeapSizeLong();
        public static long Totalallocated => Profiler.GetTotalAllocatedMemoryLong();
        public static long Totalreserved => Profiler.GetTotalReservedMemoryLong();
        public static long Totalunusedreserved => Profiler.GetTotalUnusedReservedMemoryLong();
        #endregion



        #region System
        public static string Cpu => SystemInfo.processorType;
        public static int Cpucores => SystemInfo.processorCount;
        public static string Gpu => SystemInfo.graphicsDeviceName;
        public static int Gpumemory => SystemInfo.graphicsMemorySize;
        public static string Os => SystemInfo.operatingSystem;
        public static double Refreshrate
        {
            get
            {
                #if UNITY_2022_2_OR_NEWER
                return Screen.currentResolution.refreshRateRatio.value;
                #else
                return Screen.currentResolution.refreshRate;
                #endif
            }
        }
        #endregion



        #region Time
        public static string Timestamp => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        public static int Year => DateTime.Now.Year;
        public static int Month => DateTime.Now.Month;
        public static int Day => DateTime.Now.Day;

        public static int Hour => DateTime.Now.Hour;
        public static int Minute => DateTime.Now.Minute;
        public static int Second => DateTime.Now.Second;
        #endregion
        
        #endregion
    }
}
