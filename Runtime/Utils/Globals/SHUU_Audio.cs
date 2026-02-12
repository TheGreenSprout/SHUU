using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SHUU.Utils.BaseScripts.Audio;
using SHUU.Utils.BaseScripts.ScriptableObjs.Audio;
using SHUU.Utils.Helpers;
using System.Linq;
using SHUU.Utils.SettingsSytem;

namespace SHUU.Utils.Globals
{

    #region Options
    public class SFX_Options
    {
        public int? priority { get; set; } = null;

        public float? volume { get; set; } = null;
        public float? pitch { get; set; } = null;
        public float? stereoPan { get; set; } = null;
        public float? spatialBlend { get; set; } = null;
        public float? reverbZoneMix { get; set; } = null;

        public bool? playOnAwake { get; set; } = null;
        public bool? loop { get; set; } = null;
        public bool deleteWhenFinished { get; set; } = true;

        public GameObject prefab = null;
        public AudioMixerGroup mixer = null;
    }


    public class Music_Options
    {
        public SFX_Options audioOptions = null;


        public bool sectionLooping = false;

        public bool startAtSection = false;
    }
    #endregion

    

    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Manages multiple audio-related functions.
    /// </summary>
    #endregion
    public class SHUU_Audio : MonoBehaviour
    {
        private static SHUU_Audio _instance;
        
        private static SHUU_Audio instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SHUU_Audio>(FindObjectsInactive.Include);

                    if (_instance == null) Debug.LogError("No SHUU_Audio found in scene.");
                }

                return _instance;
            }
        }



        #region Variables
        private const float MIN_DB = -80f;


        private AudioSystem sfxSystem = null;
        private AudioSystem musicSystem = null;


        [Header("General")]
        [SerializeField] private AudioMixerGroup masterMixer = null;
        [SerializeField] private AudioMixerGroup sfxMixer = null;
        [SerializeField] private AudioMixerGroup musicMixer = null;

        [SerializeField] private SfxStorage sfxStorage;
        [SerializeField] private MusicStorage musicStorage;


        [SerializeField] private GameObject audioInstance = null;



        [Header("Settings")]
        [SerializeField] private SettingsData settingsData = null;

        [SerializeField] private string masterAudio_fieldName = "MasterVolume";
        [SerializeField] private string sfxAudio_fieldName = "SfxSVolume";
        [SerializeField] private string musicAudio_fieldName = "MusicVolume";



        [Header("Object Pooling")]
        [Tooltip("If 0, the sfx system won't use an object pool.")]
        [SerializeField] [Min(0)] private int sfxObjectPool_initialSize = 10;

        [Tooltip("If 0, the music system won't use an object pool.")]
        [SerializeField] [Min(0)] private int musicObjectPool_initialSize = 0;


        [SerializeField] private Transform sfxPoolObjects_parent = null;
        [SerializeField] private Transform musicPoolObjects_parent = null;
        #endregion



        
        #region Init
        private void Awake()
        {
            if (_instance == null) _instance = this;


            if (settingsData != null)
            {
                settingsData.OnSettingsChanged += SettingsUpdate;
                SettingsUpdate(null);
            }


            if (sfxObjectPool_initialSize == 0) sfxSystem = new Basic_AudioSystem(audioInstance, false);
            else sfxSystem = new ObjectPool_AudioSystem(audioInstance, false, sfxObjectPool_initialSize, sfxPoolObjects_parent);

            if (musicObjectPool_initialSize == 0) musicSystem = new Basic_AudioSystem(audioInstance, true);
            else musicSystem = new ObjectPool_AudioSystem(audioInstance, true, musicObjectPool_initialSize, musicPoolObjects_parent);
        }

        private void OnDestroy()
        {
            if (settingsData != null) settingsData.OnSettingsChanged -= SettingsUpdate;
        }


        private void SettingsUpdate(string field)
        {
            if (settingsData == null || field != null && field != masterAudio_fieldName && field != sfxAudio_fieldName && field != musicAudio_fieldName) return;


            float masterVolume = settingsData.GetFloat(masterAudio_fieldName);
            float sfxVolume = settingsData.GetFloat(sfxAudio_fieldName);
            float musicVolume = settingsData.GetFloat(musicAudio_fieldName);

            AudioMixer mixer = masterMixer.audioMixer;

            mixer.SetFloat("MasterVolume", PercentToDb(masterVolume));
            mixer.SetFloat("SFXVolume", PercentToDb(sfxVolume));
            mixer.SetFloat("MusicVolume", PercentToDb(musicVolume));
        }

        private float PercentToDb(float percent)
        {
            float linear = Mathf.Clamp01(percent / 100f);

            if (linear <= 0.0001f)
                return MIN_DB;

            return Mathf.Log10(linear) * 20f;
        }
        #endregion



        #region Manage SFX

        public static AudioSource PlaySfxAt(Transform pos, string audio, SFX_Options audioOptions = null) => instance._PlaySfxAt(pos, audio, audioOptions);
        private AudioSource _PlaySfxAt(Transform pos, string audio, SFX_Options audioOptions = null){
            if (sfxStorage == null) return null;

            return _PlaySfxAt(pos, sfxStorage.GetAudio(audio), audioOptions);
        }
        #region XML doc
        /// <summary>
        /// Creates a default audio instance (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audio">The audio to play.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public static AudioSource PlaySfxAt(Transform pos, AudioClip audio, SFX_Options audioOptions = null) => instance._PlaySfxAt(pos, audio, audioOptions);
        private AudioSource _PlaySfxAt(Transform pos, AudioClip audio, SFX_Options audioOptions = null){
            if (audio == null)
            {
                Debug.LogError("Null AudioClip!");
                
                return null;
            }


            if (audioOptions == null)
            {
                audioOptions = new SFX_Options();
            }
        
            
            AudioSource theSource = null;
            if (audioOptions.prefab != null || audioInstance != null) theSource = sfxSystem.InstantiateAudio(pos, audioOptions.prefab);
            else
            {
                Debug.LogError("No audio prefab assigned to AudioManager or SFX_Options!");

                return null;
            }

            sfxSystem.CreationCheck(theSource);
            

            theSource.clip = audio;
            if (audioOptions.mixer != null) theSource.outputAudioMixerGroup = audioOptions.mixer;
            else if (sfxMixer != null) theSource.outputAudioMixerGroup = sfxMixer;

            if (audioOptions.priority != null) theSource.priority = audioOptions.priority.Value;
            if (audioOptions.volume != null) theSource.volume = audioOptions.volume.Value;
            if (audioOptions.pitch != null) theSource.pitch = audioOptions.pitch.Value;
            if (audioOptions.stereoPan != null) theSource.panStereo = audioOptions.stereoPan.Value;
            if (audioOptions.spatialBlend != null) theSource.spatialBlend = audioOptions.spatialBlend.Value;
            if (audioOptions.reverbZoneMix != null) theSource.reverbZoneMix = audioOptions.reverbZoneMix.Value;

            if (audioOptions.loop != null) theSource.loop = audioOptions.loop.Value;


            AudioSelfDestruct selfDestruct = theSource.gameObject.AddComponent<AudioSelfDestruct>();
            sfxSystem.SetupSelfDestruct(selfDestruct, audioOptions.deleteWhenFinished);


            if (audioOptions.playOnAwake != null) theSource.Play();
            else if (theSource.playOnAwake) theSource.Play();

            theSource.playOnAwake = false;


            return theSource;
        }
        
        public static AudioSource PlayRandomSfxAt(Transform pos, string[] audio, SFX_Options audioOptions = null) => instance._PlayRandomSfxAt(pos, audio, audioOptions);
        private AudioSource _PlayRandomSfxAt(Transform pos, string[] audio, SFX_Options audioOptions = null){
            if (sfxStorage == null) return null;

            List<AudioClip> audioList = new();

            foreach (string clip in audio)
            {
                audioList.Add(sfxStorage.GetAudio(clip));
            }

            return _PlayRandomSfxAt(pos, audioList, audioOptions);
        }
        #region XML doc
        /// <summary>
        /// Creates a default random audio instance from a list (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audioList">The list of audios to pick from.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public static AudioSource PlayRandomSfxAt(Transform pos, List<AudioClip> audioList, SFX_Options audioOptions = null) => instance._PlayRandomSfxAt(pos, audioList, audioOptions);
        private AudioSource _PlayRandomSfxAt(Transform pos, List<AudioClip> audioList, SFX_Options audioOptions = null)
        {
            if (audioList == null || audioList.Count == 0) return null;


            int voiceline = Random.Range(0, audioList.Count);

            return _PlaySfxAt(pos, audioList[voiceline], audioOptions);
        }
        
        #endregion



        #region Manage Music

        public static AudioSource PlayMusicAt(Transform pos, string audio, Music_Options musicOptions = null) => instance._PlayMusicAt(pos, audio, musicOptions);
        private AudioSource _PlayMusicAt(Transform pos, string audio, Music_Options musicOptions = null){
            if (sfxStorage == null) return null;

            return PlayMusicAt(pos, musicStorage.GetAudio(audio), musicOptions);
        }
        #region XML doc
        /// <summary>
        /// Creates a default audio instance (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audio">The audio to play.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public static AudioSource PlayMusicAt(Transform pos, Music_Set set, Music_Options musicOptions = null) => instance._PlayMusicAt(pos, set, musicOptions);
        private AudioSource _PlayMusicAt(Transform pos, Music_Set set, Music_Options musicOptions = null){
            if (set == null || set.music == null)
            {
                Debug.LogError("Null set or AudioClip!");
                
                return null;
            }


            SFX_Options audioOptions = musicOptions.audioOptions;
            if (audioOptions == null)
            {
                audioOptions = new SFX_Options();
            }
        
            
            AudioSource theSource = null;
            if (audioOptions.prefab != null || audioInstance != null) theSource = musicSystem.InstantiateAudio(pos, audioOptions.prefab);
            else
            {
                Debug.LogError("No audio prefab assigned to AudioManager or SFX_Options!");

                return null;
            }

            musicSystem.CreationCheck(theSource);
            

            theSource.clip = set.music;
            if (audioOptions.mixer != null) theSource.outputAudioMixerGroup = audioOptions.mixer;
            else if (musicMixer != null) theSource.outputAudioMixerGroup = musicMixer;

            if (audioOptions.priority != null) theSource.priority = audioOptions.priority.Value;
            if (audioOptions.volume != null) theSource.volume = audioOptions.volume.Value;
            if (audioOptions.pitch != null) theSource.pitch = audioOptions.pitch.Value;
            if (audioOptions.stereoPan != null) theSource.panStereo = audioOptions.stereoPan.Value;
            if (audioOptions.spatialBlend != null) theSource.spatialBlend = audioOptions.spatialBlend.Value;
            if (audioOptions.reverbZoneMix != null) theSource.reverbZoneMix = audioOptions.reverbZoneMix.Value;

            if (audioOptions.playOnAwake != null) theSource.playOnAwake = audioOptions.playOnAwake.Value;
            if (audioOptions.loop != null) theSource.loop = audioOptions.loop.Value;


            AudioSelfDestruct selfDestruct = theSource.gameObject.AddComponent<AudioSelfDestruct>();
            musicSystem.SetupSelfDestruct(selfDestruct, audioOptions.deleteWhenFinished);


            if (musicOptions.sectionLooping && set.loopSections != null) theSource.gameObject.AddComponent<MusicLooper>().Setup(theSource, set.loopSections, musicOptions.startAtSection);


            if (!theSource.enabled) theSource.enabled = true;

            return theSource;
        }
        
        public static AudioSource PlayRandomMusicAt(Transform pos, string[] audio, Music_Options musicOptions = null) => instance._PlayRandomMusicAt(pos, audio, musicOptions);
        private AudioSource _PlayRandomMusicAt(Transform pos, string[] audio, Music_Options audioOptions = null){
            if (sfxStorage == null) return null;

            List<Music_Set> audioList = new();

            foreach (string clip in audio)
            {
                audioList.Add(musicStorage.GetAudio(clip));
            }

            return PlayRandomMusicAt(pos, audioList, audioOptions);
        }
        #region XML doc
        /// <summary>
        /// Creates a default random audio instance from a list (with custom volume) at a position.
        /// </summary>
        /// <param name="pos">The position where the audio will be created at.</param>
        /// <param name="audioList">The list of audios to pick from.</param>
        /// <param name="volume">The volume the audio will play at.</param>
        #endregion
        public static AudioSource PlayRandomMusicAt(Transform pos, List<Music_Set> setList, Music_Options musicOptions = null) => instance._PlayRandomMusicAt(pos, setList, musicOptions);
        private AudioSource _PlayRandomMusicAt(Transform pos, List<Music_Set> setList, Music_Options audioOptions = null)
        {
            if (setList == null || setList.Count == 0) return null;


            int voiceline = Random.Range(0, setList.Count);

            return PlayMusicAt(pos, setList[voiceline], audioOptions);
        }
        
        #endregion



        #region Audio list stuff

        #region XML doc
        /// <summary>
        /// Gets all audios currently playing in the game.
        /// </summary>
        /// <returns>All audios currently playing as an int.</returns>
        #endregion
        public static AudioSource[] GetAllAudio() => GetAllSFX().Concat(GetAllMusic()).ToArray();
        #region XML doc
        /// <summary>
        /// Gets all sfx audios currently playing in the game.
        /// </summary>
        /// <returns>All sfx audios currently playing as an int.</returns>
        #endregion
        public static AudioSource[] GetAllSFX() => instance.sfxSystem.GetAllAudio();
        #region XML doc
        /// <summary>
        /// Gets all music audios currently playing in the game.
        /// </summary>
        /// <returns>All music audios currently playing as an int.</returns>
        #endregion
        public static AudioSource[] GetAllMusic() => instance.musicSystem.GetAllAudio();

        #region XML doc
        /// <summary>
        /// Gets the ammount of audios currently playing in the game.
        /// </summary>
        /// <returns>The the ammount of audios currently playing as an int.</returns>
        #endregion
        public static int GetAllAudio_Count() => GetAllSFX_Count() + GetAllMusic_Count();
        #region XML doc
        /// <summary>
        /// Gets the ammount of sfx audios currently playing in the game.
        /// </summary>
        /// <returns>The the ammount of sfx audios currently playing as an int.</returns>
        #endregion
        public static int GetAllSFX_Count() => instance.sfxSystem.GetAudioCount();
        #region XML doc
        /// <summary>
        /// Gets the ammount of music audios currently playing in the game.
        /// </summary>
        /// <returns>The the ammount of music audios currently playing as an int.</returns>
        #endregion
        public static int GetAllMusic_Count() => instance.musicSystem.GetAudioCount();


        #region XML doc
        /// <summary>
        /// Destroys all audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ClearAllAudio(){
            ClearAllSFX();
            ClearAllMusic();
        }
        #region XML doc
        /// <summary>
        /// Destroys all sfx audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ClearAllSFX() => ClearAllAudio(instance.sfxSystem);
        #region XML doc
        /// <summary>
        /// Destroys all music audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ClearAllMusic() => ClearAllAudio(instance.sfxSystem);
        private static void ClearAllAudio(AudioSystem audioSystem){
            foreach (AudioSource source in audioSystem.GetAllAudio()) if (source.gameObject.TryGetComponent(out AudioSelfDestruct selfDestruct)) selfDestruct.DestroySource();

            audioSystem.ClearAudioList();
        }

        #region XML doc
        /// <summary>
        /// Pauses all audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void PauseAllAudio(){
            PauseAllSfx();
            PauseAllMusic();
        }
        #region XML doc
        /// <summary>
        /// Pauses all sfx audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void PauseAllSfx() => PauseAllAudio(instance.sfxSystem);
        #region XML doc
        /// <summary>
        /// Pauses all music audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void PauseAllMusic() => PauseAllAudio(instance.musicSystem);
        private static void PauseAllAudio(AudioSystem audioSystem){
            foreach (AudioSource source in audioSystem.GetAllAudio()) source.Pause();
        }

        #region XML doc
        /// <summary>
        /// Resumes all audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ResumeAllAudio(){
            ResumeAllSfx();
            ResumeAllMusic();
        }
        #region XML doc
        /// <summary>
        /// Resumes all sfx audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ResumeAllSfx() => ResumeAllAudio(instance.sfxSystem);
        #region XML doc
        /// <summary>
        /// Resumes all music audio instances currently playing in the game.
        /// </summary>
        #endregion
        public static void ResumeAllMusic() => ResumeAllAudio(instance.musicSystem);
        public static void ResumeAllAudio(AudioSystem audioSystem){
            foreach (AudioSource source in audioSystem.GetAllAudio()) source.UnPause();
        }

        #endregion
    }




    #region Audio Systems
    public abstract class AudioSystem
    {
        protected bool isMusic = false;


        public virtual void CreationCheck(AudioSource source) { }

        public abstract AudioSource InstantiateAudio(Transform pos, GameObject overridePrefab = null);
        public abstract void SetupSelfDestruct(AudioSelfDestruct selfDestruct, bool deleteWhenFinished);

        public abstract AudioSource[] GetAllAudio();
        public abstract int GetAudioCount();
        public virtual void ClearAudioList() { }
    }



    public class Basic_AudioSystem : AudioSystem
    {
        private GameObject prefab = null;

        private List<AudioSource> audioList = new();


        public Basic_AudioSystem(GameObject _prefab, bool _isMusic)
        {
            isMusic = _isMusic;
            
            prefab = _prefab;
        }

        public override AudioSource InstantiateAudio(Transform pos, GameObject overridePrefab)
        {
            AudioSource source;
            if (overridePrefab == null) source = Object.Instantiate(prefab, pos).GetComponent<AudioSource>();
            else source = Object.Instantiate(overridePrefab, pos).GetComponent<AudioSource>();
            audioList.Add(source);

            return source;
        }
        public override void SetupSelfDestruct(AudioSelfDestruct selfDestruct, bool deleteWhenFinished) => selfDestruct.Setup(deleteWhenFinished);

        public override AudioSource[] GetAllAudio()
        {
            audioList.Clean();

            return audioList.ToArray();
        }
        public override int GetAudioCount()
        {
            audioList.Clean();

            return audioList.Count;
        }
        public override void ClearAudioList() => audioList.Clear();
    }


    public class ObjectPool_AudioSystem : AudioSystem
    {
        private SHUU_ObjectPool<AudioSource> pool = null;


        public ObjectPool_AudioSystem(GameObject _prefab, bool _isMusic, int initialPoolSize, Transform parent)
        {
            isMusic = _isMusic;

            pool = new SHUU_ObjectPool<AudioSource>(_prefab.GetComponent<AudioSource>(), initialPoolSize, parent);
        }

        public override AudioSource InstantiateAudio(Transform pos, GameObject overridePrefab)
        {
            bool canRecycle = !isMusic;


            AudioSource source;
            if (overridePrefab == null)
            {
                source = pool.Get();

                source.gameObject.transform.SetParent(pos, false);
            }
            else
            {
                source = Object.Instantiate(overridePrefab, pos).GetComponent<AudioSource>();
                pool.ForceAdd_Active(source);

                canRecycle = false;
            }

            pool.SetCanRecycle(source, canRecycle);


            if (!source.gameObject.TryGetComponent(out AudioPoolingHelper poolingHelper)) source.gameObject.AddComponent<AudioPoolingHelper>().Setup();


            return source;
        }
        public override void SetupSelfDestruct(AudioSelfDestruct selfDestruct, bool deleteWhenFinished) => selfDestruct.Setup(deleteWhenFinished, pool);

        public override AudioSource[] GetAllAudio() => pool.GetActives();
        public override int GetAudioCount() => pool.activesCount;
    }
    #endregion
}
