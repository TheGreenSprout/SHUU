using System.Collections.Generic;
using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.UserSide.Addons
{
    [CreateAssetMenu(fileName = "Random_AudioClip", menuName = "SHUU/Audio System/Random_AudioClip")]
    public class Random_AudioClip : ScriptableObject
    {
        #region Variables
        [SerializeField] private List<AudioClip> audioClips = new();
        #endregion




        #region Logic
        public AudioClip GetClip() => audioClips.RandomElement();
        #endregion
    }
}
