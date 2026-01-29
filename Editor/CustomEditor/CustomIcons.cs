#if UNITY_EDITOR
using SHUU.Utils.BaseScripts.ScriptableObjs.Audio;
using SHUU.Utils.InputSystem;
using SHUU.Utils.SettingsSytem;
using UnityEditor;
using UnityEngine;

namespace SHUU._Editor._CustomEditor
{
    [InitializeOnLoad]
    public static class CustomIcons
    {
        static CustomIcons() => EditorApplication.delayCall += ApplyIcons;



        private static void ApplyIcons()
        {
            SetIcon<InputBindingMap>("InputBindingMap_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/InputBindingMap_Icon.png");

            SetIcon<SettingsData>("SettingsData_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/SettingsData_Icon.png");

            SetIcon<SfxStorage>("SfxStorage_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/SfxStorage_Icon.png");
            SetIcon<MusicStorage>("MusicStorage_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/MusicStorage_Icon.png");


            EditorApplication.delayCall += EditorApplication.RepaintProjectWindow;
        }


        public static void SetIcon<T>(string name, string resourcePath) where T : ScriptableObject
        {
            Texture2D icon = Resources.Load<Texture2D>(name);
            if (!icon) icon = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath); 
            if (!icon)
            {
                Debug.LogError($"Icon not found in Resources at: {resourcePath}");
                return;
            }

            MonoScript script = MonoScript.FromScriptableObject(ScriptableObject.CreateInstance<T>());

            EditorGUIUtility.SetIconForObject(script, null);
            EditorGUIUtility.SetIconForObject(script, icon);
            EditorApplication.RepaintProjectWindow();
        }
    }
}
#endif
