using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.Helpers
{
    public static class ObjectPooling
    {
        public static Transform default_parent;


        public static List<IObjectPool> pools = new List<IObjectPool>();
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

    public class ObjectPool<T> : IObjectPool where T : Component
    {
        public System.Type GetItemType() => typeof(T);

        public int totalCount => total;
        public int poolCount => pool.Count;

        public string name => poolName;



        private readonly T prefab;

        private readonly Queue<ObjectPoolNode<T>> pool = new Queue<ObjectPoolNode<T>>();


        private readonly Transform parent;

        private readonly bool autoExpand;


        private int total = 0;

        private string poolName = null;




        public ObjectPool(T prefab, int initialSize, Transform parent = null, string _name = null, bool autoExpand = true)
        {
            ObjectPooling.pools.Add(this);



            if (parent == null && ObjectPooling.default_parent != null) parent = ObjectPooling.default_parent;
            if (string.IsNullOrEmpty(name)) _name = prefab.name;


            poolName = _name;


            this.prefab = prefab;
            this.parent = parent;
            this.autoExpand = autoExpand;

            total = 0;

            for (int i = 0; i < initialSize; i++) AddObject();
        }



        private ObjectPoolNode<T> AddObject(bool canBeDeleted = true)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);

            var node = new ObjectPoolNode<T>(obj);

            pool.Enqueue(node);

            total++;


            return node;
        }

        public void Expand(int add)
        {
            for (int i = 0; i < add; i++) AddObject();
        }


        public T Get()
        {
            if (pool.Count == 0)
            {
                if (autoExpand) return AddObject().instance;
                else return GetOldestRecyclable();
            }
            
            ObjectPoolNode<T> node = pool.Dequeue();

            node.instance.gameObject.SetActive(true);


            return node.instance;
        }


        private T GetOldestRecyclable()
        {
            int attempts = pool.Count;

            while (attempts-- > 0)
            {
                var node = pool.Dequeue();

                if (node.canRecycle)
                {
                    node.instance.gameObject.SetActive(true);

                    return node.instance;
                }

                
                pool.Enqueue(node);
            }

            
            return null;
        }

        public void Return(T instance)
        {
            foreach (var node in pool)
            {
                if (node.instance == instance)
                {
                    instance.gameObject.SetActive(false);

                    return;
                }
            }

            var newNode = new ObjectPoolNode<T>(instance)
            {
                canRecycle = true
            };


            instance.gameObject.SetActive(false);

            pool.Enqueue(newNode);
        }


        public void SetCanRecycle(T instance, bool value)
        {
            foreach (var node in pool)
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
