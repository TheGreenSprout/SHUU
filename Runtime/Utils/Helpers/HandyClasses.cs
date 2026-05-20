using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;
using SHUU.Utils.SceneManagement;

using static SHUU.Utils.Data.DataManager;

namespace SHUU.Utils.Helpers
{
    public class Box<T> { T value; }




    #region Dual-Key Dictionary
    public class DualDictionary<TKey1, TKey2, TValue> : IDictionary<(TKey1, TKey2), TValue>
    {
        private readonly Dictionary<(TKey1, TKey2), TValue> data = new();



        public ICollection<(TKey1, TKey2)> Keys => data.Keys;

        public ICollection<TValue> Values => data.Values;


        public int Count => data.Count;

        public bool IsReadOnly => false;




        private static (TKey1, TKey2) K1(TKey1 key) => (key, default);
        private static (TKey1, TKey2) K2(TKey2 key) => (default, key);



        #region IDictionary implementation

        public TValue this[(TKey1, TKey2) key]
        {
            get => data[key];
            set => data[key] = value;
        }


        public void Add((TKey1, TKey2) key, TValue value) => data.Add(key, value);

        public bool ContainsKey((TKey1, TKey2) key) => data.ContainsKey(key);

        public bool TryGetValue((TKey1, TKey2) key, out TValue value) => data.TryGetValue(key, out value);

        public bool Remove((TKey1, TKey2) key) => data.Remove(key);

        public void Clear() => data.Clear();

        public void Add(KeyValuePair<(TKey1, TKey2), TValue> item) => ((IDictionary<(TKey1, TKey2), TValue>)data).Add(item);

        public bool Contains(KeyValuePair<(TKey1, TKey2), TValue> item) => ((IDictionary<(TKey1, TKey2), TValue>)data).Contains(item);

        public void CopyTo(KeyValuePair<(TKey1, TKey2), TValue>[] array, int arrayIndex) => ((IDictionary<(TKey1, TKey2), TValue>)data).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<(TKey1, TKey2), TValue> item) => ((IDictionary<(TKey1, TKey2), TValue>)data).Remove(item);


        public IEnumerator<KeyValuePair<(TKey1, TKey2), TValue>> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion



        #region Dual-key implementations

        public TValue this[TKey1 key]
        {
            get => this[K1(key)];
            set => this[K1(key)] = value;
        }
        public TValue this[TKey2 key]
        {
            get => this[K2(key)];
            set => this[K2(key)] = value;
        }
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get => this[(key1, key2)];
            set => this[(key1, key2)] = value;
        }


        public TKey2 GetKey2(TKey1 key) => Keys.First(k => EqualityComparer<TKey1>.Default.Equals(k.Item1, key)).Item2;
        public TKey1 GetKey1(TKey2 key) => Keys.First(k => EqualityComparer<TKey2>.Default.Equals(k.Item2, key)).Item1;


        public void Add(TKey1 key, TValue value) => Add(K1(key), value);
        public void Add(TKey2 key, TValue value) => Add(K2(key), value);
        public void Add(TKey1 key1, TKey2 key2, TValue value) => Add((key1, key2), value);

        public bool ContainsKey(TKey1 key) => ContainsKey(K1(key));
        public bool ContainsKey(TKey2 key) => ContainsKey(K2(key));
        public bool ContainsKey(TKey1 key1, TKey2 key2) => ContainsKey((key1, key2));

        public bool TryGetValue(TKey1 key, out TValue value) => TryGetValue(K1(key), out value);
        public bool TryGetValue(TKey2 key, out TValue value) => TryGetValue(K2(key), out value);
        public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value) => TryGetValue((key1, key2), out value);

        public bool Remove(TKey1 key) => Remove(K1(key));
        public bool Remove(TKey2 key) => Remove(K2(key));
        public bool Remove(TKey1 key1, TKey2 key2) => Remove((key1, key2));
        
        #endregion
    }
    #endregion




    #region Circular Queue
    public class CircularQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>
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
    public class InvertedList<T> : IReadOnlyList<T>
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




    #region Static Instance Scripts

    #region MonoBehaviour

    #region General
    [DefaultExecutionOrder(-20000)]
    public abstract class Singleton_MonoBehaviour<T> : MonoBehaviour where T : Singleton_MonoBehaviour<T>
    {
        #region Variables
        protected static T _instance;
        
        public static T instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                return _instance;
            }
        }



        protected abstract bool PersistantSingleton();

        protected virtual UnityEvent _onCreation => null;


        [Header("Singleton Settings")]
        [SerializeField] protected bool handleGameobject = true;
        #endregion




        #region Main
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (SHUU_Preferences.instance.singleton_debugLogEmission) Debug.LogWarning($"[{typeof(T)} Singleton] Multiple instances detected. Destroying newest instance...");
                Dispose();

                return;
            }


            _instance = this as T;

            if (PersistantSingleton())
            {
                transform.parent = null;

                DontDestroyOnLoad(gameObject);
            }

            OnCreation();
            _onCreation?.Invoke();
        }

        protected virtual void OnCreation() { }


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
        [Min(-1)] public int bridges = -1;
        [Tooltip("These scenes won't cost a bridge to enter.")]
        [SerializeField] private List<string> bridgeFree_Scenes = new List<string>() {"LoadingScene"};
        private bool initialized = false;


        [Tooltip("If the singleton enters one of these scenes it will be deleted.")]
        [SerializeField] private List<string> banned_Scenes = new List<string>();
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

           _instance = null;

            
            Dispose();
        }
        #endregion



        #region Logic
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (banned_Scenes.Contains(SceneLoader.GetCurrentSceneName()))
            {
                if (SHUU_Preferences.instance.singleton_debugLogEmission) Debug.LogWarning($"[{typeof(T)} Singleton] Banned scene entered. Destroying singleton...");

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
                    if (SHUU_Preferences.instance.singleton_debugLogEmission) Debug.LogWarning($"[{typeof(T)} Singleton] All bridges burnt. Destroying singleton...");

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
        protected static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<T>(GetResourcesPath());

                return _instance;
            }
        }


        protected virtual string resourcesPath => name;

        private static string cachedPath;
        private static string GetResourcesPath()
        {
            if (cachedPath != null) return cachedPath;

            var temp = CreateInstance<T>();
            cachedPath = temp.resourcesPath;
            DestroyImmediate(temp);

            return cachedPath;
        }
        #endregion




        #region Main
        protected virtual void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
                if (SHUU_Preferences.instance.singleton_debugLogEmission) Debug.LogWarning($"[{typeof(T)} Singleton] Multiple instances detected. Destroying newest instance...");

                Destroy(this);
                return;
            }

            _instance = this as T;
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

        [JsonIgnore] protected abstract string id { get; }
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


        protected virtual void OnDestroy() => SaveFile();
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
