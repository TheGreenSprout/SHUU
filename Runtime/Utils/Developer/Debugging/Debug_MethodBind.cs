/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Reflection;
using SHUU.Utils.Helpers;
using UnityEditor;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class BindAttribute : Attribute
    {
        public KeyCode Key { get; private set; }
        public object[] Parameters { get; private set; }

        public BindAttribute(string key, params object[] parameters)
        {
            (KeyCode? k, int? m, string s) parse = InputParser.ParseInput(key);

            if (parse.k == null)
            {
                Debug.LogWarning($"Invalid key '{key}' in BindAttribute. Defaulting to KeyCode.None.");

                Key = KeyCode.None;
            }
            else Key = parse.k.Value;

            Parameters = parameters;
        }
    }



    [InitializeOnLoad]
    public static class Debug_MethodBind
    {
        static Debug_MethodBind()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (!Application.isPlaying) return;


            MonoBehaviour[] behaviours;
            #if UNITY_2023_1_OR_NEWER
            behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            #else
            behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            #endif
            foreach (var obj in behaviours)
            {
                var type = obj.GetType();
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(BindAttribute), true);
                    foreach (BindAttribute attr in attributes)
                    {
                        if (Input.GetKeyDown(attr.Key))
                        {
                            var parameters = attr.Parameters;

                            var methodParams = method.GetParameters();
                            if (parameters.Length != methodParams.Length)
                            {
                                Debug.LogWarning($"Method {method.Name} expects {methodParams.Length} arguments, but {parameters.Length} were provided.");
                                continue;
                            }

                            try
                            {
                                method.Invoke(obj, parameters);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to invoke {method.Name}: {e.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif
