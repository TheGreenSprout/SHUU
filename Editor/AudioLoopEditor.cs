/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace SHUU._Editor
{
    public class AudioLoopEditor : EditorWindow
    {
        AudioClip clip;

        float startNorm = 0.1f;
        float endNorm = 0.9f;

        bool draggingStart;
        bool draggingEnd;

        const int waveformResolution = 1024;
        float[] waveform;

        double previewStartDSP;
        bool previewing;

        // =========================
        // Window
        // =========================

        [MenuItem("Tools/Sprout's Handy Unity Utils/Audio/Audio Loop Editor")]
        static void Open()
        {
            GetWindow<AudioLoopEditor>("Audio Loop Editor");
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            AudioClip newClip = (AudioClip)EditorGUILayout.ObjectField(
                "Audio Clip", clip, typeof(AudioClip), false);

            if (newClip != clip)
            {
                clip = newClip;
                waveform = null;
                StopPreview();
            }

            if (clip == null)
                return;

            DrawWaveform();
            DrawLoopLines();
            DrawInfo();
            DrawControls();
        }

        // =========================
        // Waveform
        // =========================

        void GenerateWaveform()
        {
            waveform = new float[waveformResolution];

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            int step = samples.Length / waveformResolution;

            for (int i = 0; i < waveformResolution; i++)
            {
                float max = 0f;
                int start = i * step;

                for (int j = 0; j < step && start + j < samples.Length; j++)
                    max = Mathf.Max(max, Mathf.Abs(samples[start + j]));

                waveform[i] = max;
            }
        }

        void DrawWaveform()
        {
            if (waveform == null)
                GenerateWaveform();

            Rect r = GUILayoutUtility.GetRect(position.width, 120);

            EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f));

            for (int i = 0; i < waveform.Length; i++)
            {
                float x = Mathf.Lerp(r.x, r.xMax, i / (float)waveform.Length);
                float h = waveform[i] * r.height;
                EditorGUI.DrawRect(
                    new Rect(x, r.center.y - h / 2f, 1, h),
                    Color.green);
            }
        }

        // =========================
        // Loop Lines
        // =========================

        void DrawLoopLines()
        {
            Rect r = GUILayoutUtility.GetLastRect();

            float startX = Mathf.Lerp(r.x, r.xMax, startNorm);
            float endX = Mathf.Lerp(r.x, r.xMax, endNorm);

            Handles.color = Color.red;
            Handles.DrawLine(new Vector2(startX, r.y), new Vector2(startX, r.yMax));

            Handles.color = Color.cyan;
            Handles.DrawLine(new Vector2(endX, r.y), new Vector2(endX, r.yMax));

            HandleDragging(r);
        }

        void HandleDragging(Rect r)
        {
            Event e = Event.current;

            float startX = Mathf.Lerp(r.x, r.xMax, startNorm);
            float endX = Mathf.Lerp(r.x, r.xMax, endNorm);

            if (e.type == EventType.MouseDown)
            {
                if (Mathf.Abs(e.mousePosition.x - startX) < 6)
                    draggingStart = true;
                else if (Mathf.Abs(e.mousePosition.x - endX) < 6)
                    draggingEnd = true;
            }

            if (e.type == EventType.MouseDrag)
            {
                if (draggingStart)
                    startNorm = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width);

                if (draggingEnd)
                    endNorm = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width);

                if (startNorm > endNorm)
                    startNorm = endNorm;

                Repaint();
            }

            if (e.type == EventType.MouseUp)
            {
                draggingStart = draggingEnd = false;
            }
        }

        // =========================
        // Info
        // =========================

        float StartTime => startNorm * clip.length;
        float EndTime => endNorm * clip.length;

        int StartSample => Mathf.RoundToInt(StartTime * clip.frequency);
        int EndSample => Mathf.RoundToInt(EndTime * clip.frequency);

        void DrawInfo()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Loop Start: {StartTime:F4}s  |  Sample: {StartSample}");
            EditorGUILayout.LabelField($"Loop End:   {EndTime:F4}s  |  Sample: {EndSample}");
            EditorGUILayout.LabelField($"Duration:  {(EndTime - StartTime):F4}s");
        }

        // =========================
        // Controls
        // =========================

        void DrawControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Play Loop"))
                PlayPreview();

            if (GUILayout.Button("Stop"))
                StopPreview();

            EditorGUILayout.EndHorizontal();
        }

        // =========================
        // Audio Preview (AudioUtil)
        // =========================

        static class AudioPreview
        {
            static System.Type audioUtilType;
            static MethodInfo playMethod;
            static MethodInfo stopMethod;

            static bool initialized;

            static void Init()
            {
                if (initialized)
                    return;

                initialized = true;

                audioUtilType = typeof(AudioImporter)
                    .Assembly
                    .GetType("UnityEditor.AudioUtil");

                if (audioUtilType == null)
                {
                    Debug.LogError("AudioUtil type not found");
                    return;
                }

                // Try common PlayClip signatures
                playMethod =
                    audioUtilType.GetMethod(
                        "PlayClip",
                        BindingFlags.Static | BindingFlags.Public,
                        null,
                        new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                        null)
                    ??
                    audioUtilType.GetMethod(
                        "PlayClip",
                        BindingFlags.Static | BindingFlags.Public,
                        null,
                        new[] { typeof(AudioClip) },
                        null);

                stopMethod =
                    audioUtilType.GetMethod(
                        "StopAllClips",
                        BindingFlags.Static | BindingFlags.Public)
                    ??
                    audioUtilType.GetMethod(
                        "StopClip",
                        BindingFlags.Static | BindingFlags.Public);

                if (playMethod == null)
                    Debug.LogError("AudioUtil.PlayClip not found");

                if (stopMethod == null)
                    Debug.LogError("AudioUtil.Stop method not found");
            }

            public static void Play(AudioClip clip, int startSample)
            {
                Init();

                if (clip == null || playMethod == null)
                    return;

                Stop();

                var parameters = playMethod.GetParameters();

                if (parameters.Length == 1)
                    playMethod.Invoke(null, new object[] { clip });
                else
                    playMethod.Invoke(null, new object[] { clip, startSample, false });
            }

            public static void Stop()
            {
                Init();

                if (stopMethod == null)
                    return;

                stopMethod.Invoke(null, null);
            }
        }

        void PlayPreview()
        {
            StopPreview();
            AudioPreview.Play(clip, StartSample);
            previewStartDSP = AudioSettings.dspTime;
            previewing = true;
            EditorApplication.update += UpdatePreview;
        }

        void StopPreview()
        {
            if (!previewing)
                return;

            AudioPreview.Stop();
            EditorApplication.update -= UpdatePreview;
            previewing = false;
        }

        void UpdatePreview()
        {
            double elapsed = AudioSettings.dspTime - previewStartDSP;
            if (elapsed >= (EndTime - StartTime))
                StopPreview();
        }

        void OnDisable()
        {
            StopPreview();
        }
    }
}
#endif
