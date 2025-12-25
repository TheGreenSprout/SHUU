/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SHUU.Utils.UI.Dialogue;
using System;

namespace SHUU._Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DialogueLineInstance))]
    public class DialogueLineInstance_Drawer : PropertyDrawer
    {
        private float lineH => EditorGUIUtility.singleLineHeight;
        private float pad => EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = 0;

            h += lineH + pad; // portrait
            h += lineH + pad; // sounds
            h += lineH + pad; // name
            h += lineH + pad; // dropdown

            var variablesProp = property.FindPropertyRelative("variables");
            if (variablesProp.managedReferenceValue != null)
            {
                h += EditorGUI.GetPropertyHeight(variablesProp, true) + pad;
            }

            // NEW: add height for the UnityEvent
            var evtProp = property.FindPropertyRelative("endLineEvent");
            if (evtProp != null)
            {
                h += EditorGUI.GetPropertyHeight(evtProp, true) + pad;
            }

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float y = position.y;

            // Basic fields
            DrawProp(property, "characterPortrait", ref y, position);
            DrawProp(property, "textSounds", ref y, position);
            DrawProp(property, "characterName", ref y, position);

            // --------------------
            // VARIABLE TYPE PICKER
            // --------------------

            var variablesProp = property.FindPropertyRelative("variables");

            Type[] types =
            {
                null,
                typeof(DialogueLineInstance.DialogueLine_Text),
                typeof(DialogueLineInstance.DialogueLine_Options),
            };

            string[] names = { "None", "Text Line", "Options Line" };

            int currentIdx = 0;
            if (variablesProp.managedReferenceValue != null)
            {
                Type t = variablesProp.managedReferenceValue.GetType();
                currentIdx = Array.FindIndex(types, x => x == t);
                if (currentIdx < 0) currentIdx = 0;
            }

            Rect dropRect = new Rect(position.x, y, position.width, lineH);
            int selectedIdx = EditorGUI.Popup(dropRect, "Line Type", currentIdx, names);
            y += lineH + pad;

            if (selectedIdx != currentIdx)
            {
                if (types[selectedIdx] == null)
                    variablesProp.managedReferenceValue = null;
                else
                    variablesProp.managedReferenceValue = Activator.CreateInstance(types[selectedIdx]);

                property.serializedObject.ApplyModifiedProperties();
            }

            if (variablesProp.managedReferenceValue != null)
            {
                float h = EditorGUI.GetPropertyHeight(variablesProp, true);
                Rect r = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(r, variablesProp, true);
                y += h + pad;
            }

            // --------------------------
            // DRAW THE UNITYEVENT FIELD
            // --------------------------

            var evtProp = property.FindPropertyRelative("endLineEvent");
            if (evtProp != null)
            {
                float h = EditorGUI.GetPropertyHeight(evtProp, true);
                Rect r = new Rect(position.x, y, position.width, h);
                EditorGUI.PropertyField(r, evtProp, true);
                y += h + pad;
            }

            EditorGUI.EndProperty();
        }

        private void DrawProp(SerializedProperty parent, string name, ref float y, Rect total)
        {
            SerializedProperty p = parent.FindPropertyRelative(name);
            Rect r = new Rect(total.x, y, total.width, lineH);
            EditorGUI.PropertyField(r, p, true);
            y += lineH + pad;
        }
    }
}
#endif
