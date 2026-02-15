using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.BaseScripts.ScriptableObjs.Audio
{
    [CreateAssetMenu(fileName = "SFX_Storage", menuName = "SHUU/Audio/SFX_Storage")]
    public class Sfx_Storage : ScriptableObject
    {
        public SFX_Instance[] allSFX;
        

        private Dictionary<string, AudioClip> allSFX_dict;




        private void OnEnable()
        {
            allSFX_dict = new();

            if (allSFX != null)
            {
                foreach (var audio in allSFX)
                {
                    if (audio != null && audio.soundEffect != null && !string.IsNullOrEmpty(audio.IDENTIFIER))
                    {
                        if (!allSFX_dict.ContainsKey(audio.IDENTIFIER)) allSFX_dict.Add(audio.IDENTIFIER, audio.soundEffect);
                        #if UNITY_EDITOR
                        else Debug.LogWarning($"Duplicate audio identifier: {audio.IDENTIFIER}");
                        #endif
                    }
                }
            }
        }


        public AudioClip GetAudio(string id)
        {
            if (allSFX_dict == null) return null;


            if (allSFX_dict.TryGetValue(id, out AudioClip clip)) return clip;

            return null;
        }
    }
}
