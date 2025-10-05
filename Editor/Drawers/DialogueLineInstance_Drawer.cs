/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace SHUU.Editor
{
    [CustomPropertyDrawer(typeof(DialogueLineInstance))]
    public class DialogueLineInstance_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var variablesProp = property.FindPropertyRelative("variables");
            var typeNames = new[] { "None", "Text Line", "Options Line" };
            var types = new System.Type[] { null, typeof(DialogueLineInstance.DialogueLine_Text), typeof(DialogueLineInstance.DialogueLine_Options) };

            int currentIndex = 0;
            if (variablesProp.managedReferenceValue != null)
            {
                var currentType = variablesProp.managedReferenceValue.GetType();
                currentIndex = Array.FindIndex(types, t => t == currentType);
                if (currentIndex < 0) currentIndex = 0;
            }

            Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int selectedIndex = EditorGUI.Popup(dropdownRect, "Line Type", currentIndex, typeNames);

            if (selectedIndex != currentIndex)
            {
                if (types[selectedIndex] == null)
                {
                    variablesProp.managedReferenceValue = null;
                }
                else
                {
                    variablesProp.managedReferenceValue = Activator.CreateInstance(types[selectedIndex]);
                }
            }

            if (variablesProp.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(variablesProp, true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
