/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



/*#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SHUU._CustomEditor;
using System;
using System.Reflection;
using SHUU.Utils.Developer.Console;

namespace SHUU._Editor._CustomEditor
{
    [CustomEditor(typeof(DevConsoleInput), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class BlockInspectorToggleDrawer : Editor
    {
        protected override void OnHeaderGUI()
        {
            bool locked = ShouldBlockInspectorToggle(target.GetType());

            if (locked)
            {
                // Begin disabled group to grey out the checkbox
                EditorGUI.BeginDisabledGroup(true);
                base.OnHeaderGUI(); // draws header including the Enabled checkbox
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                base.OnHeaderGUI();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); // normal inspector
        }

        private static bool ShouldBlockInspectorToggle(Type type)
        {
            Type current = type;

            while (current != null && current != typeof(MonoBehaviour))
            {
                var ownAttr = current.GetCustomAttribute<BlockInspectorToggleAttribute>(false);
                if (ownAttr != null)
                    return true;

                var baseType = current.BaseType;
                if (baseType == null)
                    break;

                var baseAttr = baseType.GetCustomAttribute<BlockInspectorToggleAttribute>(false);
                if (baseAttr != null)
                    return baseAttr.inheritable;

                current = baseType;
            }

            return false;
        }
    }
}
#endif*/
