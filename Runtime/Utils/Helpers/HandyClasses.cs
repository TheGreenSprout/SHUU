using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public class Box<T>
    {
        T value;
    }




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




    #region Looping Queue
    public class LoopingQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly Queue<T> data = new Queue<T>();



        public int Count => data.Count;


        private bool hasLast = false;
        private T last;




        public void Enqueue(T item) => data.Enqueue(item);

        public T Dequeue()
        {
            if (data.Count == 0) throw new InvalidOperationException("LoopingQueue is empty.");


            last = data.Dequeue();
            hasLast = true;

            data.Enqueue(last);
            
            return last;
        }


        public T Peek() => data.Peek();
        public T PeekLast() => hasLast ? last : throw new InvalidOperationException("LoopingQueue has no last item.");



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
    public abstract class StaticInstance_Monobehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        
        public static T instance
        {
            get
            {
                if (_instance == null) _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);

                return _instance;
            }
        }




        protected virtual void Awake() => _instance = this as T;
    }


    public abstract class StaticInstance_ScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<T>(resourcesPath);

                return _instance;
            }
        }


        protected static string resourcesPath => typeof(T).Name;




        protected virtual void OnEnable() => _instance = this as T;
    }
    #endregion



    #region AutoSave
    public abstract class AutoSave_ScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        [JsonIgnore] protected virtual string filePath
        {
            get => Path.Combine(Application.persistentDataPath, $"Data/{id}.json");
        }


        [JsonIgnore] protected abstract T obj { get; }

        [JsonIgnore] protected abstract string id { get; }




        protected virtual void OnEnable() => Load();

        protected virtual void OnDisable() => Save();



        protected void Save() => HandyFunctions.WriteToFile(filePath, ToJson());

        protected virtual string ToJson() => JsonConvert.SerializeObject(
                                                obj,
                                                Formatting.Indented,
                                                new JsonSerializerSettings {
                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                                }
                                            );


        protected void Load()
        {
            if (HandyFunctions.TryReadFromFile(filePath, out string json)) FromJson(json);
        }

        protected virtual void FromJson(string json) => JsonConvert.PopulateObject(
                                                            json,
                                                            obj,
                                                            new JsonSerializerSettings {
                                                                TypeNameHandling = TypeNameHandling.Auto,
                                                                ObjectCreationHandling = ObjectCreationHandling.Replace
                                                            }
                                                        );
    }


    public abstract class AutoSave_PlayMode_ScriptableObject<T> : AutoSave_Build_ScriptableObject<T> where T : ScriptableObject
    {
        #if UNITY_EDITOR
        static AutoSave_PlayMode_ScriptableObject()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is AutoSave_PlayMode_ScriptableObject<T> auto) auto.Load();
                }
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                var assets = Resources.FindObjectsOfTypeAll<T>();

                foreach (var asset in assets)
                {
                    if (asset is AutoSave_PlayMode_ScriptableObject<T> auto) auto.Save();
                }
            }
        }
        #endif


        protected override void OnDisable()
        {
            base.OnDisable();

            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            #endif
        }
    }

    public abstract class AutoSave_Build_ScriptableObject<T> : AutoSave_ScriptableObject<T> where T : ScriptableObject
    {
        protected override void OnEnable()
        {
            #if !UNITY_EDITOR
            Load();
            #endif
        }

        protected override void OnDisable()
        {
            #if !UNITY_EDITOR
            Save();
            #endif
        }
    }
    #endregion



    #region Static Instance + AutoSave
    public abstract class StaticInstance_AutoSave_ScriptableObject<T> : StaticInstance_ScriptableObject<T> where T : ScriptableObject
    {
        [JsonIgnore] protected virtual string filePath
        {
            get => Path.Combine(Application.persistentDataPath, $"Data/{id}.json");
        }


        [JsonIgnore] protected abstract T obj { get; }

        [JsonIgnore] protected abstract string id { get; }




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



        protected void Save() => HandyFunctions.WriteToFile(filePath, ToJson());

        protected virtual string ToJson() => JsonConvert.SerializeObject(
                                                obj,
                                                Formatting.Indented,
                                                new JsonSerializerSettings {
                                                    TypeNameHandling = TypeNameHandling.Auto,
                                                    ObjectCreationHandling = ObjectCreationHandling.Replace
                                                }
                                            );


        protected void Load()
        {
            if (HandyFunctions.TryReadFromFile(filePath, out string json)) FromJson(json);
        }

        protected virtual void FromJson(string json) => JsonConvert.PopulateObject(
                                                            json,
                                                            obj,
                                                            new JsonSerializerSettings {
                                                                TypeNameHandling = TypeNameHandling.Auto,
                                                                ObjectCreationHandling = ObjectCreationHandling.Replace
                                                            }
                                                        );
    }
    #endregion
}
