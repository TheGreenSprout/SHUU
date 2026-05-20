using System;
using UnityEngine;

namespace SHUU.Utils.BaseScripts.ScriptableObjs.Audio
{
    #region SFX
    [Serializable]
    public class SFX_Instance
    {
        public string IDENTIFIER = "SFX_ID";
        public AudioClip soundEffect = null;
    }
    #endregion


    
    #region Music
    [Serializable]
    public class Music_Instance
    {
        public string IDENTIFIER = "Music_ID";
        public AudioClip music = null;

        public Music_LoopSections loopSlices = new();
    }

    public class Music_Set
    {
        public AudioClip music = null;

        public Music_LoopSections loopSections;
    }


    [Serializable]
    public class Music_LoopSections
    {
        public float startPoint;
        public float endPoint;
    }
    #endregion
}
