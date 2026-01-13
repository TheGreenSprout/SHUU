using UnityEngine;

namespace SHUU.Utils.BaseScripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPoolingHelper : MonoBehaviour
    {
        private struct AudioSourceSnapshot
        {
            // Playback
            public AudioClip clip;
            public bool playOnAwake;
            public bool loop;
            public int priority;
            public float volume;
            public float pitch;
            public float panStereo;
            public float spatialBlend;

            // 3D Sound
            public float dopplerLevel;
            public float spread;
            public AudioRolloffMode rolloffMode;
            public float minDistance;
            public float maxDistance;

            // Effects
            public bool mute;
            public bool bypassEffects;
            public bool bypassListenerEffects;
            public bool bypassReverbZones;
            public float reverbZoneMix;

            // Output
            public UnityEngine.Audio.AudioMixerGroup outputAudioMixerGroup;
        }
        private AudioSourceSnapshot? snapshot = null;


        private AudioSource source = null;




        public void Setup()
        {
            source = GetComponent<AudioSource>();
            
            snapshot = new AudioSourceSnapshot
            {
                // Playback
                clip = source.clip,
                playOnAwake = source.playOnAwake,
                loop = source.loop,
                priority = source.priority,
                volume = source.volume,
                pitch = source.pitch,
                panStereo = source.panStereo,
                spatialBlend = source.spatialBlend,

                // 3D Sound
                dopplerLevel = source.dopplerLevel,
                spread = source.spread,
                rolloffMode = source.rolloffMode,
                minDistance = source.minDistance,
                maxDistance = source.maxDistance,

                // Effects
                mute = source.mute,
                bypassEffects = source.bypassEffects,
                bypassListenerEffects = source.bypassListenerEffects,
                bypassReverbZones = source.bypassReverbZones,
                reverbZoneMix = source.reverbZoneMix,

                // Output
                outputAudioMixerGroup = source.outputAudioMixerGroup
            };
        }


        private void OnDisable()
        {
            ResetSource();

            if (gameObject.TryGetComponent(out AudioSelfDestruct selfDestruct)) Destroy(selfDestruct);
            if (gameObject.TryGetComponent(out MusicLooper looper)) Destroy(looper);
        }

        public void ResetSource()
        {
            if (source == null || snapshot == null) return;

            source.Stop();

            // Playback
            source.clip = snapshot.Value.clip;
            source.playOnAwake = snapshot.Value.playOnAwake;
            source.loop = snapshot.Value.loop;
            source.priority = snapshot.Value.priority;
            source.volume = snapshot.Value.volume;
            source.pitch = snapshot.Value.pitch;
            source.panStereo = snapshot.Value.panStereo;
            source.spatialBlend = snapshot.Value.spatialBlend;

            // 3D Sound
            source.dopplerLevel = snapshot.Value.dopplerLevel;
            source.spread = snapshot.Value.spread;
            source.rolloffMode = snapshot.Value.rolloffMode;
            source.minDistance = snapshot.Value.minDistance;
            source.maxDistance = snapshot.Value.maxDistance;

            // Effects
            source.mute = snapshot.Value.mute;
            source.bypassEffects = snapshot.Value.bypassEffects;
            source.bypassListenerEffects = snapshot.Value.bypassListenerEffects;
            source.bypassReverbZones = snapshot.Value.bypassReverbZones;
            source.reverbZoneMix = snapshot.Value.reverbZoneMix;

            // Output
            source.outputAudioMixerGroup = snapshot.Value.outputAudioMixerGroup;
        }
    }
}
