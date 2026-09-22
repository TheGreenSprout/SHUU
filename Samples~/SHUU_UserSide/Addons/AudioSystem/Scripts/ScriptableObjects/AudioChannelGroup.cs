using System;
using System.Collections.Generic;
using UnityEngine;

using Alchemy.Inspector;

using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.AudioSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AudioChannelGroup", menuName = "SHUU/Audio System/AudioChannelGroup")]
    public partial class AudioChannelGroup : ScriptableObject, IAudioChannel
    {
        #region Variables
        [SerializeField] private string ID = "Master";


        [SerializeReference] private List<AudioChannel> children = new();



        [FoldoutGroup("Parameters")]
        [SerializeField] private SHUU_AudioInstance.Options options = new();

        [FoldoutGroup("Parameters")]
        [SerializeField] private AudioLookup audioLookup = null;


        [FoldoutGroup("Parameters/ObjectPool Settings")]
        [SerializeField, Min(0)] private int initialSize = 10;
        [FoldoutGroup("Parameters/ObjectPool Settings")]
        [SerializeField] internal SHUU_AudioObjectPoolParent parentPrefab = null;
        [FoldoutGroup("Parameters/ObjectPool Settings"), Required]
        [SerializeField] private SHUU_AudioInstance prefab = null;
        [FoldoutGroup("Parameters/ObjectPool Settings")]
        [SerializeField] internal bool scenePersistant = true;

        private static SHUU_ObjectPool<SHUU_AudioInstance> ObjectPool = null;
        private static string CurrentScene = null;



        private static Dictionary<string, IAudioChannel> ChildrenDict = new();
        private static bool Init = false;



        internal static bool PersistantPool = false;
        #endregion




        #region Main
        void IAudioChannel.Init(IAudioChannel parent)
        {
            if (scenePersistant) PersistantPool = SHUU_AudioObjectPoolParent.Instance != null;
            else PersistantPool = false;
            
            ReloadObjectPool();

            foreach (IAudioChannel child in children)
            {
                if (!Init) ChildrenDict[child.GetID()] = child;
                child.Init(this);
            }
            PersistantPool = scenePersistant;
                
            if (!Init) Init = true;
        }
        #endregion



        #region Override points

        #region Internal
        string IAudioChannel.GetID() => ID;


        SHUU_AudioInstance.Options IAudioChannel.GetOptions() => options;

        AudioLookup IAudioChannel.GetAudioLookup() => audioLookup;

        SHUU_ObjectPool<SHUU_AudioInstance> IAudioChannel.GetObjectPool()
        {
            ReloadObjectPool();

            return ObjectPool;
        }
        SHUU_AudioInstance IAudioChannel.GetObjectPool_Prefab() => prefab;
        SHUU_AudioObjectPoolParent IAudioChannel.GetObjectPool_ParentPrefab() => parentPrefab;
        Transform IAudioChannel.GetObjectPool_Parent() => ObjectPool?.GetParent();

        IAudioChannel IAudioChannel.GetChild(string path)
        {
            if (path.StartsWith("/")) path = path.Substring(1);
            else if (path.StartsWith($"{ID}/")) path = path.Substring(ID.Length + 1);
            else if (path == ID) path = "";

            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            if (string.IsNullOrEmpty(path)) return this;


            int index = path.IndexOf("/");
            string childID = index == -1 ? path : path.Substring(0, index);
            string remainingPath = index == -1 ? "" : path.Substring(index + 1);

            return ChildrenDict[childID].GetChild(remainingPath);
        }
        IEnumerable<IAudioChannel> IAudioChannel.GetChildren() => children;
        string IAudioChannel.GetPath(bool includeRoot)
        {
            if (includeRoot) return ID;
            else return "";
        }
        #endregion


        public SHUU_AudioInstance GetAudioInstance(SHUU_AudioInstance.Options options)
        {
            var instanceOptions = new SHUU_AudioInstance.Options(options);
            instanceOptions.InheritOptions(this.options);
            
            var ObjectPool = ((IAudioChannel)this).GetObjectPool();
            var ret = ObjectPool.Get();
            ret.Init(ObjectPool, instanceOptions);

            return ret;
        }

        #endregion



        #region Logic
        internal IEnumerable<IAudioChannel> GetAllChildren() => ChildrenDict.Values;

        private void ReloadObjectPool()
        {
            var name = SceneLoader.GetCurrentSceneName();
            if (string.IsNullOrEmpty(CurrentScene) || !CurrentScene.Equals(name) && (!scenePersistant || SHUU_AudioObjectPoolParent.Instance == null))
            {
                Transform poolParent = Instantiate(parentPrefab).Init(ID, scenePersistant);
                ObjectPool = new SHUU_ObjectPool<SHUU_AudioInstance>(prefab, initialSize, poolParent, poolName : $"{ID}_AudioPool");

                CurrentScene = name;
            }
        }
        #endregion
    }
}
