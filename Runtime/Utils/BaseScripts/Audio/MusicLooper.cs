using SHUU.Utils.BaseScripts.ScriptableObjs.Audio;
using UnityEngine;

namespace SHUU.Utils.BaseScripts.Audio
{

    public class MusicLooper : MonoBehaviour
    {
        private AudioClip clip;
        [HideInInspector] public AudioSource source;


        private Music_LoopSections loopSections;


        private bool jumpStart = false;



        private double nextLoopDspTime;
        private bool scheduled = false;




        public void Setup(AudioSource _source, Music_LoopSections sections, bool _jumpStart)
        {
            source = _source;
            clip = source.clip;

            loopSections = sections;

            jumpStart = _jumpStart;


            source.loop = false;
            source.playOnAwake = false;
        }


        private void OnEnable()
        {
            if (clip == null) return;


            source.time = jumpStart ? loopSections.startPoint : 0f;

            source.Play();
        }



        private void Update()
        {
            if (!source.isPlaying || scheduled) return;


            if (source.time >= loopSections.endPoint - 0.1f) ScheduleLoop();
        }

        private void LateUpdate()
        {
            if (scheduled && AudioSettings.dspTime >= nextLoopDspTime) scheduled = false;
        }


        private void ScheduleLoop()
        {
            double timeRemaining = loopSections.endPoint - source.time;

            nextLoopDspTime = AudioSettings.dspTime + timeRemaining;


            source.SetScheduledEndTime(nextLoopDspTime);

            source.PlayScheduled(nextLoopDspTime);
            source.time = loopSections.startPoint;


            scheduled = true;
        }
    }
}
