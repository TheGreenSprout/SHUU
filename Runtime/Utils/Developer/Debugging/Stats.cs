using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace SHUU.Utils.Developer.Debugging
{
    public class Stats : MonoBehaviour
    {
        private static float fpsTimer;
        private static int frameCount;
        private static float currentFps;



        public void Update()
        {
            frameCount++;
            fpsTimer += Time.unscaledDeltaTime;

            if (fpsTimer >= 0.5f) // update twice per second
            {
                currentFps = frameCount / fpsTimer;
                fpsTimer = 0f;
                frameCount = 0;
            }
        }



        public static float fps => currentFps;
        public static float frametimems => currentFps > 0 ? 1000f / currentFps : 0f;


        // Memory Stats
        public static long monoused => Profiler.GetMonoUsedSizeLong();
        public static long monoheap => Profiler.GetMonoHeapSizeLong();
        public static long totalallocated => Profiler.GetTotalAllocatedMemoryLong();
        public static long totalreserved => Profiler.GetTotalReservedMemoryLong();
        public static long totalunusedreserved => Profiler.GetTotalUnusedReservedMemoryLong();


        // System Stats
        public static string cpu => SystemInfo.processorType;
        public static int cpucores => SystemInfo.processorCount;
        public static string gpu => SystemInfo.graphicsDeviceName;
        public static int gpumemory => SystemInfo.graphicsMemorySize;
        public static string os => SystemInfo.operatingSystem;
        public static double refreshrate
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


        // Time Stats
        public static string timestamp => DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");


        public static int year => DateTime.Now.Year;
        public static int month => DateTime.Now.Month;
        public static int day => DateTime.Now.Day;

        public static int hour => DateTime.Now.Hour;
        public static int minute => DateTime.Now.Minute;
        public static int second => DateTime.Now.Second;
    }
}
