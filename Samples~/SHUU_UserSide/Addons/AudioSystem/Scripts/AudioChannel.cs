using System;
using System.Collections.Generic;
using UnityEngine;

using Alchemy.Inspector;

using SHUU.UserSide.Addons.AudioSystem.ScriptableObjects;
using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;

namespace SHUU.UserSide.Addons.AudioSystem
{
    [Serializable]
    public partial class AudioChannel : IAudioChannel
    {
        #region Variables
        // External
        [SerializeField] private string ID = "AudioChannel_Name";


        [NonSerialized] private IAudioChannel parent;

        [SerializeReference] private List<AudioChannel> children = new();



        [FoldoutGroup("Parameters")]
        [SerializeField] private bool inheritNullOptions = true;
        [FoldoutGroup("Parameters")]
        [SerializeField] private SHUU_AudioInstance.Options options = new();

        [FoldoutGroup("Parameters")]
        [SerializeField] private bool inheritAudioLookup = false;
        [FoldoutGroup("Parameters"), HideIf("inheritAudioLookup")]
        [SerializeField] private AudioLookup audioLookup = null;


        [FoldoutGroup("Parameters")]
        [SerializeField] private bool useParentPool = true;

        [FoldoutGroup("Parameters/ObjectPool Settings"), HideIf("useParentPool")]
        [SerializeField, Min(0)] private int initialSize = 10;

        [FoldoutGroup("Parameters/ObjectPool Settings"), HideIf("useParentPool")]
        [SerializeField] private bool useParentPoolParentPrefab = true;
        private bool hidePoolParentPrefabField => useParentPool || useParentPoolParentPrefab;
        [FoldoutGroup("Parameters/ObjectPool Settings"), HideIf("hidePoolParentPrefabField")]
        [SerializeField] private SHUU_AudioObjectPoolParent parentPrefab = null;

        [FoldoutGroup("Parameters/ObjectPool Settings"), HideIf("useParentPool")]
        [SerializeField] private bool useParentPoolPrefab = true;
        private bool hidePoolPrefabField => useParentPool || useParentPoolParentPrefab;
        [FoldoutGroup("Parameters/ObjectPool Settings"), HideIf("hidePoolPrefabField"), Required]
        [SerializeField] private SHUU_AudioInstance prefab = null;



        // Internal
        private SHUU_ObjectPool<SHUU_AudioInstance> objectPool = null;


        private string currentScene = null;

        private Dictionary<string, IAudioChannel> childrenDict = new();

        private string path = null;


        private static Dictionary<string, bool> InitDict = new();
        private bool init
        {
            get => InitDict.GetValueOrDefault(ID, false);
            set => InitDict[ID] = value;
        }
        #endregion




        #region Main
        void IAudioChannel.Init(IAudioChannel parent)
        {
            if (!init)
            {
                currentScene = null;
                childrenDict.Clear();
                path = null;


                this.parent = parent;
                if (inheritNullOptions) options?.InheritOptions(parent.GetOptions());
                if (inheritAudioLookup) audioLookup = parent.GetAudioLookup();
            }

            ReloadObjectPool();

            foreach (IAudioChannel child in children)
            {
                if (!init) childrenDict[child.GetID()] = child;
                child.Init(this);
            }
                
            if (!init) init = true;
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

            return objectPool;
        }
        SHUU_AudioInstance IAudioChannel.GetObjectPool_Prefab() => prefab;
        SHUU_AudioObjectPoolParent IAudioChannel.GetObjectPool_ParentPrefab() => parentPrefab;
        Transform IAudioChannel.GetObjectPool_Parent() => objectPool?.GetParent();
        IAudioChannel IAudioChannel.GetChild(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;


            int index = path.IndexOf("/");
            string childID = index == -1 ? path : path.Substring(0, index);
            string remainingPath = index == -1 ? "" : path.Substring(index + 1);

            return childrenDict[childID].GetChild(remainingPath);
        }
        IEnumerable<IAudioChannel> IAudioChannel.GetChildren() => children;
        string IAudioChannel.GetPath(bool includeRoot)
        {
            if (path == null) path = parent.GetPath(includeRoot) + "/" + ID;

            return path;
        }
        #endregion


        public SHUU_AudioInstance GetAudioInstance(SHUU_AudioInstance.Options options)
        {
            var instanceOptions = new SHUU_AudioInstance.Options(options);
            instanceOptions.InheritOptions(options);
            
            var ret = objectPool.Get();
            ret.Init(objectPool, instanceOptions);

            return ret;
        }

        #endregion



        #region Logic
        private void ReloadObjectPool()
        {
            if (useParentPool) 
            {
                string hello = parent.GetPath();
                objectPool = parent.GetObjectPool();

                return;
            }


            if (!init && useParentPoolParentPrefab) parentPrefab = parent.GetObjectPool_ParentPrefab();
            var name = SceneLoader.GetCurrentSceneName();

            if ((!string.IsNullOrEmpty(currentScene) && currentScene.Equals(name)) || AudioChannelGroup.PersistantPool) return;
        
            SHUU_AudioInstance poolPrefab = useParentPoolPrefab ? parent.GetObjectPool_Prefab() : prefab;
            SHUU_AudioObjectPoolParent poolParentPrefab = useParentPoolParentPrefab ? parent.GetObjectPool_ParentPrefab() : parentPrefab;

            Transform poolParent = GameObject.Instantiate(poolParentPrefab, parent.GetObjectPool_Parent()).Init(ID);
            objectPool = new SHUU_ObjectPool<SHUU_AudioInstance>(poolPrefab, initialSize, poolParent, poolName : $"{ID}_AudioPool");

            currentScene = name;
        }
        #endregion
    }
}
