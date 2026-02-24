/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class BindAttribute : Attribute
    {
        public KeyCode Key { get; private set; }
        public object[] Parameters { get; private set; }

        public BindAttribute(KeyCode key, params object[] parameters)
        {
            Key = key;
            Parameters = parameters;
        }
    }

    public class Debug_MethodBind : MonoBehaviour
    {
        [SerializeField] private bool active = true;

        private static readonly Dictionary<KeyCode, List<Action>> keyBindings = new();

        private void Awake()
        {
            if (!active) return;

            CacheBindings();
            EditorApplication.update += OnEditorUpdate;
        }

        private static void CacheBindings()
        {
            keyBindings.Clear();

#if UNITY_2023_1_OR_NEWER
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
#else
            var behaviours = FindObjectsOfType<MonoBehaviour>();
#endif

            foreach (var obj in behaviours)
            {
                var methods = obj.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes<BindAttribute>(true);

                    foreach (var attr in attributes)
                    {
                        if (attr.Key == KeyCode.None)
                            continue;

                        var parameters = attr.Parameters;
                        var methodParams = method.GetParameters();

                        if (parameters.Length != methodParams.Length)
                        {
                            Debug.LogWarning($"Method {method.Name} expects {methodParams.Length} arguments, but {parameters.Length} were provided.");
                            continue;
                        }

                        void ActionWrapper()
                        {
                            try
                            {
                                method.Invoke(obj, parameters);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to invoke {method.Name}: {e.Message}");
                            }
                        }

                        if (!keyBindings.ContainsKey(attr.Key))
                            keyBindings[attr.Key] = new List<Action>();

                        keyBindings[attr.Key].Add(ActionWrapper);
                    }
                }
            }
        }

        private static void OnEditorUpdate()
        {
            if (!Application.isPlaying) return;

            foreach (var pair in keyBindings)
            {
                if (Input.GetKeyDown(pair.Key))
                {
                    foreach (var action in pair.Value)
                        action.Invoke();
                }
            }
        }
    }
}
#endif
