using System;
using System.Collections.Generic;

namespace SHUU.Utils.Helpers
{

    public static class SHUU_Sorter
    {
        private static bool CanBeSorted<E>(IList<E> list)
        {
            return !(list.Count <= 1 || list == null);
        }




        #region O(n + k)

        public static void CountingSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;




        }


        //Worst Case: O(n^2)
        public static void BucketSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            
        }

        #endregion



        #region O(n * k)

        public static void RadixSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            
        }

        #endregion



        #region O(n * log[n])

        public static void MergeSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            if (list.Count <= 1)
                return;

            var aux = new E[list.Count];
            MergeSortInternal(ref list, aux, 0, list.Count - 1);
        }
            #region MergeSort Logic
            private static void MergeSortInternal<E>(ref IList<E> list, E[] aux, int left, int right) where E : IComparable<E>
            {
                if (left >= right)
                    return;


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


        public static void HeapSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;




        }
        

        // Worst case: O(n^2)
        public static void QuickSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            
        }


        //Average Case: O(n^2)
        //Worst Case: O(n^[3/2])
        public static void ShellSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            
        }

        #endregion



        #region O(n^2)

        //Best Case: O(n)
        public static void BubbleSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            int n = list.Count;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (list[j].CompareTo(list[j + 1]) > 0)
                    {
                        (list[j], list[j + 1]) = (list[j + 1], list[j]);
                    }
                }
            }
        }

        //Best Case: O(n)
        public static void BidirectionalBubbleSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            
        }


        //Best Case: O(n)
        public static void InsertionSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            int n = list.Count;

            for (int i = 1; i < n; i++) {
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


        public static void SelectionSort<E>(ref IList<E> list) where E : IComparable<E>
        {
            if (!CanBeSorted(list)) return;



            int n = list.Count;

            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;

                for (int j = i + 1; j < n; j++)
                {
                    if (list[j].CompareTo(list[minIndex]) < 0) minIndex = j;
                }

                (list[i], list[minIndex]) = (list[minIndex], list[i]);
            }
        }

        #endregion
    }
    
}
