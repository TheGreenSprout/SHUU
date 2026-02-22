using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.BaseScripts.ScriptableObjs.Audio
{
    [CreateAssetMenu(fileName = "Music_Storage", menuName = "SHUU/Audio/Music_Storage")]
    public class Music_Storage : ScriptableObject
    {
        public Music_Instance[] allMusic;


        private Dictionary<string, Music_Set> allMusic_dict;




        private void OnEnable()
        {
            allMusic_dict = new();

            if (allMusic != null)
            {
                foreach (var audio in allMusic)
                {
                    if (audio != null && audio.music != null && !string.IsNullOrEmpty(audio.IDENTIFIER))
                    {
                        if (!allMusic_dict.ContainsKey(audio.IDENTIFIER)) allMusic_dict.Add(audio.IDENTIFIER, new Music_Set { music = audio.music, loopSections = audio.loopSlices });
                        #if UNITY_EDITOR
                        else Debug.LogWarning($"Duplicate audio identifier: {audio.IDENTIFIER}");
                        #endif
                    }
                }
            }
        }


        public Music_Set GetAudio(string id)
        {
            if (allMusic_dict == null) return null;


            if (allMusic_dict.TryGetValue(id, out Music_Set set)) return set;

            return null;
        }
    }
}
