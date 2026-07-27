using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects
{
    //[CreateAssetMenu(fileName = "SHUU_TagRegistry", menuName = "Scriptable Objects/SHUU_TagRegistry")]
    public class SHUU_TagRegistry : ScriptableObject
    {
        #region Variables

        #region Singleton
        private static SHUU_TagRegistry _instance;

        public static SHUU_TagRegistry instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<SHUU_TagRegistry>("SHUU/InnerWorkings/SHUU_TagRegistry");

                return _instance;
            }
        }
        #endregion



        public static List<string> tagRegistry => instance._tagRegistry;
        [SerializeField] private List<string> _tagRegistry;
        #endregion




        #region Main
        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
#if UNITY_EDITOR
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_Preferences)}.\nRecorded instance: {AssetDatabase.GetAssetPath(_instance)}\nRepeated instance:{AssetDatabase.GetAssetPath(this)}\nDestroying newest instance...");
#else
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_Preferences)}.\nDestroying newest instance...");
#endif

                DestroyImmediate(this);
                return;
            }

            _instance = this;


#if UNITY_EDITOR
            _tagRegistry = new(UnityEditorInternal.InternalEditorUtility.tags);
#endif
        }
        #endregion
    }
}
