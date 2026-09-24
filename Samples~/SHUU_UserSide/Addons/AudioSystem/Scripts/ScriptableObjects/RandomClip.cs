using System.Collections.Generic;
using UnityEngine;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.UserSide.Addons
{
    [CreateAssetMenu(fileName = "RandomClip", menuName = "SHUU/Audio System/RandomClip")]
    public class RandomClip : ScriptableObject
    {
        #region Variables
        [SerializeField] private List<AudioClip> audioClips = new();
        #endregion




        #region Logic
        public AudioClip GetClip() => audioClips.RandomElement();
        #endregion
    }
}
