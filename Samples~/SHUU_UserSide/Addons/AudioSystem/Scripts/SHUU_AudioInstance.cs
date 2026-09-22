using System;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

using Alchemy.Inspector;

using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.AudioSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SHUU_AudioInstance : MonoBehaviour, IObjectPoolable
    {
        #region Variables
        public bool returnToPool_clipEnd = true;
        public bool returnToPool_sceneChange = true;


        public UnityEvent onAudioEnd = null;



        private Options snapshot = null;
        private AudioClip snapshot_clip = null;


        private AudioSource target;


        private SHUU_ObjectPool<SHUU_AudioInstance> pool = null;


        private bool playing = false;
        private bool paused = false;
        #endregion




        #region Main
        private void Awake() => target = GetComponent<AudioSource>();

        public void Init(SHUU_ObjectPool<SHUU_AudioInstance> pool, Options options)
        {
            if (!target) target = GetComponent<AudioSource>();

            this.pool = pool;
            if (returnToPool_sceneChange) SceneLoader.OnSceneLoadRequested += ReturnToPool;

            SetOptions(options);
        }


        private void Update()
        {
            if (!playing && target.isPlaying) playing = true;


            if (!playing || paused) return;

            if (!target.isPlaying)
            {
                playing = false;

                onAudioEnd?.Invoke();
                if (returnToPool_clipEnd) ReturnToPool();
            }
        }
        #endregion



        #region Logic
        public void ReturnToPool(string _ = null)
        {
            if (target.isPlaying) target.Stop();

            if (returnToPool_sceneChange) SceneLoader.OnSceneLoadRequested -= ReturnToPool;

            if (pool == null) Destroy(gameObject);
            else pool.Return(this);
        }
        #endregion



        #region Proxy Logic

        #region Playback
        public void Play() => target.Play();
        public void Play(ulong delay) => target.Play(delay);
        public void PlayDelayed(float delay) => target.PlayDelayed(delay);
        public void PlayScheduled(double time) => target.PlayScheduled(time);
        public void PlayOneShot(AudioClip clip) => target.PlayOneShot(clip);
        public void PlayOneShot(AudioClip clip, float volumeScale) => target.PlayOneShot(clip, volumeScale);

        public void Stop(bool tempStop = false)
        {
            paused = tempStop;
            target.Stop();
        }

        public void Pause()
        {
            paused = true;
            target.Pause();
        }
        public void UnPause()
        {
            paused = false;
            target.UnPause();
        }

        public void SetScheduledStartTime(double time) => target.SetScheduledStartTime(time);
        public void SetScheduledEndTime(double time) => target.SetScheduledEndTime(time);
        #endregion


        #region State
        public bool isPlaying => target.isPlaying;
        public bool isVirtual => target.isVirtual;
        #endregion


        #region Properties
        public AudioClip clip { get => target.clip; set => target.clip = value; }

        public float volume { get => target.volume; set => target.volume = value; }
        public float pitch { get => target.pitch; set => target.pitch = value; }
        public float panStereo { get => target.panStereo; set => target.panStereo = value; }
        public float spatialBlend { get => target.spatialBlend; set => target.spatialBlend = value; }
        public int priority { get => target.priority; set => target.priority = value; }

        public float time { get => target.time; set => target.time = value; }
        public int timeSamples { get => target.timeSamples; set => target.timeSamples = value; }

        public bool loop { get => target.loop; set => target.loop = value; }
        public bool mute { get => target.mute; set => target.mute = value; }
        public bool playOnAwake { get => target.playOnAwake; set => target.playOnAwake = value; }

        public float dopplerLevel { get => target.dopplerLevel; set => target.dopplerLevel = value; }
        public float spread { get => target.spread; set => target.spread = value; }
        public AudioRolloffMode rolloffMode { get => target.rolloffMode; set => target.rolloffMode = value; }
        public float minDistance { get => target.minDistance; set => target.minDistance = value; }
        public float maxDistance { get => target.maxDistance; set => target.maxDistance = value; }

        public bool bypassEffects { get => target.bypassEffects; set => target.bypassEffects = value; }
        public bool bypassListenerEffects { get => target.bypassListenerEffects; set => target.bypassListenerEffects = value; }
        public bool bypassReverbZones { get => target.bypassReverbZones; set => target.bypassReverbZones = value; }
        public float reverbZoneMix { get => target.reverbZoneMix; set => target.reverbZoneMix = value; }

        public AudioMixerGroup outputAudioMixerGroup { get => target.outputAudioMixerGroup; set => target.outputAudioMixerGroup = value; }
        #endregion

        #endregion



        #region Override points
        public void SaveDefaults()
        {
            if (!target) target = GetComponent<AudioSource>();

            
            snapshot_clip = target.clip;

            snapshot = new Options()
            {
                // Playback
                playOnAwake = target.playOnAwake,
                loop = target.loop,
                priority = target.priority,
                volume = target.volume,
                pitch = target.pitch,
                panStereo = target.panStereo,
                spatialBlend = target.spatialBlend,

                // 3D Sound
                dopplerLevel = target.dopplerLevel,
                spread = target.spread,
                rolloffMode = target.rolloffMode,
                minDistance = target.minDistance,
                maxDistance = target.maxDistance,

                // Effects
                mute = target.mute,
                bypassEffects = target.bypassEffects,
                bypassListenerEffects = target.bypassListenerEffects,
                bypassReverbZones = target.bypassReverbZones,
                reverbZoneMix = target.reverbZoneMix,

                // Output
                outputAudioMixerGroup = target.outputAudioMixerGroup,

                // Custom
                onAudioEnd = this.onAudioEnd,
                returnToPool_clipEnd = this.returnToPool_clipEnd,
                returnToPool_sceneChange = this.returnToPool_sceneChange
            };
        }

        public void RestoreDefaults()
        {
            pool = null;

            playing = false;
            paused = false;


            if (snapshot_clip) target.clip = snapshot_clip;

            if (snapshot == null) return;
            SetOptionsOverride(snapshot);
        }
        #endregion



        #region Helpers
        private void SetOptionsOverride(Options options)
        {
            // Playback
            target.playOnAwake = options.playOnAwake;
            target.loop = options.loop;
            target.priority = options.priority;
            target.volume = options.volume;
            target.pitch = options.pitch;
            target.panStereo = options.panStereo;
            target.spatialBlend = options.spatialBlend;

            // 3D Sound
            target.dopplerLevel = options.dopplerLevel;
            target.spread = options.spread;
            target.rolloffMode = options.rolloffMode;
            target.minDistance = options.minDistance;
            target.maxDistance = options.maxDistance;

            // Effects
            target.mute = options.mute;
            target.bypassEffects = options.bypassEffects;
            target.bypassListenerEffects = options.bypassListenerEffects;
            target.bypassReverbZones = options.bypassReverbZones;
            target.reverbZoneMix = options.reverbZoneMix;

            // Output
            target.outputAudioMixerGroup = options.outputAudioMixerGroup;

            // Custom
            this.onAudioEnd = options.onAudioEnd;
            this.returnToPool_clipEnd = options.returnToPool_clipEnd;
            this.returnToPool_sceneChange = options.returnToPool_sceneChange;
        }

        internal void SetOptions(Options options)
        {
            // Playback
            if (options.has_playOnAwake) target.playOnAwake = options.playOnAwake;
            if (options.has_loop) target.loop = options.loop;
            if (options.has_priority) target.priority = options.priority;
            if (options.has_volume) target.volume = options.volume;
            if (options.has_pitch) target.pitch = options.pitch;
            if (options.has_panStereo) target.panStereo = options.panStereo;
            if (options.has_spatialBlend) target.spatialBlend = options.spatialBlend;

            // 3D Sound
            if (options.has_dopplerLevel) target.dopplerLevel = options.dopplerLevel;
            if (options.has_spread) target.spread = options.spread;
            if (options.has_rolloffMode) target.rolloffMode = options.rolloffMode;
            if (options.has_minDistance) target.minDistance = options.minDistance;
            if (options.has_maxDistance) target.maxDistance = options.maxDistance;

            // Effects
            if (options.has_mute) target.mute = options.mute;
            if (options.has_bypassEffects) target.bypassEffects = options.bypassEffects;
            if (options.has_bypassListenerEffects) target.bypassListenerEffects = options.bypassListenerEffects;
            if (options.has_bypassReverbZones) target.bypassReverbZones = options.bypassReverbZones;
            if (options.has_reverbZoneMix) target.reverbZoneMix = options.reverbZoneMix;

            // Output
            if (options.has_outputAudioMixerGroup) target.outputAudioMixerGroup = options.outputAudioMixerGroup;

            // Custom
            if (options.has_onAudioEnd) this.onAudioEnd = options.onAudioEnd;
            if (options.has_returnToPool_clipEnd) this.returnToPool_clipEnd = options.returnToPool_clipEnd;
            if (options.has_returnToPool_sceneChange) this.returnToPool_sceneChange = options.returnToPool_sceneChange;
        }
        #endregion




        #region Options class
        [Serializable]
        public class Options
        {
            #region Variables
            // Playback
            [Title("Playback")]

            public bool has_playOnAwake = false;
            [ShowIf("has_playOnAwake"), Indent] public bool playOnAwake = false;

            public bool has_loop = false;
            [ShowIf("has_loop"), Indent] public bool loop = false;

            public bool has_priority = false;
            [ShowIf("has_priority"), Indent] public int priority = 128;

            public bool has_volume = false;
            [ShowIf("has_volume"), Indent] public float volume = 1f;

            public bool has_pitch = false;
            [ShowIf("has_pitch"), Indent] public float pitch = 1f;

            public bool has_panStereo = false;
            [ShowIf("has_panStereo"), Indent] public float panStereo = 0f;

            public bool has_spatialBlend = false;
            [ShowIf("has_spatialBlend"), Indent] public float spatialBlend = 0f;


            // 3D Sound
            [Title("3D Sound")]
            
            public bool has_dopplerLevel = false;
            [ShowIf("has_dopplerLevel"), Indent] public float dopplerLevel = 1f;

            public bool has_spread = false;
            [ShowIf("has_spread"), Indent] public float spread = 0f;

            public bool has_rolloffMode = false;
            [ShowIf("has_rolloffMode"), Indent] public AudioRolloffMode rolloffMode;

            public bool has_minDistance = false;
            [ShowIf("has_minDistance"), Indent] public float minDistance = 1f;

            public bool has_maxDistance = false;
            [ShowIf("has_maxDistance"), Indent] public float maxDistance = 500f;


            // Effects
            [Title("Effects")]
            
            public bool has_mute = false;
            [ShowIf("has_mute"), Indent] public bool mute = false;

            public bool has_bypassEffects = false;
            [ShowIf("has_bypassEffects"), Indent] public bool bypassEffects = false;

            public bool has_bypassListenerEffects = false;
            [ShowIf("has_bypassListenerEffects"), Indent] public bool bypassListenerEffects = false;

            public bool has_bypassReverbZones = false;
            [ShowIf("has_bypassReverbZones"), Indent] public bool bypassReverbZones = false;

            public bool has_reverbZoneMix = false;
            [ShowIf("has_reverbZoneMix"), Indent] public float reverbZoneMix = 1f;


            // Output
            [Title("Output")]
            
            public bool has_outputAudioMixerGroup = false;
            [ShowIf("has_outputAudioMixerGroup"), Indent] public AudioMixerGroup outputAudioMixerGroup;


            // Custom
            [Title("Custom")]

            public bool has_onAudioEnd = false;
            [ShowIf("has_returnToPool"), Indent] public UnityEvent onAudioEnd = null;
            
            
            public bool has_returnToPool_clipEnd = false;
            [ShowIf("has_returnToPool_clipEnd"), Indent] public bool returnToPool_clipEnd = true;

            public bool has_returnToPool_sceneChange = false;
            [ShowIf("has_returnToPool_sceneChange"), Indent] public bool returnToPool_sceneChange = true;
            #endregion



            #region Main
            public Options() { }

            public Options(Options other)
            {
                if (other == null) return;

                // Playback
                has_playOnAwake = other.has_playOnAwake;
                playOnAwake = other.playOnAwake;
                
                has_loop = other.has_loop;
                loop = other.loop;

                has_priority = other.has_priority;
                priority = other.priority;

                has_volume = other.has_volume;
                volume = other.volume;

                has_pitch = other.has_pitch;
                pitch = other.pitch;

                has_panStereo = other.has_panStereo;
                panStereo = other.panStereo;

                has_spatialBlend = other.has_spatialBlend;
                spatialBlend = other.spatialBlend;


                // 3D Sound
                has_dopplerLevel = other.has_dopplerLevel;
                dopplerLevel = other.dopplerLevel;

                has_spread = other.has_spread;
                spread = other.spread;

                has_rolloffMode = other.has_rolloffMode;
                rolloffMode = other.rolloffMode;

                has_minDistance = other.has_minDistance;
                minDistance = other.minDistance;

                has_maxDistance = other.has_maxDistance;
                maxDistance = other.maxDistance;


                // Effects
                has_mute = other.has_mute;
                mute = other.mute;

                has_bypassEffects = other.has_bypassEffects;
                bypassEffects = other.bypassEffects;

                has_bypassListenerEffects = other.has_bypassListenerEffects;
                bypassListenerEffects = other.bypassListenerEffects;

                has_bypassReverbZones = other.has_bypassReverbZones;
                bypassReverbZones = other.bypassReverbZones;

                has_reverbZoneMix = other.has_reverbZoneMix;
                reverbZoneMix = other.reverbZoneMix;


                // Output
                has_outputAudioMixerGroup = other.has_outputAudioMixerGroup;
                outputAudioMixerGroup = other.outputAudioMixerGroup;


                // Custom
                has_onAudioEnd = other.has_onAudioEnd;
                onAudioEnd = other.onAudioEnd;

                has_returnToPool_clipEnd = other.has_returnToPool_clipEnd;
                returnToPool_clipEnd = other.returnToPool_clipEnd;

                has_returnToPool_sceneChange = other.has_returnToPool_sceneChange;
                returnToPool_sceneChange = other.returnToPool_sceneChange;
            }
            #endregion


            #region Logic
            public Options InheritOptionsNewClass(Options inheritOptions)
            {
                var ret = new Options(this);

                ret.InheritOptions(inheritOptions);

                return ret;
            }
        

            public void InheritOptions(Options options)
            {
                // Playback
                if (!has_playOnAwake) playOnAwake = options.playOnAwake;
                if (!has_loop) loop = options.loop;
                if (!has_priority) priority = options.priority;
                if (!has_volume) volume = options.volume;
                if (!has_pitch) pitch = options.pitch;
                if (!has_panStereo) panStereo = options.panStereo;
                if (!has_spatialBlend) spatialBlend = options.spatialBlend;

                // 3D Sound
                if (!has_dopplerLevel) dopplerLevel = options.dopplerLevel;
                if (!has_spread) spread = options.spread;
                if (!has_rolloffMode) rolloffMode = options.rolloffMode;
                if (!has_minDistance) minDistance = options.minDistance;
                if (!has_maxDistance) maxDistance = options.maxDistance;

                // Effects
                if (!has_mute) mute = options.mute;
                if (!has_bypassEffects) bypassEffects = options.bypassEffects;
                if (!has_bypassListenerEffects) bypassListenerEffects = options.bypassListenerEffects;
                if (!has_bypassReverbZones) bypassReverbZones = options.bypassReverbZones;
                if (!has_reverbZoneMix) reverbZoneMix = options.reverbZoneMix;

                // Output
                if (!has_outputAudioMixerGroup) outputAudioMixerGroup = options.outputAudioMixerGroup;

                // Custom
                if (!has_onAudioEnd) onAudioEnd = options.onAudioEnd;
                if (!has_returnToPool_clipEnd) returnToPool_clipEnd = options.returnToPool_clipEnd;
                if (!has_returnToPool_sceneChange) returnToPool_sceneChange = options.returnToPool_sceneChange;
            }

            public void InheritOptions(params Options[] options) => InheritOptions(options.AsEnumerable());
            public void InheritOptions(IEnumerable<Options> options)
            {
                foreach (var mergeOptions in options)
                    InheritOptions(mergeOptions);
            }

            public static Options MergeOptions(params Options[] options)
            {
                var ret = new Options(options[0]);

                ret.InheritOptions(options.Skip(1));

                return ret;
            }
            #endregion
        }
        #endregion
    }
}
