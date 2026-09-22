using UnityEngine;
using System.Collections.Generic;

using SHUU.UserSide.Addons.AudioSystem.ScriptableObjects;
using SHUU.Utils.Helpers;

namespace SHUU.UserSide.Addons.AudioSystem
{
    public interface IAudioChannel
    {
        #region Main
        internal void Init(IAudioChannel parent);
        #endregion



        #region Internal
        internal string GetID();


        internal SHUU_AudioInstance.Options GetOptions();

        internal AudioLookup GetAudioLookup();

        internal SHUU_ObjectPool<SHUU_AudioInstance> GetObjectPool();
        internal SHUU_AudioInstance GetObjectPool_Prefab();
        internal SHUU_AudioObjectPoolParent GetObjectPool_ParentPrefab();
        internal Transform GetObjectPool_Parent();

        internal IAudioChannel GetChild(string path);
        internal IEnumerable<IAudioChannel> GetChildren();
        internal string GetPath(bool includeRoot = true);
        #endregion



        #region Logic
        public SHUU_AudioInstance GetAudioInstance(SHUU_AudioInstance.Options options);
        #endregion
    }
}
