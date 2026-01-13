using System.Collections.Generic;
using UnityEngine;

namespace SHUU._Editor._CustomEditor.InspectorCreation
{
    public enum EditorCreatable_ObjectNames
    {
        OnEveryScene,

        SplineCollider,

        AudioAmbienceZone_Collider,
        AudioAmbienceZone_Shape,
        AudioAmbienceZone_Path
    }

    
    //[CreateAssetMenu(fileName = "EditorCreatable_Objects", menuName = "SHUU/CustomEditor/EditorCreatable_Objects")]
    public class EditorCreatable_Objects : ScriptableObject
    {
        public List<GameObject> objects;




        public GameObject GetObject(EditorCreatable_ObjectNames name) => GetObject((int)name);

        public GameObject GetObject(int index) => objects[index];
    }
}
