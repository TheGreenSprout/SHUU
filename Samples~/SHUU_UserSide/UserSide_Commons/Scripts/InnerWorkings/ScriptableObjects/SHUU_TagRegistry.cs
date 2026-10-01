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

        public static SHUU_TagRegistry Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<SHUU_TagRegistry>("SHUUResources/SHUU_TagRegistry");

                return _instance;
            }
        }
        #endregion



        public static List<string> TagRegistry => Instance._tagRegistry;
        [SerializeField] private List<string> _tagRegistry;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;


        private void OnEnable()
        {
            if (_instance != null && _instance != this)
            {
#if UNITY_EDITOR
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_TagRegistry)}.\nRecorded instance: {AssetDatabase.GetAssetPath(_instance)}\nRepeated instance:{AssetDatabase.GetAssetPath(this)}\nDestroying newest instance...");
#else
                Debug.LogError($"Multiple instances of Singleton (ScriptableObject); type: {typeof(SHUU_TagRegistry)}.\nDestroying newest instance...");
#endif

                DestroyImmediate(this);
                return;
            }

            _instance = this;


#if UNITY_EDITOR
            RefreshTags();
#endif
        }
        #endregion



#if UNITY_EDITOR
        #region Editor
        private void RefreshTags() => _tagRegistry = new(UnityEditorInternal.InternalEditorUtility.tags);

        [MenuItem("Tools/Sprout's Handy Unity Utils/Quick Tools/Reload Tag Registry")]
        private static void ReloadTagRegistry()
        {
            if (Instance == null)
            {
                Debug.LogWarning("No SHUU_TagRegistry asset found (expected at Resources/SHUUResources/SHUU_TagRegistry).");

                return;
            }

            Instance.RefreshTags();
            EditorUtility.SetDirty(Instance);

            Debug.Log("SHUU_TagRegistry reloaded from the project's tags.");
        }
        #endregion
#endif
    }
}
