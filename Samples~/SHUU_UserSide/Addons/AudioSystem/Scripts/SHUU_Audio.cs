using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SHUU.Utils.Helpers;
using SHUU.UserSide.Addons.AudioSystem.ScriptableObjects;

namespace SHUU.UserSide.Addons.AudioSystem
{
    [DefaultExecutionOrder(-10000)]
    #region XML doc
    /// <summary>
    /// Manages multiple audio-related functions.
    /// </summary>
    #endregion
    public class SHUU_Audio : HiddenSingleton_MonoBehaviour<SHUU_Audio>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        [SerializeField] private AudioChannelGroup audioChannelGroup;
        private static IAudioChannel IaudioChannelGroup => instance?.audioChannelGroup;


        private static Dictionary<string, AudioLookup> AudioLookupDict = null;
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();


            if (IaudioChannelGroup != null) IaudioChannelGroup.Init(null);
            else Debug.LogError("AudioChannelGroup is null! Please assign an AudioChannelGroup to the SHUU_Audio component in the scene.");
            
            FetchLookups(true);
        }
        #endregion



        #region Logic
        private void FetchLookups(bool force = false)
        {
            if (AudioLookupDict != null && !force) return;

            if (audioChannelGroup != null)
            {
                AudioLookupDict = new();
                AudioLookupDict[IaudioChannelGroup.GetPath(true)] = IaudioChannelGroup.GetAudioLookup();

                foreach (var channel in audioChannelGroup.GetAllChildren())
                    AudioLookupDict[channel.GetPath(true)] = channel.GetAudioLookup();
            }
        }


        public static IAudioChannel GetChannel(string channelPath) => IaudioChannelGroup?.GetChild(channelPath);

        public static AudioLookup GetLookup(string channelPath)
        {
            Instance?.FetchLookups();

            AudioLookupDict.TryGetValue(channelPath, out var lookup);
            return lookup;
        }

        public static SHUU_ObjectPool<SHUU_AudioInstance> GetPool(string channelPath) => GetChannel(channelPath)?.GetObjectPool();


        public static AudioLookup.LookupItem GetClipFromLookup(string clipName, string channelPath)
        {
            string path = channelPath.StartsWith("General/") ? channelPath : "General/" + channelPath;
            if (!AudioLookupDict.ContainsKey(path))
            {
                Debug.LogError($"AudioLookup not found for channel path: {path}. Please check the channel path.");
                return null;
            }

            AudioLookup lookup = GetLookup(path);
            if (lookup == null)
            {
                Debug.LogError($"AudioLookup is null for channel path: {path}. Please ensure the channel has a valid AudioLookup assigned.");
                return null;
            }

            if (!lookup.TryGetLookupItem(clipName, out AudioLookup.LookupItem item))
            {
                Debug.LogError($"AudioClip '{clipName}' not found in AudioLookup for channel path: {path}. Please check the clip name and AudioLookup configuration.");
                return null;
            }
            return item;
        }


        public static SHUU_AudioInstance GetAudioInstance(string channelPath, Transform parent = null, SHUU_AudioInstance.Options options = null)
        {
            if (IaudioChannelGroup == null) Debug.LogError("AudioChannelGroup is null! Please assign an AudioChannelGroup to the SHUU_Audio component in the scene.");
            else
            {
                IAudioChannel channel = GetChannel(channelPath);

                if (channel == null) Debug.LogError($"AudioChannel not found at path: {channelPath}. Please check the channel path.");
                else
                {
                    SHUU_AudioInstance audioInstance = channel.GetAudioInstance(options);
                    if (audioInstance != null) Debug.LogError("Failed to get an audio instance from the channel. Please check the channel's configuration.");
                    
                    audioInstance.gameObject.transform.SetParent(parent, true);
                    return audioInstance;
                }
            }

            return null;
        }

        public static SHUU_AudioInstance PlayAudio(AudioClip clip, string channelPath = null, Transform parent = null, SHUU_AudioInstance.Options options = null)
        {
            if (clip == null) 
            {
                Debug.LogError("AudioClip is null! Please provide a valid AudioClip to play.");

                return null;
            }

            
            var instance = GetAudioInstance(channelPath, parent, options);

            if (instance) instance.clip = clip;
            instance?.Play();

            return instance;
        }
        public static SHUU_AudioInstance PlayAudio(string clipName, string channelPath = null, Transform parent = null, SHUU_AudioInstance.Options options = null)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                Debug.LogError("Clip name is null or empty! Please provide a valid clip name to play.");

                return null;
            }


            var lookupItem = GetClipFromLookup(clipName, channelPath);

            SHUU_AudioInstance.Options newOptions = options.InheritOptionsNewClass(lookupItem.options);

            
            var instance = GetAudioInstance(channelPath, parent, newOptions);

            if (instance) instance.clip = lookupItem.clip;
            instance?.Play();

            return instance;
        }
        #endregion
    
    
    
        #region Helpers
        public static AudioClip GetRandomClip(IEnumerable<AudioClip> clips)
        {
            if (clips == null || !clips.Any())
            {
                Debug.LogError("The provided collection of AudioClips is null or empty. Please provide a valid collection.");
                return null;
            }

            return clips.RandomElement();
        }

        public static string GetRandomClipName(IEnumerable<string> clipNames)
        {
            if (clipNames == null || !clipNames.Any())
            {
                Debug.LogError("The provided collection of clip names is null or empty. Please provide a valid collection.");
                return null;
            }

            return clipNames.RandomElement();
        }
        #endregion
    }
}