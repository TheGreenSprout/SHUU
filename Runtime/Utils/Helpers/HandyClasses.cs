using System;
using System.Collections;
using System.Collections.Generic;

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


        public void Add(TKey1 key, TValue value) => Add(K1(key), value);
        public void Add(TKey2 key, TValue value) => Add(K2(key), value);

        public bool ContainsKey(TKey1 key) => ContainsKey(K1(key));
        public bool ContainsKey(TKey2 key) => ContainsKey(K2(key));

        public bool TryGetValue(TKey1 key, out TValue value) => TryGetValue(K1(key), out value);
        public bool TryGetValue(TKey2 key, out TValue value) => TryGetValue(K2(key), out value);

        public bool Remove(TKey1 key) => Remove(K1(key));
        public bool Remove(TKey2 key) => Remove(K2(key));
        
        #endregion
    }
    #endregion



    #region Looping Queue
    public class LoopingQueue<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly Queue<T> data = new Queue<T>();



        public int Count => data.Count;


        private T last;




        public void Enqueue(T item) => data.Enqueue(item);

        public T Dequeue()
        {
            if (data.Count == 0) throw new InvalidOperationException("LoopingQueue is empty.");


            last = data.Dequeue();
            data.Enqueue(last);
            
            return last;
        }


        public T Peek() => data.Peek();
        public T PeekLast() => last;



        public IEnumerator<T> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
}
