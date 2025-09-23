using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SHUU.Utils.BaseScripts;

namespace SHUU.Utils.Helpers
{

#region XML doc
/// <summary>
/// Manages multiple audio-related functions.
/// </summary>
#endregion
public class AudioManager : MonoBehaviour
{
    #region Variables
    
    private static List<AudioSource> audioList;

    private static GameObject audioInstance;

    private static AudioMixer defaultMixer;



    public static int defaultPriority = 128;
    
    public static float defaultVolume = 1f;
    public static float defaultPitch = 1f;
    public static float defaultStereoPan = 1f;
    public static float defaultSpatialBlend = 1f;
    public static float defaultReverbZoneMix = 1f;

    public static bool defaultPlayOnAwake = true;
    public static bool defaultLoop = false;
    public static bool defaultDeleteWhenFinished = true;

    #endregion




    #region Manage audio

    void Awake(){
        audioList = new List<AudioSource>();

        audioInstance = Resources.Load<GameObject>("AudioInstance");
        defaultMixer = Resources.Load<AudioMixer>("DefaultMixer");
    }


    #region XML doc
    /// <summary>
    /// Creates a default audio instance (with custom volume) at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audio">The audio to play.</param>
    /// <param name="volume">The volume the audio will play at.</param>
    #endregion
    public static GameObject SpawnAudioAt(Transform pos, AudioClip audio, float volume){
        AudioSource theSource = Instantiate(audioInstance, pos).GetComponent<AudioSource>();


        theSource.clip = audio;
        //theSource.outputAudioMixerGroup = defaultMixer.outputAudioMixerGroup;

        theSource.priority = defaultPriority;
        theSource.volume = volume;
        theSource.pitch = defaultPitch;
        theSource.panStereo = defaultStereoPan;
        theSource.spatialBlend = defaultSpatialBlend;
        theSource.reverbZoneMix = defaultReverbZoneMix;

        theSource.playOnAwake = defaultPlayOnAwake;
        theSource.loop = defaultLoop;

        if (defaultDeleteWhenFinished)
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
    /// Creates a default audio instance at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audio">The audio to play.</param>
    /// <returns>The audio object.</returns>
    #endregion
    public static GameObject SpawnDefaultAudioAt(Transform pos, AudioClip audio){
        AudioSource theSource = Instantiate(audioInstance, pos).GetComponent<AudioSource>();


        theSource.clip = audio;
        //theSource.outputAudioMixerGroup = defaultMixer.outputAudioMixerGroup;

        theSource.priority = defaultPriority;
        theSource.volume = defaultVolume;
        theSource.pitch = defaultPitch;
        theSource.panStereo = defaultStereoPan;
        theSource.spatialBlend = defaultSpatialBlend;
        theSource.reverbZoneMix = defaultReverbZoneMix;

        theSource.playOnAwake = defaultPlayOnAwake;
        theSource.loop = defaultLoop;

        if (defaultDeleteWhenFinished)
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
    /// Creates a custom audio instance at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audio">The audio to play.</param>
    /// <param name="priority">The priority the audio will play have.</param>
    /// <param name="volume">The volume the audio will play at.</param>
    /// <param name="pitch">The pitch the audio will play at.</param>
    /// <param name="stereoPan">The stereo pan the audio will play have.</param>
    /// <param name="spatialBlend">The spatial blend the audio will play have.</param>
    /// <param name="reverbZoneMix">The reverb zone mix the audio will play have.</param>
    /// <param name="playOnAwake">Wether the audio will play when created.</param>
    /// <param name="loop">Wether the audio will loop.</param>
    /// <param name="deleteWhenFinished">Wether the audio will get deleted when it stops playing.</param>
    /// <returns>The audio object.</returns>
    #endregion
    public static GameObject SpawnCustomAudioAt(Transform pos, AudioClip audio, int priority, float volume, float pitch, float stereoPan, float spatialBlend, float reverbZoneMix, bool playOnAwake, bool loop, bool deleteWhenFinished){
        AudioSource theSource = Instantiate(audioInstance, pos).GetComponent<AudioSource>();


        theSource.clip = audio;
        //theSource.outputAudioMixerGroup = defaultMixer.outputAudioMixerGroup;

        theSource.priority = priority;
        theSource.volume = volume;
        theSource.pitch = pitch;
        theSource.panStereo = stereoPan;
        theSource.spatialBlend = spatialBlend;
        theSource.reverbZoneMix = reverbZoneMix;

        theSource.playOnAwake = playOnAwake;
        theSource.loop = loop;

        if (deleteWhenFinished)
        {
            theSource.gameObject.AddComponent<AudioSelfDestruct>();
        }


        if (theSource != null)
        {
            audioList.Add(theSource);
        }
        
        return theSource.gameObject;
    }

    #endregion


    #region Randomized audio
    
    #region XML doc
    /// <summary>
    /// Creates a default random audio instance from a list (with custom volume) at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audioList">The list of audios to pick from.</param>
    /// <param name="volume">The volume the audio will play at.</param>
    #endregion
    public static void PlayRandomLineAt(Transform pos, List<AudioClip> audioList, float volume)
    {
        int voiceline = Random.Range(0, audioList.Count);

        SpawnAudioAt(pos, audioList[voiceline], volume);
    }
    
    #region XML doc
    /// <summary>
    /// Creates a default random audio instance from a list at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audioList">The list of audios to pick from.</param>
    #endregion
    public static void PlayRandomDefaultLineAt(Transform pos, List<AudioClip> audioList)
    {
        int voiceline = Random.Range(0, audioList.Count);

        SpawnDefaultAudioAt(pos, audioList[voiceline]);
    }
    
    #region XML doc
    /// <summary>
    /// Creates a custom random audio instance from a list instance at a position.
    /// </summary>
    /// <param name="pos">The position where the audio will be created at.</param>
    /// <param name="audioList">The list of audios to pick from.</param>
    /// <param name="priority">The priority the audio will play have.</param>
    /// <param name="volume">The volume the audio will play at.</param>
    /// <param name="pitch">The pitch the audio will play at.</param>
    /// <param name="stereoPan">The stereo pan the audio will play have.</param>
    /// <param name="spatialBlend">The spatial blend the audio will play have.</param>
    /// <param name="reverbZoneMix">The reverb zone mix the audio will play have.</param>
    /// <param name="playOnAwake">Wether the audio will play when created.</param>
    /// <param name="loop">Wether the audio will loop.</param>
    /// <param name="deleteWhenFinished">Wether the audio will get deleted when it stops playing.</param>
    #endregion
    public static void PlayRandomCustomLineAt(Transform pos, List<AudioClip> audioList, int priority, float volume, float pitch, float stereoPan, float spatialBlend, float reverbZoneMix, bool playOnAwake, bool loop, bool deleteWhenFinished)
    {
        int voiceline = Random.Range(0, audioList.Count);

        SpawnCustomAudioAt(pos, audioList[voiceline], priority, volume, pitch, stereoPan, spatialBlend, reverbZoneMix, playOnAwake, loop, deleteWhenFinished);
    }
    
    #endregion



    #region Audio list stuff

    #region XML doc
    /// <summary>
    /// Gets the ammount of audios currently playing in the game.
    /// </summary>
    /// <returns>The the ammount of audios currently playing as an int.</returns>
    #endregion
    public static int GetAudioCount()
    {
        return audioList.Count;
    }


    #region XML doc
    /// <summary>
    /// Destroys all audio instances currently playing in the game.
    /// </summary>
    #endregion
    public static void ClearAllAudio(){
        foreach (AudioSource source in audioList)
        {
            Destroy(source.gameObject);
        }

        audioList.Clear();
    }

    #endregion
}

}
