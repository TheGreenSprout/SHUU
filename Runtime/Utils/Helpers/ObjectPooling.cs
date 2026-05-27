using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public class SHUU_ObjectPool<T> : IObjectPool where T : Component
    {
        #region Variables
        public System.Type GetItemType() => typeof(T);

        public int totalCount => poolCount + activesCount;
        public int poolCount => available.Count;
        public int activesCount => inUse.Count;

        public string name => poolName;



        private readonly T prefab;

        private readonly Queue<ObjectPoolNode<T>> available = new();

        private readonly HashSet<ObjectPoolNode<T>> inUse = new();


        private readonly Transform parent;

        private readonly bool autoExpand;

        private readonly bool autoRestore;


        private string poolName = null;
        #endregion




        #region Main
        public SHUU_ObjectPool(T prefab, int initialSize, Transform parent = null, bool autoExpand = true, bool autoRestore = true, string _name = null)
        {
            ObjectPooling.pools.Add(this);


            if (parent == null && ObjectPooling.default_parent != null) parent = ObjectPooling.default_parent;
            if (string.IsNullOrEmpty(_name)) _name = prefab.name;


            poolName = _name;

            this.prefab = prefab;
            this.parent = parent;
            this.autoExpand = autoExpand;
            this.autoRestore = autoRestore;


            for (int i = 0; i < initialSize; i++) AddObject();
        }


        public void Dispose()
        {
            foreach (var node in available)
                Object.Destroy(node.instance.gameObject);
            
            foreach (var node in inUse)
                Object.Destroy(node.instance.gameObject);
        }
        #endregion



        #region Logic

        #region Pool Handling
        private ObjectPoolNode<T> AddObject()
        {
            T obj = Object.Instantiate(prefab, parent);

            string json = null;
            if (obj is IObjectPoolable) ((IObjectPoolable)obj).SaveDefaults();
            else if (autoRestore)
            {
                try
                {
                    json = JsonConvert.SerializeObject(
                        obj,
                        Formatting.Indented,
                        new JsonSerializerSettings {
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace
                        }
                    );
                }
                catch { Debug.LogError($"[{poolName}] Failed to serialize object pool item '{obj.name}'."); }
            }

            obj.gameObject.SetActive(false);

            var node = new ObjectPoolNode<T>(obj, json);
            available.Enqueue(node);

            return node;
        }
        public ObjectPoolNode<T> ForceAdd_Active(T obj)
        {
            var node = new ObjectPoolNode<T>(obj);
            inUse.Add(node);

            return node;
        }

        public void Expand(int add)
        {
            for (int i = 0; i < add; i++)
                AddObject();
        }
        #endregion



        #region Fetching
        public T Get()
        {
            ObjectPoolNode<T> node;

            if (available.Count > 0) node = available.Dequeue();
            else if (autoExpand)
            {
                AddObject();
                
                return Get();
            }
            else return GetOldestRecyclable();

            node?.instance?.gameObject?.SetActive(true);
            inUse.Add(node);

            return node.instance;
        }

        public T[] GetActives()
        {
            ObjectPoolNode<T>[] arrayBridge = inUse.ToArray();
            T[] array = new T[arrayBridge.Length];

            for (int i = 0; i < arrayBridge.Length; i++)
                array[i] = arrayBridge[i].instance;

            return array;
        }
        public T[] GetPooled()
        {
            ObjectPoolNode<T>[] arrayBridge = available.ToArray();
            T[] array = new T[arrayBridge.Length];

            for (int i = 0; i < arrayBridge.Length; i++)
                array[i] = arrayBridge[i].instance;
            
            return array;
        }

        private T GetOldestRecyclable()
        {
            foreach (var node in inUse)
            {
                if (!node.canRecycle) continue;

                node.instance.gameObject.SetActive(true);

                return node.instance;
            }

            return null;
        }
        #endregion



        #region Return
        public void Return(T instance)
        {
            ObjectPoolNode<T> found = null;

            foreach (var node in inUse)
            {
                if (node.instance == instance)
                {
                    found = node;

                    break;
                }
            }


            if (found == null)
            {
                Debug.LogWarning($"[{poolName}] Tried to return object not owned by pool.");

                return;
            }

            inUse.Remove(found);
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent, false);


            if (instance is IObjectPoolable) ((IObjectPoolable)instance).RestoreDefaults();
            else if (autoRestore && !string.IsNullOrWhiteSpace(found.json))
            {
                try
                {
                    JsonConvert.PopulateObject(
                        found.json,
                        instance,
                        new JsonSerializerSettings {
                            TypeNameHandling = TypeNameHandling.Auto,
                            ObjectCreationHandling = ObjectCreationHandling.Replace
                        }
                    );
                }
                catch { Debug.LogError($"[{poolName}] Failed to deserialize object pool item '{instance.name}'."); }
            }

            available.Enqueue(found);
        }
        #endregion



        #region Item Handling
        public bool SetCanRecycle(T instance, bool value)
        {
            foreach (var node in inUse)
            {
                if (node.instance == instance)
                {
                    node.canRecycle = value;

                    return true;
                }
            }

            return false;
        }
        #endregion
    
        #endregion
    }




    #region Helper Classes
        #region Tracking
        public static class ObjectPooling
        {
            public static Transform default_parent = null;


            public static List<IObjectPool> pools = new();
        }


        public interface IObjectPool
        {
            System.Type GetItemType();


            int totalCount { get; }
            int poolCount { get; }

            string name { get; }
        }
        #endregion

        
        
        #region Inner workings
        public class ObjectPoolNode<T> where T : Component
        {
            public T instance;
            public bool canRecycle = true;

            public string json = null;


            public ObjectPoolNode(T instance, string json = null)
            {
                this.instance = instance;
                this.json = json;
            }
        }


        public interface IObjectPoolable
        {
            public void SaveDefaults();
            public void RestoreDefaults();
        }
        #endregion
    #endregion
}
