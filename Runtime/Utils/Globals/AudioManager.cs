using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SHUU.Utils.BaseScripts;

namespace SHUU.Utils.Globals
{

    #region XML doc
    /// <summary>
    /// Manages multiple audio-related functions.
    /// </summary>
    #endregion
    public class AudioManager : MonoBehaviour
    {
        #region Variables

        public class AudioOptions
        {
            public int priority { get; set; } = 128;

            public float volume { get; set; } = 1f;
            public float pitch { get; set; } = 1f;
            public float stereoPan { get; set; } = 1f;
            public float spatialBlend { get; set; } = 1f;
            public float reverbZoneMix { get; set; } = 1f;

            public bool playOnAwake { get; set; } = true;
            public bool loop { get; set; } = false;
            public bool deleteWhenFinished { get; set; } = true;

            public AudioMixer mixer = null;
        }
        
        
        private List<AudioSource> audioList;


        [SerializeField] private AudioMixer defaultMixer = null;

        [SerializeField] private GameObject audioInstance = null;
        #endregion




        #region Manage audio

        void Awake()
        {
            SHUU_GlobalsProxy.audioManager = this;


            audioList = new List<AudioSource>();
        }


        #region XML doc
        /// <summary>
        /// Creates a default audio instance (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audio">The audio to play.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public GameObject PlayAudioAt(Transform pos, AudioClip audio, AudioOptions audioOptions = null){
            if (audioOptions == null)
            {
                audioOptions = new AudioOptions();
            }
        

            AudioSource theSource = Instantiate(audioInstance, pos).GetComponent<AudioSource>();


            theSource.clip = audio;
            if (audioOptions.mixer != null) theSource.outputAudioMixerGroup = audioOptions.mixer.outputAudioMixerGroup;
            else if (defaultMixer != null) theSource.outputAudioMixerGroup = defaultMixer.outputAudioMixerGroup;

            theSource.priority = audioOptions.priority;
            theSource.volume = audioOptions.volume;
            theSource.pitch = audioOptions.pitch;
            theSource.panStereo = audioOptions.stereoPan;
            theSource.spatialBlend = audioOptions.spatialBlend;
            theSource.reverbZoneMix = audioOptions.reverbZoneMix;

            theSource.playOnAwake = audioOptions.playOnAwake;
            theSource.loop = audioOptions.loop;

            if (audioOptions.deleteWhenFinished)
            {
                theSource.gameObject.AddComponent<AudioSelfDestruct>();
            }


            if (theSource != null)
            {
                audioList.Add(theSource);
            }

            return theSource.gameObject;
        }
        
        #region XML doc
        /// <summary>
        /// Creates a default random audio instance from a list (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audioList">The list of audios to pick from.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public void PlayRandomAudioAt(Transform pos, List<AudioClip> audioList, AudioOptions audioOptions = null)
        {
            int voiceline = Random.Range(0, audioList.Count);

            PlayAudioAt(pos, audioList[voiceline], audioOptions);
        }
        
        #endregion



        #region Audio list stuff

        #region XML doc
        /// <summary>
        /// Gets the ammount of audios currently playing in the game.
        /// </summary>
        /// <returns>The the ammount of audios currently playing as an int.</returns>
        #endregion
        public int GetAudioCount()
        {
            return audioList.Count;
        }


        #region XML doc
        /// <summary>
        /// Destroys all audio instances currently playing in the game.
        /// </summary>
        #endregion
        public void ClearAllAudio(){
            foreach (AudioSource source in audioList)
            {
                Destroy(source.gameObject);
            }

            audioList.Clear();
        }

        #endregion
    }

}
