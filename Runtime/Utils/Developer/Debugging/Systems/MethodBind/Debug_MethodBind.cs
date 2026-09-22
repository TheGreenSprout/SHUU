/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using SHUU.Utils.Globals;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    public class Debug_MethodBind : MonoBehaviour
    {
        #region Variables
        private bool active => SHUU_Debug.Instance.methodBind_enabled;


        private static readonly Dictionary<KeyCode, List<Action>> KeyBindings = new();
        #endregion




        #region Main
        private void Awake()
        {
            if (!active) return;

            SHUU_Time.OnNextFrame += CacheBindings;
        }


        private void Update()
        {
            if (!Application.isPlaying) return;


            foreach (var pair in KeyBindings)
            {
                if (Input.GetKeyDown(pair.Key))
                {
                    foreach (var action in pair.Value)
                        action.Invoke();
                }
            }
        }
        #endregion



        #region Logic
        private static void CacheBindings()
        {
            KeyBindings.Clear();

#if UNITY_2023_1_OR_NEWER
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
#else
            var behaviours = FindObjectsOfType<MonoBehaviour>();
#endif

            foreach (var obj in behaviours)
            {
                var methods = obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes<BindAttribute>(true);

                    foreach (var attr in attributes)
                    {
                        if (attr.Key == KeyCode.None) continue;

                        var parameters = attr.Parameters;
                        var methodParams = method.GetParameters();

                        if (parameters.Length != methodParams.Length)
                        {
                            Debug.LogWarning($"Method {method.Name} expects {methodParams.Length} arguments, but {parameters.Length} were provided.");
                            continue;
                        }

                        void ActionWrapper()
                        {
                            try { method.Invoke(obj, parameters); }
                            catch (Exception e) { Debug.LogError($"Failed to invoke {method.Name}: {e.Message}"); }
                        }

                        if (!KeyBindings.ContainsKey(attr.Key)) KeyBindings[attr.Key] = new List<Action>();

                        KeyBindings[attr.Key].Add(ActionWrapper);
                    }
                }
            }
        }
        #endregion
    }




    #region Attribute class
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
    #endregion
}
#endif
