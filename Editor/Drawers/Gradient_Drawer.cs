/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using SHUU.Utils.UI;
using UnityEngine;
using UnityEditor;

namespace SHUU.Editor
{
    [CustomPropertyDrawer(typeof(SHUU_Gradient.GradientType), true)]
    public class Gradient_Drawer : PropertyDrawer
    {
        private static readonly string[] typeNames = new[]
        {
            "None",
            "UI Gradient",
            "UI Corners Gradient",
            "UI Text Gradient",
            "UI Text Corners Gradient"
        };

        private static readonly Type[] types = new Type[]
        {
            null,
            typeof(SHUU_Gradient.UIGradient),
            typeof(SHUU_Gradient.UICornersGradient),
            typeof(SHUU_Gradient.UITextGradient),
            typeof(SHUU_Gradient.UITextCornersGradient)
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // figure out current index
            int currentIndex = 0;
            if (property.managedReferenceValue != null)
            {
                var currentType = property.managedReferenceValue.GetType();
                currentIndex = Array.FindIndex(types, t => t == currentType);
                if (currentIndex < 0) currentIndex = 0;
            }

            // dropdown
            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int selectedIndex = EditorGUI.Popup(dropdownRect, "Gradient Type", currentIndex, typeNames);

            // if type changed -> replace instance
            if (selectedIndex != currentIndex)
            {
                if (types[selectedIndex] == null)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    property.managedReferenceValue = Activator.CreateInstance(types[selectedIndex]);
                }
            }

            // draw sub-properties (child fields of chosen gradient type)
            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                var iterator = property.Copy();
                var end = iterator.GetEndProperty();

                // Move into children
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(iterator, end)) break;
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    while (iterator.NextVisible(false));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight; // dropdown height
        }
    }
}
#endif