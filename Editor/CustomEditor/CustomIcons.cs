#if UNITY_EDITOR
using SHUU.Utils.InputSystem;
using SHUU.Utils.SettingsSytem;
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor._CustomEditor
{
    [InitializeOnLoad]
    public static class CustomIcons
    {
        static CustomIcons()
        {
            SetIcon<InputBindingMap>("InputBindingMap_Icon");
            SetIcon<SettingsData>("SettingsData_Icon");
        }

        public static void SetIcon<T>(string resourcePath) where T : ScriptableObject
        {
            Texture2D icon = FindIcon(resourcePath);
            if (!icon)
            {
                Debug.LogError($"Icon not found in Resources at: {resourcePath}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(T).Name}");
            if (guids.Length == 0)
            {
                Debug.LogError($"MonoScript not found for type {typeof(T).Name}");
                return;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);

            EditorGUIUtility.SetIconForObject(script, icon);
        }

        private static Texture2D FindIcon(string iconName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Texture2D l:shuu-icon");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == iconName)
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            return null;
        }
    }
}
#endif
