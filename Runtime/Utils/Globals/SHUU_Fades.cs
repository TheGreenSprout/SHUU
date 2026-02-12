using System;
using SHUU.Utils.UI;
using UnityEngine;

namespace SHUU.Utils.Globals
{

    #region Option classes
    public class FadeOptions
    {
        public Color? startColor = null;
        public Color? endColor = null;


        public float? duration = null;

        public float start_delay = 0f;
        public float end_delay = 0f;


        public Action end_Action = null;


        public bool clearOnEnd = true;



        public FadeOptions() { }
        public FadeOptions(FadeOptions other)
        {
            startColor = other.startColor;
            endColor = other.endColor;


            duration = other.duration;

            start_delay = other.start_delay;
            end_delay = other.end_delay;


            end_Action = other.end_Action;


            clearOnEnd = other.clearOnEnd;
        }
    }
    public class PingPong_FadeOptions
    {
        public Color? startColor = null;
        public Color? middleColor = null;
        public Color? endColor = null;


        public float? firstFade_duration = null;
        public float? pingPong_duration = null;
        public float? secondFade_duration = null;

        public float start_delay = 0f;
        public float end_delay = 0f;

        public float middleAction_delay = 0.05f;


        public Action endFirstFade_Action = null;
        public Action middle_Action = null;
        public Action startSecondFade_Action = null;

        public Action end_Action = null;



        public PingPong_FadeOptions() { }
        public PingPong_FadeOptions(PingPong_FadeOptions other)
        {
            startColor = other.startColor;
            middleColor = other.middleColor;
            endColor = other.endColor;


            firstFade_duration = other.firstFade_duration;
            pingPong_duration = other.pingPong_duration;
            secondFade_duration = other.secondFade_duration;

            start_delay = other.start_delay;
            end_delay = other.end_delay;

            middleAction_delay = other.middleAction_delay;


            endFirstFade_Action = other.endFirstFade_Action;
            middle_Action = other.middle_Action;
            startSecondFade_Action = other.startSecondFade_Action;

            end_Action = other.end_Action;
        }
    }
    #endregion



    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Manages fade-in/outs for scene transitions.
    /// </summary>
    #endregion
    public class SHUU_Fades : MonoBehaviour
    {
        private static SHUU_Fades _instance;
        
        private static SHUU_Fades instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SHUU_Fades>(FindObjectsInactive.Include);

                    if (_instance == null) Debug.LogError("No SHUU_Fades found in scene.");
                }

                return _instance;
            }
        }



        [SerializeField] private FadePanel fadePanel_scr;



        [SerializeField] private float defaultFadeDuration;
        [SerializeField] private float defaultPingPongDuration;



        [SerializeField] private Color default_solid = Color.black;
        [SerializeField] private Color default_transparent = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);



        private FadeOptions currentOptions = null;




        private void Awake()
        {
            if (_instance == null) _instance = this;
        }



        public static void CreateFade(FadeOptions fadeOptions = null) => instance._CreateFade(fadeOptions);
        private void _CreateFade(FadeOptions fadeOptions = null)
        {
            if (currentOptions != null) return;


            FadeOptions localOptions;
            if (fadeOptions == null) localOptions = new FadeOptions();
            else localOptions = new FadeOptions(fadeOptions);

            if (localOptions.startColor == null) localOptions.startColor = default_solid;
            if (localOptions.endColor == null) localOptions.endColor = GetOppositeAlpha(localOptions.startColor.Value);
            if (localOptions.duration == null) localOptions.duration = defaultFadeDuration;


            localOptions.end_Action += () => currentOptions = null;

            currentOptions = localOptions;

            if (currentOptions != null) fadePanel_scr.NewFade(currentOptions);
        }

        public static void CreateFade_In(FadeOptions fadeOptions = null) => instance._CreateFade_In(fadeOptions);
        private void _CreateFade_In(FadeOptions fadeOptions = null)
        {
            FadeOptions localOptions;
            if (fadeOptions == null) localOptions = new FadeOptions();
            else localOptions = new FadeOptions(fadeOptions);


            localOptions.startColor = default_solid;
            localOptions.endColor = default_transparent;


            CreateFade(localOptions);
        }
        public static void CreateFade_Out(FadeOptions fadeOptions = null) => instance._CreateFade_Out(fadeOptions);
        private void _CreateFade_Out(FadeOptions fadeOptions = null)
        {
            FadeOptions localOptions;
            if (fadeOptions == null) localOptions = new FadeOptions();
            else localOptions = new FadeOptions(fadeOptions);


            localOptions.startColor = default_transparent;
            localOptions.endColor = default_solid;


            CreateFade(localOptions);
        }

        public static void CreateFade_PingPong(PingPong_FadeOptions pingPongOptions = null) => instance._CreateFade_PingPong(pingPongOptions);
        private void _CreateFade_PingPong(PingPong_FadeOptions pingPongOptions = null)
        {
            PingPong_FadeOptions localOptions;
            if (pingPongOptions == null) localOptions = new PingPong_FadeOptions();
            else localOptions = new PingPong_FadeOptions(pingPongOptions);

            if (localOptions.startColor == null) localOptions.startColor = default_solid;
            if (localOptions.middleColor == null) localOptions.middleColor = GetOppositeAlpha(localOptions.startColor.Value);
            if (localOptions.endColor == null) localOptions.endColor = localOptions.startColor;

            if (localOptions.pingPong_duration == null) localOptions.pingPong_duration = defaultPingPongDuration;

            if (localOptions.firstFade_duration == null && localOptions.secondFade_duration != null) localOptions.firstFade_duration = localOptions.secondFade_duration;
            else if (localOptions.firstFade_duration != null && localOptions.secondFade_duration == null) localOptions.secondFade_duration = localOptions.firstFade_duration;


            Action pingPong = () =>
            {
                currentOptions = null;
                
                CreateFade(new FadeOptions
                {
                    startColor = localOptions.middleColor,
                    endColor = localOptions.endColor,
                    duration = localOptions.secondFade_duration,
                    end_delay = localOptions.end_delay,
                    end_Action = localOptions.end_Action
                });

                localOptions.startSecondFade_Action?.Invoke();
            };
            
            Action holdColor = () =>
            {
                currentOptions = null;

                CreateFade(new FadeOptions
                {
                    startColor = localOptions.middleColor,
                    endColor = localOptions.middleColor,
                    duration = localOptions.pingPong_duration,
                    end_Action = pingPong
                });

                if (localOptions.middle_Action != null) SHUU_Time.Timer(localOptions.middleAction_delay, localOptions.middle_Action);
                localOptions.endFirstFade_Action?.Invoke();
            };

            CreateFade(new FadeOptions {
                startColor = localOptions.startColor,
                endColor = localOptions.middleColor,
                duration = localOptions.firstFade_duration,
                start_delay = localOptions.start_delay,
                end_Action = holdColor
            });
        }


        private Color GetOppositeAlpha(Color color)
        {
            if (color.a > 0) return new Color(color.r, color.g, color.b, 0f);
            else return new Color(color.r, color.g, color.b, 1f);
        }
    }

}
