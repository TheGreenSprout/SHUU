#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SHUU._Editor.Utils
{
    [InitializeOnLoad]
    public static class ComponentHeaderItems
    {
        #region Variables
        public delegate bool Item(Rect slot, Object[] targets);


        private static readonly List<Item> Pending = new List<Item>();

        private static readonly FieldInfo UnityItemList = typeof(EditorGUIUtility).GetField("s_EditorHeaderItemsMethods", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type UnityItemType = typeof(EditorGUIUtility).GetNestedType("HeaderItemDelegate", BindingFlags.NonPublic | BindingFlags.Public);

        private static bool Available => UnityItemList != null && UnityItemType != null;
        #endregion




        #region Main
        static ComponentHeaderItems()
        {
            if (!Available) Debug.LogWarning("[SHUU] This Unity version doesn't have the component header items list that the header icons need, so they won't show.");
        }


        public static void Register(Item item)
        {
            if (!Available) return;

            Pending.Add(item);

            EditorApplication.update -= TryAdd;
            EditorApplication.update += TryAdd;
        }
        #endregion



        #region Logic
        private static void TryAdd()
        {
            if (!(UnityItemList.GetValue(null) is IList list)) return;

            foreach (Item item in Pending)
            {
                Delegate unityItem = item.Target == null
                    ? Delegate.CreateDelegate(UnityItemType, item.Method)
                    : Delegate.CreateDelegate(UnityItemType, item.Target, item.Method);

                list.Add(unityItem);
            }

            Pending.Clear();

            EditorApplication.update -= TryAdd;
        }
        #endregion
    }
}
#endif
