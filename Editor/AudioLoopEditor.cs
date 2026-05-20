/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

using SETB.SuperClasses;
using static SETB.EditorGUI_Base;
using static SETB.HandyEditorFunctions;

namespace SHUU._Editor
{
    public class AudioLoopEditor : EditorWindow_Base<AudioLoopEditor>
    {
        #region Variables
        private SerializedObject so;
        private SerializedProperty musicProp;
        private SerializedProperty clipProp;
        private SerializedProperty loopProp;



        private AudioClip clip;

        private float startNorm = 0.1f;
        private float endNorm = 0.9f;


        private bool draggingStart;
        private bool draggingEnd;

        private const int waveformResolution = 1024;
        private float[] waveform;

        private double previewStartDSP;
        private bool previewing;
        #endregion




        #region Main
        protected override void OnDisable()
        {
            base.OnDisable();


            StopPreview();
        }

        public static void ShowWindow(Object target, string propertyPath)
        {
            var window = CreateWindow("Test Tool", true, true, false);


            window.so = new SerializedObject(target);
            window.musicProp = FindProperty(window.so, propertyPath);

            window.clipProp = FindPropertyRelative(window.musicProp, "music");
            window.loopProp = FindPropertyRelative(window.musicProp, "loopSlices");

            
            window.clip = window.clipProp.objectReferenceValue as AudioClip;

            if (window.loopProp != null)
            {
                float start = FindPropertyRelative(window.loopProp, "startPoint").floatValue;
                float end = FindPropertyRelative(window.loopProp, "endPoint").floatValue;

                if (window.clip != null && window.clip.length > 0f)
                {
                    window.startNorm = start / window.clip.length;
                    window.endNorm = end / window.clip.length;
                }
            }
        }


        protected void OnGUI()
        {
            Space();

            AudioClip newClip = clip;
            DrawInputObject("Audio Clip", ref newClip, false);

            if (newClip != clip)
            {
                clip = newClip;
                waveform = null;
                StopPreview();
            }

            if (clip == null) return;

            DrawWaveform();
            DrawLoopLines();
            DrawInfo();
            DrawControls();



            if (so != null && loopProp != null && clip != null)
            {
                so.Update();

                clipProp.objectReferenceValue = clip;

                float startTime = startNorm * clip.length;
                float endTime = endNorm * clip.length;

                loopProp.FindPropertyRelative("startPoint").floatValue = startTime;
                loopProp.FindPropertyRelative("endPoint").floatValue = endTime;

                so.ApplyModifiedProperties();
            }
        }
        #endregion



        #region Logic
            #region Visual
            private void GenerateWaveform()
            {
                waveform = new float[waveformResolution];

                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                int step = Mathf.Max(1, samples.Length / waveformResolution);

                for (int i = 0; i < waveformResolution; i++)
                {
                    float max = 0f;
                    int start = i * step;

                    for (int j = 0; j < step && start + j < samples.Length; j++)
                    {
                        max = Mathf.Max(max, Mathf.Abs(samples[start + j]));
                    }

                    waveform[i] = max;
                }
            }

            private void DrawWaveform()
            {
                if (waveform == null) GenerateWaveform();

                Rect r = GetRect(position.width, 120);
                

                DrawRect(r, new Color(0.15f, 0.15f, 0.15f));

                for (int i = 0; i < waveform.Length; i++)
                {
                    float x = Mathf.Lerp(r.x, r.xMax, i / (float)waveform.Length);
                    float h = waveform[i] * r.height;

                    DrawRect(new Rect(x, r.center.y - h / 2f, 1, h), Color.green);
                }
            }
            #endregion



            #region Loop Lines
            private void DrawLoopLines()
            {
                Rect r = GUILayoutUtility.GetLastRect();

                float startX = Mathf.Lerp(r.x, r.xMax, startNorm);
                float endX = Mathf.Lerp(r.x, r.xMax, endNorm);

                Handles.color = Color.red;
                Handles.DrawLine(new Vector2(startX, r.y), new Vector2(startX, r.yMax));

                Handles.color = Color.cyan;
                Handles.DrawLine(new Vector2(endX, r.y), new Vector2(endX, r.yMax));

                HandleDragging(r, startX, endX);
            }


            private void HandleDragging(Rect r, float startX, float endX)
            {
                Event e = Event.current;

                if (e.type == EventType.MouseDown)
                {
                    if (Mathf.Abs(e.mousePosition.x - startX) < 6)
                    {
                        draggingStart = true;
                        e.Use();
                    }
                    else if (Mathf.Abs(e.mousePosition.x - endX) < 6)
                    {
                        draggingEnd = true;
                        e.Use();
                    }
                }

                if (e.type == EventType.MouseDrag)
                {
                    if (draggingStart) startNorm = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width);
                    if (draggingEnd) endNorm = Mathf.Clamp01((e.mousePosition.x - r.x) / r.width);

                    PreventCrossing();

                    Repaint();
                }

                if (e.type == EventType.MouseUp) draggingStart = draggingEnd = false;
            }

            private void PreventCrossing()
            {
                startNorm = Mathf.Min(startNorm, endNorm);
                endNorm = Mathf.Max(startNorm, endNorm);
            }
            #endregion

            

            #region Info
            #region Local vars
            float StartTime
            {
                get => startNorm * clip.length;
                set
                {
                    startNorm = value / clip.length;
                }
            }
            float EndTime
            {
                get => endNorm * clip.length;
                set
                {
                    endNorm = value / clip.length;
                }
            }

            int StartSample
            {
                get => Mathf.RoundToInt(StartTime * clip.frequency);
                set
                {
                    StartTime = value / clip.frequency;
                }
            }
            int EndSample
            {
                get => Mathf.RoundToInt(EndTime * clip.frequency);
                set
                {
                    EndTime = value / clip.frequency;
                }
            }
            #endregion


            private void DrawInfo()
            {
                Space();


                Horizontal(() =>
                {
                    StartTime = DrawInputFloat("Loop Start (seconds)", StartTime);

                    Space(4f);

                    DrawLabel($"Loop Start (samples): {StartSample}");
                });

                Horizontal(() =>
                {
                    EndTime = DrawInputFloat("Loop End (seconds)", EndTime);

                    Space(4f);

                    DrawLabel($"Loop End (samples): {EndSample}");
                });

                PreventCrossing();


                Space();

                DrawLabel($"Duration:  {EndTime - StartTime}s");
            }
            #endregion


            
            #region Preview Handling
            private void DrawControls()
            {
                FlexibleSpace();
                
                Horizontal(() =>
                {
                    DrawButton("Play Loop", () => PlayPreview());
                    DrawButton("Stop", () => StopPreview());
                });

                Space(4f);
            }


            private void PlayPreview()
            {
                StopPreview();

                AudioPreview.Play(clip, StartSample);

                previewStartDSP = AudioSettings.dspTime;
                previewing = true;

                EditorApplication.update += UpdatePreview;
            }
            private void StopPreview()
            {
                if (!previewing) return;

                AudioPreview.Stop();
                EditorApplication.update -= UpdatePreview;
                previewing = false;
            }
            
            private void UpdatePreview()
            {
                if (!previewing || clip == null) return;

                double elapsed = AudioSettings.dspTime - previewStartDSP;
                double loopDuration = EndTime - StartTime;

                if (elapsed >= loopDuration)
                {
                    AudioPreview.Play(clip, StartSample);
                    previewStartDSP = AudioSettings.dspTime;
                }
            }
            #endregion
        #endregion
    }




    #region Helper class
    public static class AudioPreview
    {
        #region Variables
        static System.Type audioUtilType;

        static MethodInfo playMethod;
        static MethodInfo stopMethod;


        static bool initialized;
        #endregion




        #region Main
        static void Init()
        {
            if (initialized) return;
            initialized = true;

            audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            if (audioUtilType == null)
            {
                Debug.LogError("AudioUtil not found");
                return;
            }

            playMethod = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
                        audioUtilType.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            stopMethod = audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ??
                        audioUtilType.GetMethod("StopAllClips", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (playMethod == null) Debug.LogError("Could not find Play method in AudioUtil");

            if (stopMethod == null) Debug.LogError("Could not find Stop method in AudioUtil");
        }
        #endregion



        #region Logic
        public static void Play(AudioClip clip, int startSample)
        {
            Init();

            if (clip == null || playMethod == null) return;

            Stop();

            var parameters = playMethod.GetParameters();

            try
            {
                if (parameters.Length == 1) playMethod.Invoke(null, new object[] { clip });
                else if (parameters.Length == 3) playMethod.Invoke(null, new object[] { clip, startSample, false });
                else playMethod.Invoke(null, new object[] { clip });
            }
            catch (System.Exception e) { Debug.LogError("Audio preview failed: " + e); }
        }


        public static void Stop()
        {
            Init();

            try { stopMethod?.Invoke(null, null); }
            catch (System.Exception e) { Debug.LogError("Audio stop failed: " + e); }
        }
        #endregion
    }
    #endregion
}
#endif
