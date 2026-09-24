using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Alchemy.Inspector;

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;
using SHUU.Utils.SceneManagement;

using static SHUU.Utils.Data.DataManager;
using SHUU.InnerWorkings.Preferences;

namespace SHUU.Utils.Helpers
{
    #region Wrappers
    public class Box<T> { public T value; }



    public class ActionVar<T>
    {
        #region Variables
        public T Value;

        private readonly Dictionary<string, Action<ActionVar<T>>> actions = new();
        #endregion



        #region Main
        public ActionVar(T v) => Value = v;

        public static implicit operator T(ActionVar<T> v) => v.Value;
        public static implicit operator ActionVar<T>(T value) => new ActionVar<T>(value);
        #endregion


        #region Logic
        public void AddAction(string name, Action<ActionVar<T>> action) => actions[name] = action;

        public void Invoke(string name)
        {
            if (actions.TryGetValue(name, out var action)) action(this);
        }
        #endregion
    }
    #endregion



    
    #region TagMask
    [Serializable]
    public struct TagMask
    {
        #region Variables
        [SerializeField] public int mask;



        private static List<string> TagRegistry => SHUU_TagRegistry.TagRegistry;
        private static Dictionary<string, int> TagCache = null;
        #endregion




        #region Main
        public static TagMask Everything => new() { mask = ~0 };
        public static TagMask Nothing => new() { mask = 0 };
        #endregion



        #region Logic
        private void EnsureCache()
        {
            if (TagCache != null) return;


            TagCache = new();

            for (int i = 0; i < TagRegistry.Count; i++)
                TagCache[TagRegistry[i]] = i;
        }

        public bool Contains(string tag)
        {
            EnsureCache();

            if (!TagCache.TryGetValue(tag, out int i)) return false;
            return (mask & (1 << i)) != 0;
        }
        #endregion
    }
    #endregion




    #region Dual-Key Dictionary
    [Serializable]
    public sealed class DualDictionary<TKey1, TKey2, TValue> : IEnumerable<(TKey1 Key1, TKey2 Key2, TValue Value)>
    {
        #region Variables
        private readonly Dictionary<TKey1, TKey2> key1ToKey2;
        private readonly Dictionary<TKey2, TKey1> key2ToKey1;
        private readonly Dictionary<TKey1, TValue> valuesByKey1;


        public int Count => key1ToKey2.Count;

        public IReadOnlyCollection<TKey1> Key1s => key1ToKey2.Keys;
        public IReadOnlyCollection<TKey2> Key2s => key2ToKey1.Keys;
        public IReadOnlyCollection<TValue> Values => valuesByKey1.Values;
        #endregion

        

        #region Main
        public DualDictionary(IEqualityComparer<TKey1> key1Comparer = null, IEqualityComparer<TKey2> key2Comparer = null)
        {
            key1ToKey2 = new Dictionary<TKey1, TKey2>(key1Comparer ?? EqualityComparer<TKey1>.Default);
            key2ToKey1 = new Dictionary<TKey2, TKey1>(key2Comparer ?? EqualityComparer<TKey2>.Default);
            valuesByKey1 = new Dictionary<TKey1, TValue>(key1Comparer ?? EqualityComparer<TKey1>.Default);
        }

        public TValue this[TKey1 key1]
        {
            get => valuesByKey1[key1];
            set => valuesByKey1[key1] = value;
        }
        public TValue this[TKey2 key2]
        {
            get => valuesByKey1[key2ToKey1[key2]];
            set => valuesByKey1[key2ToKey1[key2]] = value;
        }
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get => this[key1];
            set => this[key1] = value;
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            if (key1ToKey2.ContainsKey(key1))
                throw new ArgumentException($"Key1 '{key1}' already exists (paired with Key2 '{key1ToKey2[key1]}').", nameof(key1));

            if (key2ToKey1.ContainsKey(key2))
                throw new ArgumentException($"Key2 '{key2}' already exists (paired with Key1 '{key2ToKey1[key2]}').", nameof(key2));

            key1ToKey2[key1] = key2;
            key2ToKey1[key2] = key1;
            valuesByKey1[key1] = value;
        }
        public bool TryAdd(TKey1 key1, TKey2 key2, TValue value)
        {
            if (key1ToKey2.ContainsKey(key1) || key2ToKey1.ContainsKey(key2))
                return false;

            key1ToKey2[key1] = key2;
            key2ToKey1[key2] = key1;
            valuesByKey1[key1] = value;
            return true;
        }

        public void SetValue(TKey1 key1, TValue value)
        {
            if (!key1ToKey2.ContainsKey(key1))
                throw new KeyNotFoundException($"Key1 '{key1}' does not exist.");

            valuesByKey1[key1] = value;
        }

        public bool Remove(TKey1 key1)
        {
            if (!key1ToKey2.TryGetValue(key1, out var key2)) return false;

            key1ToKey2.Remove(key1);
            key2ToKey1.Remove(key2);
            valuesByKey1.Remove(key1);

            return true;
        }
        public bool Remove(TKey2 key2)
        {
            if (!key2ToKey1.TryGetValue(key2, out var key1)) return false;

            key1ToKey2.Remove(key1);
            key2ToKey1.Remove(key2);
            valuesByKey1.Remove(key1);

            return true;
        }
        public bool Remove(TKey1 key1, TKey2 key2) => Remove(key1);

        public void Clear()
        {
            key1ToKey2.Clear();
            key2ToKey1.Clear();
            valuesByKey1.Clear();
        }
        #endregion


        #region Lookups
        public bool ContainsKey1(TKey1 key1) => key1ToKey2.ContainsKey(key1);
        public bool ContainsKey2(TKey2 key2) => key2ToKey1.ContainsKey(key2);

        public bool TryGetValue(TKey1 key1, out TValue value) => valuesByKey1.TryGetValue(key1, out value);
        public bool TryGetValue(TKey2 key2, out TValue value)
        {
            if (key2ToKey1.TryGetValue(key2, out var key1)) return valuesByKey1.TryGetValue(key1, out value);
            
            value = default;
            return false;
        }
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value) => TryGetValue(key1, out value);

        public TKey2 GetKey2(TKey1 key1) => key1ToKey2[key1];
        public TKey1 GetKey1(TKey2 key2) => key2ToKey1[key2];

        public bool TryGetKey2(TKey1 key1, out TKey2 key2) => key1ToKey2.TryGetValue(key1, out key2);
        public bool TryGetKey1(TKey2 key2, out TKey1 key1) => key2ToKey1.TryGetValue(key2, out key1);
        #endregion

        
        #region Rebinding
        public void RebindKey2(TKey1 key1, TKey2 newKey2)
        {
            if (!key1ToKey2.TryGetValue(key1, out var oldKey2))
                throw new KeyNotFoundException($"Key1 '{key1}' does not exist.");

            if (key2ToKey1.TryGetValue(newKey2, out var owner) && !EqualityComparer<TKey1>.Default.Equals(owner, key1))
                throw new ArgumentException($"Key2 '{newKey2}' already belongs to Key1 '{owner}'.", nameof(newKey2));

            key2ToKey1.Remove(oldKey2);
            key1ToKey2[key1] = newKey2;
            key2ToKey1[newKey2] = key1;
        }

        public void RebindKey1(TKey2 key2, TKey1 newKey1)
        {
            if (!key2ToKey1.TryGetValue(key2, out var oldKey1))
                throw new KeyNotFoundException($"Key2 '{key2}' does not exist.");

            if (key1ToKey2.TryGetValue(newKey1, out var owner) && !EqualityComparer<TKey2>.Default.Equals(owner, key2))
                throw new ArgumentException($"Key1 '{newKey1}' already belongs to Key2 '{owner}'.", nameof(newKey1));

            var value = valuesByKey1[oldKey1];
            key1ToKey2.Remove(oldKey1);
            valuesByKey1.Remove(oldKey1);

            key1ToKey2[newKey1] = key2;
            key2ToKey1[key2] = newKey1;
            valuesByKey1[newKey1] = value;
        }
        #endregion


        #region Enumeration
        public IEnumerator<(TKey1 Key1, TKey2 Key2, TValue Value)> GetEnumerator()
        {
            foreach (var kvp in key1ToKey2)
                yield return (kvp.Key, kvp.Value, valuesByKey1[kvp.Key]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
    #endregion




    #region Circular Queue
    [Serializable]
    public sealed class CircularQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly Queue<T> data = new Queue<T>();



        public int Count => data.Count;


        private bool hasLast = false;
        private T last;




        public void Enqueue(T item) => data.Enqueue(item);

        public T Dequeue()
        {
            if (data.Count == 0) throw new InvalidOperationException("CircularQueue is empty.");


            last = data.Dequeue();
            hasLast = true;

            data.Enqueue(last);
            
            return last;
        }


        public T Peek() => data.Peek();
        public T PeekLast() => hasLast ? last : throw new InvalidOperationException("CircularQueue has no last item.");



        public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



        #region Queue implemented methods
        public void Clear() => data.Clear();

        public bool Contains(T item) => data.Contains(item);


        public T[] ToArray() => data.ToArray();

        public void CopyTo(T[] array, int arrayIndex) => data.CopyTo(array, arrayIndex);


        public bool TryDequeue(out T result)
        {
            if (data.Count == 0)
            {
                result = default!;
                return false;
            }

            result = Dequeue();
            return true;
        }

        public bool TryPeek(out T result)
        {
            if (data.Count == 0)
            {
                result = default!;
                return false;
            }

            result = data.Peek();
            return true;
        }
        #endregion
    }
    #endregion




    #region Inverted List
    [Serializable]
    public sealed class InvertedList<T> : IReadOnlyList<T>
    {
        private readonly List<T> data = new List<T>();



        public int Count => data.Count;




        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= data.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return data[data.Count - 1 - index];
            }
            set
            {
                if (index < 0 || index >= data.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                data[data.Count - 1 - index] = value;
            }
        }



        public void Add(T item) => data.Add(item);


        public bool Remove(T item) => data.Remove(item);
        
        public int RemoveAll(Predicate<T> match) => data.RemoveAll(match);


        public void Clear() => data.Clear();

        

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < data.Count; i++)
            {
                yield return data[data.Count - 1 - i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    #endregion




    #region Bucket Dictionary
    [Serializable]
    public sealed class BucketDictionary<TKey, TValue> : IEnumerable<(TKey Key, List<TValue> Values)>
    {
        #region Variables
        private readonly Dictionary<TKey, List<TValue>> data;

        public int KeyCount => data.Count;
        public int TotalValueCount
        {
            get
            {
                int count = 0;
                foreach (var list in data.Values) count += list.Count;
                return count;
            }
        }

        public IReadOnlyCollection<TKey> Keys => data.Keys;
        #endregion




        #region Main
        public BucketDictionary(IEqualityComparer<TKey> keyComparer = null)
        {
            data = new Dictionary<TKey, List<TValue>>(keyComparer ?? EqualityComparer<TKey>.Default);
        }


        public TValue this[TKey key]
        {
            get
            {
                var list = GetOrCreateBucket(key);

                if (list.Count != 0) return list[0];
                else return default;
            }
            set => Add(key, value);
        }

        public void Add(TKey key, TValue value) => GetOrCreateBucket(key).Add(value);
        public void AddRange(TKey key, IEnumerable<TValue> values) => GetOrCreateBucket(key).AddRange(values);
        public bool AddUnique(TKey key, TValue value)
        {
            var bucket = GetOrCreateBucket(key);
            if (bucket.Contains(value)) return false;

            bucket.Add(value);
            return true;
        }


        public bool Remove(TKey key, TValue value)
        {
            if (!data.TryGetValue(key, out var bucket)) return false;

            bool removed = bucket.Remove(value);
            if (removed && bucket.Count == 0)
                data.Remove(key);

            return removed;
        }
        public bool RemoveKey(TKey key) => data.Remove(key);
        public int RemoveAll(TKey key, Predicate<TValue> match)
        {
            if (!data.TryGetValue(key, out var bucket)) return 0;

            int removed = bucket.RemoveAll(match);
            if (bucket.Count == 0)
                data.Remove(key);

            return removed;
        }


        public void Clear() => data.Clear();
        #endregion



        #region Logic

        #region Lookups
        public bool ContainsKey(TKey key) => data.ContainsKey(key);
        public bool Contains(TKey key, TValue value) => data.TryGetValue(key, out var bucket) && bucket.Contains(value);

        public bool TryGetValues(TKey key, out List<TValue> values) => data.TryGetValue(key, out values);
        public List<TValue> GetValues(TKey key) => TryGetValues(key, out var bucket) ? bucket : new List<TValue>();
        public List<List<TValue>> GetAllValues()
        {
            var result = new List<List<TValue>>(data.Count);
            foreach (var bucket in data.Values)
                result.Add(bucket);

            return result;
        }

        public int CountFor(TKey key) => data.TryGetValue(key, out var bucket) ? bucket.Count : 0;
        #endregion


        #region Internal
        private List<TValue> GetOrCreateBucket(TKey key)
        {
            if (!data.TryGetValue(key, out var bucket))
            {
                bucket = new List<TValue>();
                data[key] = bucket;
            }
            return bucket;
        }
        #endregion


        #region Enumeration
        public IEnumerator<(TKey Key, List<TValue> Values)> GetEnumerator()
        {
            foreach (var kvp in data)
                yield return (kvp.Key, kvp.Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        #endregion
    }
    #endregion




    #region Static Instance Scripts

    #region MonoBehaviour

    #region General
    [DefaultExecutionOrder(-20000)]
    public abstract class HiddenSingleton_MonoBehaviour<T> : MonoBehaviour where T : HiddenSingleton_MonoBehaviour<T>
    {
        #region Variables
        private static T instance;
        
        public static T Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                return instance;
            }

            protected set => instance = value;
        }


        protected abstract bool PersistantSingleton();

        protected virtual UnityEvent _onCreation => null;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                if (SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission)
                    Debug.LogWarning($"[{typeof(T)} Singleton] Multiple instances detected. Destroying newest Instance...");
                Dispose();

                return;
            }


            instance = this as T;

            if (PersistantSingleton())
            {
                transform.parent = null;

                DontDestroyOnLoad(gameObject);
            }

            OnCreation();
            _onCreation?.Invoke();
        }

        protected virtual void OnCreation() { }


        protected virtual void OnDestroy()
        {
            if (instance == this) instance = null;
        }


        protected void Dispose() => Destroy(this);
        #endregion
    }

    [DefaultExecutionOrder(-20000)]
    public abstract class Singleton_MonoBehaviour<T> : MonoBehaviour where T : Singleton_MonoBehaviour<T>
    {
        #region Variables

        #region Singleton
        private static T instance;
        
        public static T Instance
        {
            get
            {
                if (instance == null) instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                return instance;
            }

            protected set => instance = value;
        }


        protected abstract bool PersistantSingleton();

        protected virtual UnityEvent _onCreation => null;
        #endregion



        #region Inspector
        [SerializeField, BoxGroup("Singleton Settings")]
        protected bool handleGameobject = true;
        #endregion

        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                if (SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission)
                    Debug.LogWarning($"[{typeof(T)} Singleton] Multiple instances detected. Destroying newest Instance...");
                Dispose();

                return;
            }


            instance = this as T;

            if (PersistantSingleton())
            {
                transform.parent = null;

                DontDestroyOnLoad(gameObject);
            }

            OnCreation();
            _onCreation?.Invoke();
        }

        protected virtual void OnCreation() { }


        protected virtual void OnDestroy()
        {
            if (instance == this) instance = null;
        }


        protected void Dispose() => Destroy(handleGameobject ? gameObject : this);
        #endregion
    }
    #endregion



    #region Complex Singletons
    [DefaultExecutionOrder(-20000)]
    public abstract class ComplexSingleton<T> : Singleton_MonoBehaviour<T> where T : ComplexSingleton<T>
    {
        #region Variables
        protected override bool PersistantSingleton() => true;


        
        [Tooltip("If set  to 0 or more, after that ammount of scene changes, on the next scene change the object will be destroyed.")]
        [SerializeField, BoxGroup("Singleton Settings"), Min(-1)]
        protected int bridges = -1;

        [Tooltip("These scenes won't cost a bridge to enter.")]
        [SerializeField, BoxGroup("Singleton Settings")]
        private List<string> bridgeFree_Scenes = new List<string>() {"LoadingScene"};
        
        private bool initialized = false;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField, BoxGroup("Singleton Settings")]
        private List<string> banned_Scenes = new List<string>();
        #endregion




        #region Main
        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }


        public void DestroySingleton()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnSceneChanged;

            Instance = null;

            
            Dispose();
        }
        #endregion



        #region Logic
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (banned_Scenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                if (SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission)
                    Debug.LogWarning($"[{typeof(T)} Singleton] Banned scene entered. Destroying singleton...");

                DestroySingleton();

                return;
            }
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            if (!initialized)
            {
                initialized = true;

                return;
            }

            
            if (bridges > -1)
            {
                if (bridges == 0)
                {
                    if (SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission)
                        Debug.LogWarning($"[{typeof(T)} Singleton] All bridges burnt. Destroying singleton...");

                    DestroySingleton();
                }
                
                if (!bridgeFree_Scenes.Contains(newScene.name)) bridges--;
            }
        }
        #endregion
    }
    #endregion

    #endregion



    #region Scriptable object
    public abstract class Singleton_ScriptableObject<T> : ScriptableObject where T : Singleton_ScriptableObject<T>
    {
        #region Variables
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null) instance = Resources.Load<T>(GetResourcesPath());

                return instance;
            }

            protected set => instance = value;
        }


        protected virtual string resourcesPath => name;

        private static string CachedPath;
        private static string GetResourcesPath()
        {
            if (CachedPath != null) return CachedPath;

            var temp = CreateInstance<T>();
            CachedPath = temp.resourcesPath;
            DestroyImmediate(temp);

            return CachedPath;
        }
        #endregion




        #region Main
        protected virtual void OnEnable()
        {
            if (instance != null && instance != this)
            {
                if (SHUUPreferences_HandyClasses.Instance != null && SHUUPreferences_HandyClasses.Instance.singleton_debugLogEmission)
                    Debug.LogWarning($"[{typeof(T)} Singleton] Multiple instances detected. Destroying newest Instance...");

                DestroyImmediate(this);
                return;
            }

            instance = this as T;
        }


        // Only the registered instance clears itself: a duplicate destroyed by OnEnable() above never got this far, so it can't null out the real one.
        protected virtual void OnDestroy()
        {
            if (instance == this) instance = null;
        }
        #endregion
    }
    #endregion

    #endregion



    #region AutoSave

    #region MonoBehaviour

    #region String
    public abstract class AutoSave_String_MonoBehaviour : MonoBehaviour
    {
        #region Main
        protected virtual void Awake() => LoadFile();


        private bool finalSave = false;

        protected virtual void OnDestroy()
        {
            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }

        protected virtual void OnApplicationQuit()
        {
            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }
        #endregion



        #region Override points
        protected virtual string FileAddress() => Path.Combine(Application.persistentDataPath, $"Data/{name}.json");


        protected abstract IEnumerable<string> SaveData();
        protected abstract void LoadData(IEnumerable<string> data);

        public virtual void SaveFile() => WriteText_ToFile(FileAddress(), SaveData());
        public virtual void LoadFile() => LoadData(ReadTextArray_FromFile(FileAddress()));
        #endregion
    }


    public abstract class AutoSave_String_Build_MonoBehaviour : AutoSave_String_MonoBehaviour
    {
        #region Main
        protected override void Awake()
        {
            #if !UNITY_EDITOR
            base.Awake();
            #endif
        }


        protected override void OnDestroy()
        {
            #if !UNITY_EDITOR
            base.OnDestroy();
            #endif
        }

        protected override void OnApplicationQuit()
        {
            #if !UNITY_EDITOR
            base.OnApplicationQuit();
            #endif
        }
        #endregion
    }
    #endregion



    #region Json
    public abstract class AutoSave_Json_MonoBehaviour<T> : MonoBehaviour
    {
        #region Main
        protected virtual void Awake() => LoadFile();


        protected virtual void OnDestroy() => Dispose();
        protected virtual void OnApplicationQuit() => Dispose();

        private bool disposed = false;
        protected virtual void Dispose()
        {
            if (disposed) return;
            disposed = true;
            
            SaveFile();
        }
        #endregion



        #region Override points
        protected virtual string FileAddress() => Path.Combine(Application.persistentDataPath, $"Data/{name}.json");


        protected abstract T SaveData();
        protected abstract void LoadData(T data);

        public virtual void SaveFile() => SaveJsonFile(SaveData(), FileAddress(), false);
        public virtual void LoadFile() => LoadData(LoadJsonFile<T>(FileAddress(), false));
        #endregion
    }


    public abstract class AutoSave_Json_Build_MonoBehaviour<T> : AutoSave_Json_MonoBehaviour<T>
    {
        #region Main
        protected override void Awake()
        {
            #if !UNITY_EDITOR
            base.Awake();
            #endif
        }


        protected override void OnDestroy()
        {
            #if !UNITY_EDITOR
            base.OnDestroy();
            #endif
        }
        #endregion
    }
    #endregion

    #endregion

    

    #region Scriptable Objects
    public abstract class AutoSave_ScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        #region Variables
        [JsonIgnore] protected virtual string filePath
        {
            get => Path.Combine(Application.persistentDataPath, $"Data/{id}.json");
        }


        [JsonIgnore] protected abstract T obj { get; }

        [JsonIgnore] protected virtual string id => obj.name;
        #endregion




        #region Main
        protected virtual void OnEnable() => LoadFile();

        protected virtual void OnDisable() => SaveFile();
        #endregion



        #region Override points
        public virtual void LoadFile() => WriteText_ToFile(filePath, SaveData());

        public void SaveFile()
        {
            if (TryReadText_FromFile(filePath, out string json)) LoadData(json);
        }


        protected virtual string SaveData() => JsonConvert.SerializeObject(
                                                obj,
                                                Formatting.Indented,
                                                new JsonSerializerSettings {
                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                                }
                                            );

        protected virtual void LoadData(string json) => JsonConvert.PopulateObject(
                                                            json,
                                                            obj,
                                                            new JsonSerializerSettings {
                                                                TypeNameHandling = TypeNameHandling.Auto,
                                                                ObjectCreationHandling = ObjectCreationHandling.Replace
                                                            }
                                                        );
        #endregion
    }


    public abstract class AutoSave_Build_ScriptableObject<T> : AutoSave_ScriptableObject<T> where T : ScriptableObject
    {
        #region Main
        protected override void OnEnable()
        {
            #if !UNITY_EDITOR
            base.OnEnable();
            #endif
        }

        protected override void OnDisable()
        {
            #if !UNITY_EDITOR
            base.OnDisable();
            #endif
        }
        #endregion
    }

    public abstract class AutoSave_PlayMode_ScriptableObject<T> : AutoSave_Build_ScriptableObject<T> where T : ScriptableObject
    {
        #region Static
        #if UNITY_EDITOR
        static AutoSave_PlayMode_ScriptableObject() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is AutoSave_PlayMode_ScriptableObject<T> auto) auto.LoadFile();
                }
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is AutoSave_PlayMode_ScriptableObject<T> auto) auto.SaveFile();
                }
            }
        }
        #endif
        #endregion



        #region Main
        protected override void OnDisable()
        {
            base.OnDisable();

            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            #endif
        }
        #endregion
    }
    #endregion

    #endregion



    #region Static Instance + AutoSave

    #region MonoBehaviour

    #region String
    public abstract class Singleton_AutoSave_String_MonoBehaviour<T> : Singleton_MonoBehaviour<T> where T : Singleton_MonoBehaviour<T>
    {
        #region Main
        protected override void Awake()
        {
            base.Awake();
            
            LoadFile();
        }


        private bool finalSave = false;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }

        protected virtual void OnApplicationQuit()
        {
            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }
        #endregion



        #region Override points
        protected virtual string FileAddress() => Path.Combine(Application.persistentDataPath, $"Data/{name}.json");


        protected abstract IEnumerable<string> SaveData();
        protected abstract void LoadData(IEnumerable<string> data);

        protected virtual void SaveFile() => WriteText_ToFile(FileAddress(), SaveData());
        protected virtual void LoadFile() => LoadData(ReadTextArray_FromFile(FileAddress()));
        #endregion
    }


    public abstract class Singleton_AutoSave_String_Build_MonoBehaviour<T> : Singleton_AutoSave_String_MonoBehaviour<T> where T : Singleton_MonoBehaviour<T>
    {
        #region Override points
        protected override void SaveFile()
        {
            #if !UNITY_EDITOR
            base.SaveFile();
            #endif
        }


        protected override void LoadFile()
        {
            #if !UNITY_EDITOR
            base.LoadFile();
            #endif
        }
        #endregion
    }
    #endregion



    #region Json
    public abstract class Singleton_AutoSave_Json_MonoBehaviour<T> : Singleton_MonoBehaviour<T> where T : Singleton_MonoBehaviour<T>
    {
        #region Main
        protected override void Awake()
        {
            base.Awake();
            
            LoadFile();
        }


        private bool finalSave = false;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }

        protected virtual void OnApplicationQuit()
        {
            if (finalSave) return;

            finalSave = true;
            SaveFile();
        }
        #endregion



        #region Override points
        protected virtual string FileAddress() => Path.Combine(Application.persistentDataPath, $"Data/{name}.json");


        protected abstract T SaveData();
        protected abstract void LoadData(T data);

        protected virtual void SaveFile() => SaveJsonFile(SaveData(), FileAddress(), false);
        protected virtual void LoadFile() => LoadData(LoadJsonFile<T>(FileAddress(), false));
        #endregion
    }
    public abstract class Singleton_AutoSave_Json_MonoBehaviour<T, E> : Singleton_MonoBehaviour<T> where T : Singleton_MonoBehaviour<T>
    {
        #region Main
        protected override void Awake()
        {
            base.Awake();
            
            LoadFile();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();

            SaveFile();
        }
        #endregion



        #region Override points
        protected virtual string FileAddress() => Path.Combine(Application.persistentDataPath, $"Data/{name}.json");


        protected abstract E SaveData();
        protected abstract void LoadData(E data);

        protected virtual void SaveFile() => SaveJsonFile(SaveData(), FileAddress(), false);
        protected virtual void LoadFile() => LoadData(LoadJsonFile<E>(FileAddress(), false));
        #endregion
    }


    public abstract class Singleton_AutoSave_Json_Build_MonoBehaviour<T> : Singleton_AutoSave_Json_MonoBehaviour<T> where T : Singleton_MonoBehaviour<T>
    {
        #region Override points
        protected override void SaveFile()
        {
            #if !UNITY_EDITOR
            base.SaveFile();
            #endif
        }


        protected override void LoadFile()
        {
            #if !UNITY_EDITOR
            base.LoadFile();
            #endif
        }
        #endregion
    }
    public abstract class Singleton_AutoSave_Json_Build_MonoBehaviour<T, E> : Singleton_AutoSave_Json_MonoBehaviour<T, E> where T : Singleton_MonoBehaviour<T>
    {
        #region Override points
        protected override void SaveFile()
        {
            #if !UNITY_EDITOR
            base.SaveFile();
            #endif
        }


        protected override void LoadFile()
        {
            #if !UNITY_EDITOR
            base.LoadFile();
            #endif
        }
        #endregion
    }
    #endregion

    #endregion



    #region Scriptable object
    public abstract class Singleton_AutoSave_ScriptableObject<T> : Singleton_ScriptableObject<T> where T : Singleton_ScriptableObject<T>
    {
        #region Variables
        [JsonIgnore] protected virtual string filePath
        {
            get => Path.Combine(Application.persistentDataPath, $"Data/{id}.json");
        }


        [JsonIgnore] protected abstract T obj { get; }

        [JsonIgnore] protected abstract string id { get; }
        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

            Load();
        }

        protected virtual void OnDisable() => Save();
        #endregion



        #region Override points
        protected void Save() => WriteText_ToFile(filePath, ToJson());

        protected void Load()
        {
            if (TryReadText_FromFile(filePath, out string json)) FromJson(json);
        }


        protected virtual string ToJson() => JsonConvert.SerializeObject(
                                                obj,
                                                Formatting.Indented,
                                                new JsonSerializerSettings {
                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                                }
                                            );

        protected virtual void FromJson(string json) => JsonConvert.PopulateObject(
                                                            json,
                                                            obj,
                                                            new JsonSerializerSettings {
                                                                TypeNameHandling = TypeNameHandling.Auto,
                                                                ObjectCreationHandling = ObjectCreationHandling.Replace
                                                            }
                                                        );
        #endregion
    }


    public abstract class Singleton_AutoSave_Build_ScriptableObject<T> : Singleton_ScriptableObject<T> where T : Singleton_ScriptableObject<T>
    {
        #region Variables
        [JsonIgnore] protected virtual string filePath
        {
            get => Path.Combine(Application.persistentDataPath, $"Data/{id}.json");
        }


        [JsonIgnore] protected abstract T obj { get; }

        [JsonIgnore] protected abstract string id { get; }
        #endregion




        #region Main
        protected override void OnEnable()
        {
            base.OnEnable();

            #if !UNITY_EDITOR
            Load();
            #endif
        }

        protected virtual void OnDisable()
        {
            #if !UNITY_EDITOR
            Save();
            #endif
        }
        #endregion



        #region Override points
        protected void Save() => WriteText_ToFile(filePath, ToJson());

        protected void Load()
        {
            if (TryReadText_FromFile(filePath, out string json)) FromJson(json);
        }


        protected virtual string ToJson() => JsonConvert.SerializeObject(
                                                obj,
                                                Formatting.Indented,
                                                new JsonSerializerSettings {
                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                                }
                                            );

        protected virtual void FromJson(string json) => JsonConvert.PopulateObject(
                                                            json,
                                                            obj,
                                                            new JsonSerializerSettings {
                                                                TypeNameHandling = TypeNameHandling.Auto,
                                                                ObjectCreationHandling = ObjectCreationHandling.Replace
                                                            }
                                                        );
        #endregion
    }
    
    public abstract class Singleton_AutoSave_PlayMode_ScriptableObject<T> : Singleton_AutoSave_Build_ScriptableObject<T> where T : Singleton_ScriptableObject<T>
    {
        #region Static
        #if UNITY_EDITOR
        static Singleton_AutoSave_PlayMode_ScriptableObject() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is Singleton_AutoSave_PlayMode_ScriptableObject<T> auto) auto.Load();
                }
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is Singleton_AutoSave_PlayMode_ScriptableObject<T> auto) auto.Save();
                }
            }
        }
        #endif
        #endregion



        #region Main
        protected override void OnDisable()
        {
            base.OnDisable();

            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            #endif
        }
        #endregion
    }
    #endregion
    
    #endregion
}
