using System;
using System.Collections.Generic;
using System.Linq;

namespace SHUU.Utils.Helpers
{
    public static class Sorter
    {
        private static bool CanBeSorted<E>(IList<E> list)
        {
            return !(list.Count <= 1 || list == null);
        }




        #region O(n + k)

        /// <summary>
        /// Counting Sort — O(n + k) — Works for integer lists.
        /// </summary>
        public static void CountingSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;
            if (typeof(E) != typeof(int))
                throw new InvalidOperationException("CountingSort only supports integer lists.");

            var intList = list.Cast<int>().ToArray();

            int max = intList.Max();
            int min = intList.Min();
            int range = max - min + 1;

            int[] count = new int[range];
            int[] output = new int[intList.Length];

            foreach (var num in intList)
                count[num - min]++;

            for (int i = 1; i < range; i++)
                count[i] += count[i - 1];

            for (int i = intList.Length - 1; i >= 0; i--)
            {
                output[count[intList[i] - min] - 1] = intList[i];
                count[intList[i] - min]--;
            }

            list = output.Cast<E>().ToList();
        }


        /// <summary>
        /// Bucket Sort — Worst case: O(n²) ; Best case: O(n + k) — Works for normalized double lists (0.0–1.0 range ideally).
        /// </summary>
        public static void BucketSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;
            if (typeof(E) != typeof(double))
                throw new InvalidOperationException("BucketSort only supports double lists.");

            var arr = list.Cast<double>().ToArray();
            int n = arr.Length;

            var buckets = new List<double>[n];
            for (int i = 0; i < n; i++)
                buckets[i] = new List<double>();

            foreach (var num in arr)
            {
                int index = (int)(num * n);
                if (index >= n) index = n - 1;
                buckets[index].Add(num);
            }

            for (int i = 0; i < n; i++)
                buckets[i].Sort();

            var result = new List<double>();
            foreach (var bucket in buckets)
                result.AddRange(bucket);

            list = result.Cast<E>().ToList();
        }

        #endregion



        #region O(n * k)

        /// <summary>
        /// Radix Sort — O(n * k) — Works for integer lists. 
        /// </summary>
        public static void RadixSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;
            if (typeof(E) != typeof(int))
                throw new InvalidOperationException("RadixSort only supports integer lists.");

            var arr = list.Cast<int>().ToArray();

            int max = arr.Max();
            for (int exp = 1; max / exp > 0; exp *= 10)
                CountingSortByDigit(arr, exp);

            list = arr.Cast<E>().ToList();
        }

        
        /// <summary>
        /// Counting Sort (by digit) — O(n * k)
        /// </summary>
        private static void CountingSortByDigit(int[] arr, int exp)
        {
            int n = arr.Length;
            int[] output = new int[n];
            int[] count = new int[10];

            for (int i = 0; i < n; i++)
                count[(arr[i] / exp) % 10]++;

            for (int i = 1; i < 10; i++)
                count[i] += count[i - 1];

            for (int i = n - 1; i >= 0; i--)
            {
                int digit = (arr[i] / exp) % 10;
                output[count[digit] - 1] = arr[i];
                count[digit]--;
            }

            for (int i = 0; i < n; i++)
                arr[i] = output[i];
        }

        #endregion



        #region O(n * log[n])

        /// <summary>
        /// Merge Sort — O(n * log n)
        /// </summary>
        public static void MergeSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;
            if (list.Count <= 1) return;

            var aux = new E[list.Count];
            MergeSortInternal(ref list, aux, 0, list.Count - 1);
        }
            #region Merge Sort internal logic
            private static void MergeSortInternal<E>(ref IList<E> list, E[] aux, int left, int right) where E : IComparable<E>
            {
                if (left >= right) return;

                int mid = (left + right) / 2;

                MergeSortInternal(ref list, aux, left, mid);
                MergeSortInternal(ref list, aux, mid + 1, right);

                Merge(ref list, aux, left, mid, right);
            }

            private static void Merge<E>(ref IList<E> list, E[] aux, int left, int mid, int right) where E : IComparable<E>
            {
                for (int k = left; k <= right; k++)
                    aux[k] = list[k];

                int i = left, j = mid + 1;

                for (int k = left; k <= right; k++)
                {
                    if (i > mid) list[k] = aux[j++];
                    else if (j > right) list[k] = aux[i++];
                    else if (aux[j].CompareTo(aux[i]) < 0) list[k] = aux[j++];
                    else list[k] = aux[i++];
                }
            }
            #endregion


        /// <summary>
        /// Heap Sort — O(n * log n)
        /// </summary>
        public static void HeapSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int n = list.Count;

            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(ref list, n, i);

            for (int i = n - 1; i > 0; i--)
            {
                (list[0], list[i]) = (list[i], list[0]);
                Heapify(ref list, i, 0);
            }
        }
            #region Heap Sort internal logic
            private static void Heapify<E>(ref IList<E> list, int n, int i) where E : IComparable<E>
            {
                int largest = i;
                int left = 2 * i + 1;
                int right = 2 * i + 2;

                if (left < n && list[left].CompareTo(list[largest]) > 0)
                    largest = left;

                if (right < n && list[right].CompareTo(list[largest]) > 0)
                    largest = right;

                if (largest != i)
                {
                    (list[i], list[largest]) = (list[largest], list[i]);
                    Heapify(ref list, n, largest);
                }
            }
            #endregion


        /// <summary>
        /// Quick Sort — Worst case: O(n²) ; Best case: O(n * log n)
        /// </summary>
        public static void QuickSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;
            QuickSortInternal(ref list, 0, list.Count - 1);
        }
            #region Merge Sort internal logic
            private static void QuickSortInternal<E>(ref IList<E> list, int low, int high) where E : IComparable<E>
            {
                if (low < high)
                {
                    int pivotIndex = Partition(ref list, low, high);
                    QuickSortInternal(ref list, low, pivotIndex - 1);
                    QuickSortInternal(ref list, pivotIndex + 1, high);
                }
            }
            private static int Partition<E>(ref IList<E> list, int low, int high) where E : IComparable<E>
            {
                E pivot = list[high];
                int i = low - 1;

                for (int j = low; j < high; j++)
                {
                    if (list[j].CompareTo(pivot) <= 0)
                    {
                        i++;
                        (list[i], list[j]) = (list[j], list[i]);
                    }
                }

                (list[i + 1], list[high]) = (list[high], list[i + 1]);
                return i + 1;
            }
            #endregion


        /// <summary>
        /// Shell Sort — Worst case: O(n^(3/2)) ; Best case: O(n log n)
        /// </summary>
        public static void ShellSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int n = list.Count;
            for (int gap = n / 2; gap > 0; gap /= 2)
            {
                for (int i = gap; i < n; i++)
                {
                    E temp = list[i];
                    int j;
                    for (j = i; j >= gap && list[j - gap].CompareTo(temp) > 0; j -= gap)
                        list[j] = list[j - gap];
                    list[j] = temp;
                }
            }
        }

        #endregion



        #region O(n²)

        /// <summary>
        /// Bubble Sort — Worst case: O(n²) ; Best case: O(n)
        /// </summary>
        public static void BubbleSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (list[j].CompareTo(list[j + 1]) > 0)
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                }
            }
        }


        /// <summary>
        /// Bidirectional (Cocktail) Bubble Sort — Worst case: O(n²) ; Best case: O(n)
        /// </summary>
        public static void BidirectionalBubbleSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int start = 0;
            int end = list.Count - 1;
            bool swapped = true;

            while (swapped)
            {
                swapped = false;

                for (int i = start; i < end; i++)
                {
                    if (list[i].CompareTo(list[i + 1]) > 0)
                    {
                        (list[i], list[i + 1]) = (list[i + 1], list[i]);
                        swapped = true;
                    }
                }

                if (!swapped) break;

                swapped = false;
                end--;

                for (int i = end - 1; i >= start; i--)
                {
                    if (list[i].CompareTo(list[i + 1]) > 0)
                    {
                        (list[i], list[i + 1]) = (list[i + 1], list[i]);
                        swapped = true;
                    }
                }

                start++;
            }
        }


        /// <summary>
        /// Insertion Sort — Worst case: O(n²) ; Best case: O(n)
        /// </summary>
        public static void InsertionSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int n = list.Count;

            for (int i = 1; i < n; i++)
            {
                E key = list[i];
                int j = i - 1;

                while (j >= 0 && list[j].CompareTo(key) > 0)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                list[j + 1] = key;
            }
        }


        /// <summary>
        /// Selection Sort — O(n²)
        /// </summary>
        public static void SelectionSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;

            int n = list.Count;

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                for (int j = i + 1; j < n; j++)
                {
                    if (list[j].CompareTo(list[minIndex]) < 0)
                        minIndex = j;
                }

                (list[i], list[minIndex]) = (list[minIndex], list[i]);
            }
        }

        #endregion
    }
    
}
