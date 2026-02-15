/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using SHUU.Utils.BaseScripts.ScriptableObjs.Audio;
using SHUU.Utils.InputSystem;
using SHUU.Utils.RandomSystem;
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
            EditorApplication.projectChanged += ApplyIcons;
            AssemblyReloadEvents.afterAssemblyReload += ApplyIcons;
            EditorApplication.delayCall += ApplyIcons;
        }



        private static void ApplyIcons()
        {
            SetIcon<InputBindingMap>("InputBindingMap_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/InputBindingMap_Icon.png");

            SetIcon<SettingsData>("SettingsData_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/SettingsData_Icon.png");

            SetIcon<Sfx_Storage>("SfxStorage_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/SfxStorage_Icon.png");
            SetIcon<Music_Storage>("MusicStorage_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/MusicStorage_Icon.png");

            SetIcon<RandomProvider_Asset>("RandomProvider_Icon", "Packages/com.sproutinggames.sprouts.huu/Editor/Resources/RandomProvider_Icon.png");


            EditorApplication.RepaintProjectWindow();
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

            
            MonoScript script = null;

            #if UNITY_6000_0_OR_NEWER

            string[] guids = AssetDatabase.FindAssets("t:MonoScript");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript candidate = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (candidate == null)
                    continue;

                System.Type candidateType = candidate.GetClass();

                if (candidateType == typeof(T))
                {
                    script = candidate;
                    break;
                }
            }

            #elif UNITY_2019_2_OR_NEWER
            script = MonoScript.FromScriptType(typeof(T));

            #else
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(T).Name}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript candidate = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (candidate != null && candidate.GetClass() == typeof(T))
                {
                    script = candidate;

                    break;
                }
            }
            #endif


            if (script == null)
            {
                Debug.LogError($"Could not find MonoScript for type {typeof(T).Name}");

                return;
            }

            EditorGUIUtility.SetIconForObject(script, icon);
            EditorApplication.RepaintProjectWindow();
        }
    }
}
#endif
