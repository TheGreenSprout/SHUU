/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using SHUU.Utils.Helpers;
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor.Drawers
{
    [CustomEditor(typeof(SplineCollider))]
    public class SplineCollider_Drawer : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SplineCollider sc = (SplineCollider)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Rebuild Collider Mesh"))
            {
                sc.Rebuild();
            }
        }
    }
}
#endif
