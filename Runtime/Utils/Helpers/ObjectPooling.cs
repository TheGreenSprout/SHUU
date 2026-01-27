using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class ObjectPooling
    {
        public static Transform default_parent = null;


        public static List<IObjectPool> pools = new();
    }

    

    public class ObjectPoolNode<T> where T : Component
    {
        public T instance;
        public bool canRecycle = true;


        public ObjectPoolNode(T instance)
        {
            this.instance = instance;
        }
    }


    public interface IObjectPool
    {
        System.Type GetItemType();


        int totalCount { get; }
        int poolCount { get; }

        string name { get; }
    }

    public class SHUU_ObjectPool<T> : IObjectPool where T : Component
    {
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


        private string poolName = null;




        public SHUU_ObjectPool(T prefab, int initialSize, Transform parent = null, bool autoExpand = true, string _name = null)
        {
            ObjectPooling.pools.Add(this);



            if (parent == null && ObjectPooling.default_parent != null) parent = ObjectPooling.default_parent;
            if (string.IsNullOrEmpty(name)) _name = prefab.name;


            poolName = _name;


            this.prefab = prefab;
            this.parent = parent;
            this.autoExpand = autoExpand;

            for (int i = 0; i < initialSize; i++) AddObject();
        }



        private ObjectPoolNode<T> AddObject()
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);

            var node = new ObjectPoolNode<T>(obj);

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
            for (int i = 0; i < add; i++) AddObject();
        }


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

            node.instance.gameObject.SetActive(true);
            inUse.Add(node);

            return node.instance;
        }

        public T[] GetActives()
        {
            ObjectPoolNode<T>[] arrayBridge = inUse.ToArray();
            T[] array = new T[arrayBridge.Length];

            for (int i = 0; i < arrayBridge.Length; i++)
            {
                array[i] = arrayBridge[i].instance;
            }


            return array;
        }
        public T[] GetPooled()
        {
            ObjectPoolNode<T>[] arrayBridge = available.ToArray();
            T[] array = new T[arrayBridge.Length];

            for (int i = 0; i < arrayBridge.Length; i++)
            {
                array[i] = arrayBridge[i].instance;
            }
            

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

            available.Enqueue(found);
        }


        public void SetCanRecycle(T instance, bool value)
        {
            foreach (var node in inUse)
            {
                if (node.instance == instance)
                {
                    node.canRecycle = value;

                    return;
                }
            }
        }
    }
}
